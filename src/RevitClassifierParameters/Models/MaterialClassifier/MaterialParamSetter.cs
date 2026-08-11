using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.SimpleServices;

using RevitClassifierParameters.Models.Work;

namespace RevitClassifierParameters.Models.MaterialClassifier;

internal class MaterialParamSetter {
    private const string _chapterParameter = "ФОП_МТР_Наименование главы";
    private const string _workTitleParameter = "ФОП_МТР_Наименование работы";
    private const string _unitParameter = "ФОП_МТР_Единица измерения";
    private const string _calculationTypeParameter = "ФОП_МТР_Тип подсчета";

    private const string _calculationTypeError = "Ошибка";

    private static readonly Dictionary<string, int> _calculationTypeDict = new() {
        ["м"] = 1,
        ["м²"] = 2,
        ["м³"] = 3,
        ["шт."] = 4
    };

    private readonly RevitRepository _revitRepository;
    private readonly MaterialReportService _materialReportService;
    private readonly ILocalizationService _localizationService;

    public MaterialParamSetter(
        RevitRepository revitRepository,
        MaterialReportService materialReportService,
        ILocalizationService localizationService) {
        _revitRepository = revitRepository;
        _materialReportService = materialReportService;
        _localizationService = localizationService;
    }

    /// <summary>
    /// Задает значения параметрам материалов 
    /// </summary>
    /// <param name="activeCodes">Выбранные пользователем коды для обработки</param>
    /// <param name="classifierWorks">Работы из классификатора</param>
    /// <param name="materialInPj">Материалы в проекте</param>
    /// <param name="forAr">Работа с архитектурными группами классификатора</param>
    public void SetParamValue(
        HashSet<string> activeCodes,
        List<WorkGroup> classifierWorks,
        List<Material> materialInPj,
        bool forAr) {

        // Сопоставляем материалы с работами Классификатора
        var revitMaterials = MatchMaterials(activeCodes, materialInPj, classifierWorks);

        SetClassifierParameters(revitMaterials, forAr);
    }

    /// <summary>
    /// Сопоставляет материалы проекта с работами Классификатора по ключевой заметке.
    /// Обрабатываются только материалы, чей код начинается с одного из выбранных кодов групп.
    /// </summary>
    private List<RevitMaterial> MatchMaterials(
        HashSet<string> activeCodes,
        List<Material> materialInPj,
        List<WorkGroup> classifierWorks) {

        var revitMaterials = new List<RevitMaterial>();

        foreach(var material in materialInPj) {
            string keynote = material.GetParamValueOrDefault<string>(BuiltInParameter.KEYNOTE_PARAM);

            // Отсеиваем ситуации, когда у материала не указана Ключевая заметка (код работы)
            if(string.IsNullOrEmpty(keynote)) {
                _materialReportService.Add(MaterialReportStatus.NoWorkCode, string.Empty, material.Name);
                continue;
            }

            // Обрабатываем только материалы, чей код принадлежит одному из выбранных кодов групп
            if(!activeCodes.Any(code => CodeStartsWith(keynote, code))) {
                continue;
            }

            // Отсеиваем ситуации, когда Классификатор не содержит указанный в материале код
            var work = FindWork(classifierWorks, keynote);
            if(work is null) {
                _materialReportService.Add(MaterialReportStatus.ClassifierCodeNotFound, keynote, material.Name);
                continue;
            }

            revitMaterials.Add(new RevitMaterial(keynote, material, work));
        }

        return revitMaterials;
    }

    /// <summary>
    /// Ищет работу по коду, спускаясь по дереву классификатора.
    /// Использует факт, что код работы включает код своей группы как префикс,
    /// поэтому заходит только в ту ветку, чей код группы является префиксом искомого кода.
    /// </summary>
    private Work.Work FindWork(List<WorkGroup> workGroups, string code) {
        if(workGroups is null) {
            return null;
        }

        foreach(var workGroup in workGroups) {
            // Код работы всегда включает код группы как префикс,
            // поэтому неподходящие ветки пропускаем целиком
            if(!CodeStartsWith(code, workGroup.Code)) {
                continue;
            }

            var work = workGroup.ChildWorks.FirstOrDefault(w => w.Code == code);
            if(work != null) {
                return work;
            }

            var found = FindWork(workGroup.ChildWorkGroups, code);
            if(found != null) {
                return found;
            }
        }

        return null;
    }

