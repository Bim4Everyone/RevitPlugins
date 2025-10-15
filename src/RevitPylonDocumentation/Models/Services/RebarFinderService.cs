using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitPylonDocumentation.Models.PluginOptions;

namespace RevitPylonDocumentation.Models.Services;
public class RebarFinderService {
    private readonly CreationSettings _settings;
    private readonly RevitRepository _repository;

    // Компаратор для сравнения элементов по Id
    private readonly ElementComparer _comparerOfElements;

    private readonly ParamValueService _paramValueService;
    private readonly string _formNumberParamName = "обр_ФОП_Форма_номер";
    private readonly string _baseMarkParamName = "обр_ФОП_Метка основы IFC";

    internal RebarFinderService(CreationSettings settings, RevitRepository repository,
                                ParamValueService paramValueService) {
        _settings = settings;
        _repository = repository;
        _paramValueService = paramValueService;
        _comparerOfElements = new ElementComparer();
    }


    private bool CheckRebarByBaseParams(Element rebar, string projectSection, string pylonKeyName) {
        // Фильтрация по комплекту документации
        if(rebar.GetParamValue<string>(_settings.ProjectSettings.ProjectSection) != projectSection) {
            return false;
        }
        // Фильтрация по марке пилона
        if(rebar.GetParamValue<string>(_baseMarkParamName) != pylonKeyName) {
            return false;
        }
        // Фильтрация по обр_ФОП_Фильтрации 1
        if(rebar.GetParamValue<string>(_settings.ProjectSettings.TypicalPylonFilterParameter) !=
            _settings.ProjectSettings.TypicalPylonFilterValue) {
            return false;
        }
        return true;
    }

    /// <summary>
    /// Получение родительского элемента сварного арматурного каркаса (верт стержни + пластины)
    /// </summary>
    public FamilyInstance GetSkeletonParentRebar(string projectSection, string pylonKeyName) {
        var rebars = new FilteredElementCollector(_repository.Document)
            .OfCategory(BuiltInCategory.OST_Rebar)
            .WhereElementIsNotElementType()
            .ToElements();

        foreach(var rebar in rebars) {
            // Фильтрация по базовым параметрам (комплект, марка, фильтрация 1)
            if(!CheckRebarByBaseParams(rebar, projectSection, pylonKeyName)) {
                continue;
            }
            // Фильтрация по имени семейства
            if(_repository.Document.GetElement(rebar.GetTypeId()) is not FamilySymbol rebarType) {
                continue;
            }
            if(rebarType.FamilyName.Equals("IFC_Пилон_Верт.Арм.") || rebarType.FamilyName.Contains("IFC_Каркас_Пилон")) {
                return rebar as FamilyInstance;
            }
        }
        return null;
    }

    /// <summary>
    /// Получение арматурных стержней с параметры по пилону (комплект, марка, фильтрация 1) с нужными номерами форм.
    /// Номера арматурных форм подаются граничными значениями диапазонов
    /// </summary>
    /// <param name="projectSection">Комплект документации</param>
    /// <param name="pylonKeyName">Марка пилона</param>
    /// <param name="formNumberMin">Минимальный номер формы арматурного стержня, который нужно найти</param>
    /// <param name="formNumberMax">Максимальный номер формы арматурного стержня, который нужно найти</param>
    /// <param name="formNumberMinException">Минимальный номер формы арматурного стержня, который нужно исключить</param>
    /// <param name="formNumberMaxException">Максимальный номер формы арматурного стержня, который нужно исключить</param>
    /// <returns></returns>
    public List<Element> GetSimpleRebars(string projectSection, string pylonKeyName, 
                                         int formNumberMin, int formNumberMax,
                                         int formNumberMinException = int.MinValue, 
                                         int formNumberMaxException = int.MinValue) {
        var rebars = new FilteredElementCollector(_repository.Document)
            .OfCategory(BuiltInCategory.OST_Rebar)
            .WhereElementIsNotElementType()
            .ToElements();

        var simpleRebars = new List<Element>();
        foreach(var rebar in rebars) {
            // Фильтрация по базовым параметрам (комплект, марка, фильтрация 1)
            if(!CheckRebarByBaseParams(rebar, projectSection, pylonKeyName)) {
                continue;
            }
            // Фильтрация по номеру формы
            // Номер формы должен попадать в диапазон нужных форм и не попадать в диапазон форм для отсева
            int formNumber = _paramValueService.GetParamValueAnywhere<int>(rebar, _formNumberParamName);
            bool includeInNeededRange = formNumber >= formNumberMin && formNumber <= formNumberMax;
            bool includeInExceptionRange = formNumber >= formNumberMinException && formNumber <= formNumberMaxException;

            if(includeInNeededRange && !includeInExceptionRange) {
                simpleRebars.Add(rebar);
            }
        }
        return simpleRebars;
    }

    public List<FamilyInstance> GetClampsParentRebars(View view, string projectSection) {
        var rebars = new FilteredElementCollector(view.Document, view.Id)
            .OfCategory(BuiltInCategory.OST_Rebar)
            .WhereElementIsNotElementType()
            .ToElements();

        var parentRebars = new List<FamilyInstance>();

        foreach(var rebar in rebars) {
            // Фильтрация по комплекту документации
            if(rebar.GetParamValue<string>(_settings.ProjectSettings.ProjectSection) != projectSection) {
                continue;
            }
            // Фильтрация по имени семейства
            if(view.Document.GetElement(rebar.GetTypeId()) is not FamilySymbol rebarType) {
                continue;
            }
            if(rebarType.FamilyName.Contains("IFC_Набор_Пилон") && rebarType.FamilyName.Contains("Хомуты")) {
                parentRebars.Add(rebar as FamilyInstance);
            }
        }
        return parentRebars;
    }

