using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using Bim4Everyone.RevitFiltration.Controls;

using dosymep.Bim4Everyone.ProjectParams;
using dosymep.Bim4Everyone.SharedParams;
using dosymep.Bim4Everyone.SystemParams;
using dosymep.Revit;

using RevitParamsChecker.Models.Revit;

namespace RevitParamsChecker.Models.Filtration;

internal class FilterDataProvider : IDataProvider {
    private readonly RevitRepository _revitRepository;

    public FilterDataProvider(RevitRepository revitRepository) {
        _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
    }

    public ICollection<IParam> GetParams(ICollection<Category> categories) {
        return ParameterFilterUtilities
            .GetFilterableParametersInCommon(_revitRepository.Document, [..categories.Select(c => c.Id)])
            .Select(GetFilterableParam)
            .Where(p => p != null)
            .ToArray();
    }

    public ICollection<Category> GetCategories() {
        return ParameterFilterUtilities.GetAllFilterableCategories()
            .Select(c => Category.GetCategory(_revitRepository.Document, c))
            .Where(category => category != null)
            .Where(c => c.CategoryType == CategoryType.Model && c.IsVisibleInUI)
            .ToArray();
    }

    public ICollection<Document> GetDocuments() {
        return _revitRepository.GetDocuments().Select(d => d.Document).ToArray();
    }

    private IParam GetFilterableParam(ElementId paramId) {
        try {
            if(paramId.IsSystemId()) {
                return new FilterableParam(
                    SystemParamsConfig.Instance.CreateRevitParam(
                        _revitRepository.Document,
                        (BuiltInParameter) paramId.GetIdValue()),
                    paramId);
            }

            var element = _revitRepository.Document.GetElement(paramId);
            if(element is SharedParameterElement sharedParameterElement) {
                return new FilterableParam(
                    SharedParamsConfig.Instance.CreateRevitParam(
                        _revitRepository.Document,
                        sharedParameterElement.Name),
                    paramId);
            }

            if(element is ParameterElement parameterElement) {
                return new FilterableParam(
                    ProjectParamsConfig.Instance.CreateRevitParam(_revitRepository.Document, parameterElement.Name),
                    paramId);
            }

            return null;
        } catch(Exception) {
            return null;
        }
    }
}