    /// <summary>
    /// Проверяет, что код совпадает с префиксом или является его дочерним кодом
    /// (совпадение по границе сегмента, разделённого точкой).
    /// Пример: для префикса "г02.03" -> true для "г02.03" и "г02.03.01", false для "г02.030".
    /// </summary>
    private bool CodeStartsWith(string code, string prefix) {
        return code == prefix || code.StartsWith(prefix + ".");
    }

    /// <summary>
    /// Заполняет параметры Классификатора у материалов в одной транзакции.
    /// </summary>
    private void SetClassifierParameters(List<RevitMaterial> revitMaterials, bool forAr) {
        using var transaction = _revitRepository.Document.StartTransaction(
            _localizationService.GetLocalizedString("Transaction.SetClassifierParameters"));
        foreach(var revitMaterial in revitMaterials) {
            var material = revitMaterial.Material;
            try {
                bool edited = false;
                var work = revitMaterial.Work;
                string chapterName = GetChapterName(work, forAr);

                edited |= SetStringParam(material, _chapterParameter, chapterName);
                edited |= SetStringParam(material, _workTitleParameter, work.Name);
                edited |= SetStringParam(material, _unitParameter, work.Unit);
                edited |= SetCalculationTypeParam(material, work.Unit);
                edited |= SetStringParam(
                    material,
                    BuiltInParameter.ALL_MODEL_DESCRIPTION,
                    revitMaterial.MaterialDescription);

                _materialReportService.Add(
                    edited ? MaterialReportStatus.Edited : MaterialReportStatus.NotEdited,
                    revitMaterial.Keynote,
                    material.Name);
            } catch {
                _materialReportService.Add(MaterialReportStatus.Error, revitMaterial.Keynote, material.Name);
            }
        }

        transaction.Commit();
    }

    /// <summary>
    /// Возвращает наименование главы для материала.
    /// forAr = true  -> наименование ближайшей главы (непосредственный родитель работы).
    /// forAr = false -> наименование главы самого верхнего уровня
    /// (поднимаемся по ParentWorkGroup, пока он не станет null).
    /// </summary>
    private string GetChapterName(Work.Work work, bool forAr) {
        var chapter = work.ParentWorkGroup;
        if(chapter is null) {
            return string.Empty;
        }

        if(forAr) {
            return chapter.Name;
        }

        while(chapter.ParentWorkGroup != null) {
            chapter = chapter.ParentWorkGroup;
        }
        return chapter.Name;
    }

    /// <summary>
    /// Устанавливает значение параметра "Тип подсчета" по единице измерения.
    /// При валидной единице пишется число, иначе - строка "Ошибка".
    /// Возвращает true, если параметр был изменён.
    /// </summary>
    private bool SetCalculationTypeParam(Material material, string unit) {
        var param = material.GetParam(_calculationTypeParameter);

        if(unit != null && _calculationTypeDict.TryGetValue(unit, out int value)) {
            if(param.AsValueString() == value.ToString()) {
                return false;
            }
            material.SetParamValue(_calculationTypeParameter, value);
            return true;
        }

        if(param.AsValueString() == _calculationTypeError) {
            return false;
        }
        material.SetParamValue(_calculationTypeParameter, _calculationTypeError);
        return true;
    }

    /// <summary>
    /// Устанавливает строковое значение параметра, только если оно отличается от текущего.
    /// Возвращает true, если параметр был изменён.
    /// </summary>
    private bool SetStringParam(Material material, string paramName, string value) {
        var param = material.GetParam(paramName);
        if(param.AsValueString() == value) {
            return false;
        }
        material.SetParamValue(paramName, value ?? string.Empty);
        return true;
    }

    /// <summary>
    /// Устанавливает строковое значение системного параметра, только если оно отличается от текущего.
    /// Возвращает true, если параметр был изменён.
    /// </summary>
    private bool SetStringParam(Material material, BuiltInParameter builtInParameter, string value) {
        var param = material.GetParam(builtInParameter);
        if(param.AsValueString() == value) {
            return false;
        }
        material.SetParamValue(builtInParameter, value ?? string.Empty);
        return true;
    }
}