    public List<Element> GetSimpleRebars(View view, string projectSection, int formNumberMin, int formNumberMax) {
        var rebars = new FilteredElementCollector(view.Document, view.Id)
            .OfCategory(BuiltInCategory.OST_Rebar)
            .WhereElementIsNotElementType()
            .ToElements();

        var simpleRebars = new List<Element>();
        foreach(var rebar in rebars) {
            // Фильтрация по комплекту документации
            if(rebar.GetParamValue<string>(_settings.ProjectSettings.ProjectSection) != projectSection) {
                continue;
            }
            // Фильтрация по номеру формы - отсев вертикальных стержней армирования
            int formNumber = _paramValueService.GetParamValueAnywhere<int>(rebar, _formNumberParamName);
            if(formNumber >= formNumberMin && formNumber <= formNumberMax) {
                simpleRebars.Add(rebar);
            }
        }
        return simpleRebars;
    }


    public List<Element> GetSimpleRebars(View view, string projectSection, int formNumberMin, int formNumberMax,
                                         int formNumberMinException, int formNumberMaxException) {
        var rebars = new FilteredElementCollector(view.Document, view.Id)
            .OfCategory(BuiltInCategory.OST_Rebar)
            .WhereElementIsNotElementType()
            .ToElements();

        var simpleRebars = new List<Element>();
        foreach(var rebar in rebars) {
            // Фильтрация по комплекту документации
            if(rebar.GetParamValue<string>(_settings.ProjectSettings.ProjectSection) != projectSection) {
                continue;
            }
            // Фильтрация по номеру формы - отсев вертикальных стержней армирования
            int formNumber = _paramValueService.GetParamValueAnywhere<int>(rebar, _formNumberParamName);
            bool includeInNeededRange = formNumber >= formNumberMin && formNumber <= formNumberMax;
            bool includeInExceptionRange = formNumber >= formNumberMinException && formNumber <= formNumberMaxException;

            if(includeInNeededRange && !includeInExceptionRange) {
                simpleRebars.Add(rebar);
            }
        }
        return simpleRebars;
    }


    public List<Element> GetSimpleRebars(View view, string projectSection, int neededFormNumber) {
        var rebars = new FilteredElementCollector(view.Document, view.Id)
            .OfCategory(BuiltInCategory.OST_Rebar)
            .WhereElementIsNotElementType()
            .ToElements();

        var simpleRebars = new List<Element>();
        foreach(var rebar in rebars) {
            // Фильтрация по комплекту документации
            if(rebar.GetParamValue<string>(_settings.ProjectSettings.ProjectSection) != projectSection) {
                continue;
            }
            // Фильтрация по номеру формы - отсев вертикальных стержней армирования
            int formNumber = _paramValueService.GetParamValueAnywhere<int>(rebar, _formNumberParamName);
            if(formNumber == neededFormNumber) {
                simpleRebars.Add(rebar);
            }
        }
        return simpleRebars;
    }


    /// <summary>
    /// Метод возвращает только те элементы, что видны на виде
    /// </summary>
    public List<Element> GetRebarsFromView(List<Element> rebars, View view) {
        var collector = new FilteredElementCollector(_repository.Document, view.Id)
            .WhereElementIsViewIndependent()
            .ToElements();
        return collector.Intersect(rebars, _comparerOfElements).ToList();
    }


    /// <summary>
    /// Определение с какой стороны относительно вида находится Г-образный стержень
    /// </summary>
    public bool DirectionHasLRebar(View view, string projectSection, DirectionType directionType) {
        // Г-образный стержень
        var lRebar = GetSimpleRebars(view, projectSection, 1101).FirstOrDefault();
        // Бутылка
        var bottleRebar = GetSimpleRebars(view, projectSection, 1204).FirstOrDefault();

        if(lRebar is null || bottleRebar is null) {
            return false;
        }

        if(lRebar.Location is not LocationPoint lRebarLocation) { return false; }
        var lRebarPt = lRebarLocation.Point;

        if(bottleRebar.Location is not LocationPoint bottleRebarLocation) { return false; }
        var bottleRebarPt = bottleRebarLocation.Point;

        var transform = view.CropBox.Transform;
        var inverseTransform = transform.Inverse;
        // Получаем координаты точек вставки в координатах вида
        var lRebarPtTransformed = inverseTransform.OfPoint(lRebarPt);
        var bottleRebarPtTransformed = inverseTransform.OfPoint(bottleRebarPt);

        return directionType switch {
            DirectionType.Top => lRebarPtTransformed.Y > bottleRebarPtTransformed.Y,
            DirectionType.Bottom => lRebarPtTransformed.Y < bottleRebarPtTransformed.Y,
            DirectionType.Right => lRebarPtTransformed.X > bottleRebarPtTransformed.X,
            DirectionType.Left => lRebarPtTransformed.X < bottleRebarPtTransformed.X,
            _ => false
        };
    }
}
