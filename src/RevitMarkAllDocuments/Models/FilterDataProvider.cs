using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;
using Bim4Everyone.RevitFiltration.Controls;
using dosymep.Bim4Everyone.ProjectParams;
using dosymep.Bim4Everyone.SharedParams;
using dosymep.Bim4Everyone.SystemParams;
using dosymep.Bim4Everyone;
using dosymep.Revit;

namespace RevitMarkAllDocuments.Models;

internal class FilterDataProvider : IDataProvider {
    private readonly Document _doc;

    public FilterDataProvider(Document doc) {
        _doc = doc;
    }

    public ICollection<RevitParam> GetParams(ICollection<Category> categories) {
        return ParameterFilterUtilities
            .GetFilterableParametersInCommon(_doc, [.. categories.Select(c => c.Id)])
            .Select(GetFilterableParam)
            .Where(p => p != null)
            .ToArray();
    }

    public ICollection<Category> GetCategories() {
        return [Category.GetCategory(_doc, BuiltInCategory.OST_Walls)];
    }

    public ICollection<Document> GetDocuments() {
        return [_doc];
    }

    private RevitParam GetFilterableParam(ElementId paramId) {
        try {
            if(paramId.IsSystemId()) {
                return SystemParamsConfig.Instance.CreateRevitParam(
                        _doc,
                        (BuiltInParameter) paramId.GetIdValue());
            }

            var element = _doc.GetElement(paramId);
            if(element is SharedParameterElement sharedParameterElement) {
                return SharedParamsConfig.Instance.CreateRevitParam(
                        _doc,
                        sharedParameterElement.Name);
            }

            if(element is ParameterElement parameterElement) {
                return ProjectParamsConfig.Instance.CreateRevitParam(_doc, parameterElement.Name);
            }
            return null;
        } catch(Exception) {
            return null;
        }
    }
}
