using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SimpleServices;
using dosymep.Revit;

namespace RevitMarkAllDocuments.Models;

internal class RevitRepository {
    private readonly IRevitParamFactory _revitParamFactory;
    
    public RevitRepository(UIApplication uiApplication, IRevitParamFactory revitParamFactory) {
        UIApplication = uiApplication;
        _revitParamFactory = revitParamFactory;
    }

    public UIApplication UIApplication { get; }
    public UIDocument ActiveUIDocument => UIApplication.ActiveUIDocument;
    public Application Application => UIApplication.Application;
    public Document Document => ActiveUIDocument.Document;

    public IList<Category> GetCategories() {
        var allCategories = Document.Settings.Categories
            .Cast<Category>()
            .Select(x => x.Id)
            .ToList();

        var filterableCategories = ParameterFilterUtilities
            .RemoveUnfilterableCategories(allCategories);

        return filterableCategories
            .Select(x => Category.GetCategory(Document, x))
            .ToList();
    }

    public IList<Document> GetAllDocuments() {
        var docs = new List<Document> { Document };

        // Получаем все связи (RevitLinkInstance)
        var linkInstances = new FilteredElementCollector(Document)
            .OfClass(typeof(RevitLinkInstance))
            .Cast<RevitLinkInstance>();

        foreach(var linkInstance in linkInstances) {
            var linkDoc = linkInstance.GetLinkDocument();
            if(linkDoc != null && !docs.Contains(linkDoc)) {
                docs.Add(linkDoc);
            }
        }

        return docs;
    }

    public ICollection<FilterableParam> GetFilterableParams(Category category) {
        var elementTypes = GetElementTypes(category);
        var parameters = GetParams(category);

        List<FilterableParam> filterableParams = [];

        foreach(var param in parameters) {
            var filterableParam = new FilterableParam() { RevitParam = param };

            if(elementTypes.Any(x => x.IsExistsParam(param))) {
                filterableParam.IsTypeParam = true;
            }

            filterableParams.Add(filterableParam);
        }

        return filterableParams;
    }

    private ICollection<RevitParam> GetParams(Category category) {
        return [.. ParameterFilterUtilities
            .GetFilterableParametersInCommon(Document, [category.Id])
            .Select(x => _revitParamFactory.Create(Document, x))
            .Where(p => p != null)];
    }

    private ICollection<Element> GetElementTypes(Category category) {
        return new FilteredElementCollector(Document)
            .OfCategory(category.GetBuiltInCategory())
            .WhereElementIsElementType()
            .ToElements();
    }
}
