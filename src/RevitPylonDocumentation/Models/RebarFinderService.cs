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

    public FamilyInstance GetSkeletonParentRebar(string projectSection, string pylonKeyName) {
        var rebars = new FilteredElementCollector(Repository.Document)
            .OfCategory(BuiltInCategory.OST_Rebar)
            .WhereElementIsNotElementType()
            .ToElements();

        foreach(var rebar in rebars) {
            // Фильтрация по комплекту документации
            if(rebar.GetParamValue<string>(ViewModel.ProjectSettings.ProjectSection) != projectSection) {
                continue;
            }
            // Фильтрация по марке пилона
            if(rebar.GetParamValue<string>(_baseMarkParamName) != pylonKeyName) {
                continue;
            }
            // Фильтрация по обр_ФОП_Фильтрации 1
            if(rebar.GetParamValue<string>(ViewModel.ProjectSettings.TypicalPylonFilterParameter) !=
                ViewModel.ProjectSettings.TypicalPylonFilterValue) {
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
