using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitPylonDocumentation.ViewModels;

namespace RevitPylonDocumentation.Models;
public class RebarFinderService {
    private readonly ParamValueService _paramValueService;
    private readonly string _formNumberParamName = "обр_ФОП_Форма_номер";
    private readonly string _baseMarkParamName = "обр_ФОП_Метка основы IFC";

    internal RebarFinderService(MainViewModel mvm, RevitRepository repository) {
        ViewModel = mvm;
        Repository = repository;
        _paramValueService = new ParamValueService(repository);
    }

    internal MainViewModel ViewModel { get; set; }
    internal RevitRepository Repository { get; set; }

    private bool CheckRebarByBaseParams(Element rebar, string projectSection, string pylonKeyName) {
        // Фильтрация по комплекту документации
        if(rebar.GetParamValue<string>(ViewModel.ProjectSettings.ProjectSection) != projectSection) {
            return false;
        }
        // Фильтрация по марке пилона
        if(rebar.GetParamValue<string>(_baseMarkParamName) != pylonKeyName) {
            return false;
        }
        // Фильтрация по обр_ФОП_Фильтрации 1
        if(rebar.GetParamValue<string>(ViewModel.ProjectSettings.TypicalPylonFilterParameter) !=
            ViewModel.ProjectSettings.TypicalPylonFilterValue) {
            return false;
        }
        return true;
    }

    /// <summary>
    /// Получение родительского элемента сварного арматурного каркаса (верт стержни + пластины)
    /// </summary>
    public FamilyInstance GetSkeletonParentRebar(string projectSection, string pylonKeyName) {
        var rebars = new FilteredElementCollector(Repository.Document)
            .OfCategory(BuiltInCategory.OST_Rebar)
            .WhereElementIsNotElementType()
            .ToElements();

        foreach(var rebar in rebars) {
            // Фильтрация по базовым параметрам (комплект, марка, фильтрация 1)
            if(!CheckRebarByBaseParams(rebar, projectSection, pylonKeyName)) {
                continue;
            }
            // Фильтрация по имени семейства
            if(Repository.Document.GetElement(rebar.GetTypeId()) is not FamilySymbol rebarType) {
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
    /// Номера арматурных форм подаются граничными значениеями диапазонов
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
        var rebars = new FilteredElementCollector(Repository.Document)
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
            int formNumber = _paramValueService.GetParamValueAnywhere(rebar, _formNumberParamName);
            bool includeInNeededRange = formNumber >= formNumberMin && formNumber <= formNumberMax;
            bool includeInExceptionRange = formNumber >= formNumberMinException && formNumber <= formNumberMaxException;

            if(includeInNeededRange && !includeInExceptionRange) {
                simpleRebars.Add(rebar);
            }
        }
        return simpleRebars;
    }

    //public FamilyInstance GetSkeletonParentRebar(View view) {
    //    var rebars = new FilteredElementCollector(view.Document, view.Id)
    //        .OfCategory(BuiltInCategory.OST_Rebar)
    //        .WhereElementIsNotElementType()
    //        .ToElements();

    //    foreach(var rebar in rebars) {
    //        // Фильтрация по комплекту документации
    //        if(rebar.GetParamValue<string>(ViewModel.ProjectSettings.ProjectSection) != ViewModel.SelectedProjectSection) {
    //            continue;
    //        }
    //        // Фильтрация по имени семейства
    //        if(view.Document.GetElement(rebar.GetTypeId()) is not FamilySymbol rebarType) {
    //            continue;
    //        }
    //        if(rebarType.FamilyName.Equals("IFC_Пилон_Верт.Арм.") || rebarType.FamilyName.Contains("IFC_Каркас_Пилон")) {
    //            return rebar as FamilyInstance;
    //        }
    //    }
    //    return null;
    //}

    public List<FamilyInstance> GetClampsParentRebars(View view, string projectSection) {
        var rebars = new FilteredElementCollector(view.Document, view.Id)
            .OfCategory(BuiltInCategory.OST_Rebar)
            .WhereElementIsNotElementType()
            .ToElements();

        var parentRebars = new List<FamilyInstance>();

        foreach(var rebar in rebars) {
            // Фильтрация по комплекту документации
            if(rebar.GetParamValue<string>(ViewModel.ProjectSettings.ProjectSection) != projectSection) {
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
            if(rebar.GetParamValue<string>(ViewModel.ProjectSettings.ProjectSection) != projectSection) {
                continue;
            }
            // Фильтрация по номеру формы - отсев вертикальных стержней армирования
            int formNumber = _paramValueService.GetParamValueAnywhere(rebar, _formNumberParamName);
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
            if(rebar.GetParamValue<string>(ViewModel.ProjectSettings.ProjectSection) != projectSection) {
                continue;
            }
            // Фильтрация по номеру формы - отсев вертикальных стержней армирования
            int formNumber = _paramValueService.GetParamValueAnywhere(rebar, _formNumberParamName);
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
            if(rebar.GetParamValue<string>(ViewModel.ProjectSettings.ProjectSection) != projectSection) {
                continue;
            }
            // Фильтрация по номеру формы - отсев вертикальных стержней армирования
            int formNumber = _paramValueService.GetParamValueAnywhere(rebar, _formNumberParamName);
            if(formNumber == neededFormNumber) {
                simpleRebars.Add(rebar);
            }
        }
        return simpleRebars;
    }







}
