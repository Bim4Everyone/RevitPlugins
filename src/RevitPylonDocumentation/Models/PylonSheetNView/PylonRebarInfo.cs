using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;

namespace RevitPylonDocumentation.Models.PylonSheetNView;
internal class PylonRebarInfo {
    private readonly CreationSettings _settings;
    private readonly PylonSheetInfo _sheetInfo;
    private readonly RevitRepository _repository; 
    
    private readonly string _hasFirstLRebarParamName = "ст_Г_1_ВКЛ";
    private readonly string _hasSecondLRebarParamName = "ст_Г_2_ВКЛ";
    private readonly string _hasDifferentRebarParamName = "ст_РАЗНЫЕ";

    private readonly int _formNumberForCBarMax = 1202;
    private readonly int _formNumberForCBarMin = 1202;

    private readonly int _formNumberForVerticalRebarMin = 1101;
    private readonly int _formNumberForVerticalRebarMax = 1499;

    private readonly int _formNumberForClampsMin = 1500;
    private readonly int _formNumberForClampsMax = 1599;

    internal PylonRebarInfo(CreationSettings settings, RevitRepository repository, PylonSheetInfo pylonSheetInfo) {
        _settings = settings;
        _sheetInfo = pylonSheetInfo;
        _repository = repository;

        SkeletonParentRebar = _sheetInfo.RebarFinder.GetSkeletonParentRebar(_sheetInfo.ProjectSection, _sheetInfo.PylonKeyName);
        GetInfo();

        SimpleVerticalRebars =
            _sheetInfo.RebarFinder.GetSimpleRebars(_sheetInfo.ProjectSection, _sheetInfo.PylonKeyName,
                                                  _formNumberForVerticalRebarMin, _formNumberForVerticalRebarMax,
                                                  _formNumberForCBarMin, _formNumberForCBarMax);

        SimpleClamps = _sheetInfo.RebarFinder.GetSimpleRebars(_sheetInfo.ProjectSection, _sheetInfo.PylonKeyName,
                                                             _formNumberForClampsMin, _formNumberForClampsMax);
    }

    internal FamilyInstance SkeletonParentRebar { get; set; }
    internal List<Element> SimpleVerticalRebars { get; set; }
    internal List<Element> SimpleClamps { get; set; }
    internal bool FirstLRebarParamValue { get; set; } = false;
    internal bool SecondLRebarParamValue { get; set; } = false;
    internal bool DifferentRebarParamValue { get; set; } = false;
    internal bool AllRebarAreL { get; set; } = false;
    internal bool HasLRebar { get; set; } = false;
    internal bool SkeletonParentRebarForParking { get; set; }



    private void GetInfo() {
        if(SkeletonParentRebar is null) { return; }
        
        // Определяем наличие в каркасе Г-образных стержней
        FirstLRebarParamValue = _sheetInfo.ParamValService
            .GetParamValueAnywhere<int>(SkeletonParentRebar, _hasFirstLRebarParamName) == 1;
        SecondLRebarParamValue = _sheetInfo.ParamValService
            .GetParamValueAnywhere<int>(SkeletonParentRebar, _hasSecondLRebarParamName) == 1;
        DifferentRebarParamValue = _sheetInfo.ParamValService
            .GetParamValueAnywhere<int>(SkeletonParentRebar, _hasDifferentRebarParamName) == 1;

        AllRebarAreL = FirstLRebarParamValue && SecondLRebarParamValue;
        HasLRebar = FirstLRebarParamValue || SecondLRebarParamValue;

        SkeletonParentRebarForParking = SkeletonParentRebar.Symbol.FamilyName.Contains("Паркинг");
    }

    /// <summary>
    /// Заполняет параметр позиции арматурных стержней
    /// </summary>
    internal void TrySetSimpleRebarMarks() {
        try {
            // Получаем все стержни, которые с учетом заполненных параметров относятся к пилону
            var allSimpleRebars =
                _sheetInfo.RebarFinder.GetSimpleRebars(_sheetInfo.ProjectSection, _sheetInfo.PylonKeyName,
                                                      _formNumberForVerticalRebarMin, _formNumberForClampsMax);
            // Необходимо выполнить двухуровневую группировку - 
            // 1й уровень - по значению параметра "обр_ФОП_Форма_префикс"
            // 2й уровень - по значениям параметров "мод_ФОП_Диаметр" и "обр_ФОП_Длина" одновременно
            // - Префикс 1
            // -- Диаметр 1 | Длина 1 => Марка = 1
            // -- Диаметр 1 | Длина 2 => Марка = 2
            // -- Диаметр 2 | Длина 1 => Марка = 3
            // - Префикс 2
            // -- Диаметр 1 | Длина 1 => Марка = 1
            // -- Диаметр 1 | Длина 2 => Марка = 2
            var query = allSimpleRebars
                .GroupBy(r => _sheetInfo.ParamValService.GetParamValueAnywhere<string>(r, "обр_ФОП_Форма_префикс"))
                .Select(prefixGroup => new {
                    Prefix = prefixGroup.Key,
                    SubGroups = prefixGroup
                        .GroupBy(r => new {
                            Diameter = _sheetInfo.ParamValService.GetParamValueAnywhere<double>(r, "мод_ФОП_Диаметр"),
                            Length = _sheetInfo.ParamValService.GetParamValueAnywhere<double>(r, "обр_ФОП_Длина")
                        })
                        .Select((group, index) => new {
                            Group = group,
                            MarkNumber = index + 1
                        })
                        .ToList()
                });
            // Выполняем запись в параметр арматуры, который отвечает за позицию
            foreach(var prefixGroup in query) {
                foreach(var subGroup in prefixGroup.SubGroups) {
                    string markValue = subGroup.MarkNumber.ToString();
                    foreach(var element in subGroup.Group) {
                        element.SetParamValue("обр_ФОП_Позиция", markValue);
                    }
                }
            }
        } catch(System.Exception) { }
    }
}
