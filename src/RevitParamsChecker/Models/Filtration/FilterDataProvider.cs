using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using Bim4Everyone.RevitFiltration.Controls;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectParams;
using dosymep.Bim4Everyone.SharedParams;
using dosymep.Bim4Everyone.SystemParams;
using dosymep.Revit;

using RevitParamsChecker.Models.Revit;

namespace RevitParamsChecker.Models.Filtration;

internal class FilterDataProvider {
    private readonly RevitRepository _revitRepository;

    public FilterDataProvider(RevitRepository revitRepository) {
        _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
    }

    public DataProvider CreateDataProvider() {
        return new DataProvider(GetCategories(), GetParams, GetParamValues);
    }

    /// <summary>
    /// Создает провайдер данных для фильтра по параметрам материалов элементов.
    /// </summary>
    /// <returns>Провайдер данных с единственной категорией - "Материалы".</returns>
    public DataProvider CreateMaterialsDataProvider() {
        return new DataProvider(GetMaterialsCategories(), GetMaterialParams, GetMaterialParamValues);
    }

    private ICollection<RevitParam> GetParams(ICollection<Category> categories) {
        return ParameterFilterUtilities
            .GetFilterableParametersInCommon(_revitRepository.Document, [..categories.Select(c => c.Id)])
            .Select(GetFilterableParam)
            .Where(p => p != null)
            .ToArray();
    }

    private ICollection<Category> GetCategories() {
        return ParameterFilterUtilities.GetAllFilterableCategories()
            .Select(c => Category.GetCategory(_revitRepository.Document, c))
            .Where(category => category != null)
            .Where(c => c.CategoryType == CategoryType.Model && c.IsVisibleInUI)
            .ToArray();
    }

    private ICollection<Category> GetMaterialsCategories() {
        var category = Category.GetCategory(_revitRepository.Document, BuiltInCategory.OST_Materials);
        return category is null ? [] : [category];
    }

    /// <summary>
    /// Возвращает параметры, доступные для фильтрации материалов.
    /// </summary>
    /// <remarks>
    /// Материалы не входят в фильтруемые категории видов Revit, поэтому,
    /// если получить параметры штатным способом не удалось,
    /// они берутся из первого попавшегося материала документа.
    /// </remarks>
    private ICollection<RevitParam> GetMaterialParams(ICollection<Category> categories) {
        ICollection<ElementId> paramIds;
        try {
            paramIds = ParameterFilterUtilities.GetFilterableParametersInCommon(
                _revitRepository.Document,
                [..categories.Select(c => c.Id)]);
        } catch(Autodesk.Revit.Exceptions.ApplicationException) {
            paramIds = [];
        }

        var revitParams = paramIds
            .Select(GetFilterableParam)
            .Where(p => p != null)
            .ToArray();
        return revitParams.Length > 0 ? revitParams : GetFirstMaterialParams();
    }

    private ICollection<RevitParam> GetFirstMaterialParams() {
        var material = new FilteredElementCollector(_revitRepository.Document)
            .OfClass(typeof(Material))
            .FirstElement();
        if(material is null) {
            return [];
        }

        return material.Parameters
            .OfType<Parameter>()
            .Select(p => p.Id)
            .Select(GetFilterableParam)
            .Where(p => p != null)
            .ToArray();
    }

    private RevitParam GetFilterableParam(ElementId paramId) {
        try {
            if(paramId.IsSystemId()) {
                return SystemParamsConfig.Instance.CreateRevitParam(
                        _revitRepository.Document,
                        (BuiltInParameter) paramId.GetIdValue());
            }

            var element = _revitRepository.Document.GetElement(paramId);
            if(element is SharedParameterElement sharedParameterElement) {
                return SharedParamsConfig.Instance.CreateRevitParam(
                        _revitRepository.Document,
                        sharedParameterElement.Name);
            }

            if(element is ParameterElement parameterElement) {
                return ProjectParamsConfig.Instance.CreateRevitParam(_revitRepository.Document, parameterElement.Name);
            }

            return null;
        } catch(Exception) {
            return null;
        }
    }

    private ICollection<string> GetParamValues(ICollection<Category> categories, RevitParam param) {
        return _revitRepository.GetDocuments()
            .SelectMany(d => GetParamValues(d.Document, categories, param))
            .Distinct()
            .ToArray();
    }

    private ICollection<string> GetParamValues(
        Document doc,
        ICollection<Category> categories,
        RevitParam param) {
        try {
            if(param is SystemParam {
                   SystemParamId: BuiltInParameter.ELEM_PARTITION_PARAM
               }) {
                return new FilteredWorksetCollector(doc)
                    .OfKind(WorksetKind.UserWorkset)
                    .Select(w => w.Name)
                    .ToArray();
            }

            return new FilteredElementCollector(doc)
                .WhereElementIsNotElementType()
                .WherePasses(new ElementMulticategoryFilter(categories.Select(c => c.GetBuiltInCategory()).ToArray()))
                .Where(e => e.IsExistsParamValue(param.Name))
                .Select(e => e.GetParamValueString(param))
                .ToArray();
        } catch(Autodesk.Revit.Exceptions.ApplicationException) {
            return [];
        }
    }

    private ICollection<string> GetMaterialParamValues(ICollection<Category> categories, RevitParam param) {
        return _revitRepository.GetDocuments()
            .SelectMany(d => GetMaterialParamValues(d.Document, param))
            .Distinct()
            .ToArray();
    }

    private ICollection<string> GetMaterialParamValues(Document doc, RevitParam param) {
        try {
            return new FilteredElementCollector(doc)
                .OfClass(typeof(Material))
                .Where(e => e.IsExistsParamValue(param.Name))
                .Select(e => e.GetParamValueString(param))
                .ToArray();
        } catch(Autodesk.Revit.Exceptions.ApplicationException) {
            return [];
        }
    }
}
