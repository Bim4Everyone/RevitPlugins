using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using RevitSleeves.Models.Placing;
using RevitSleeves.Services.Core;

using ParameterValueProvider = RevitClashDetective.Models.FilterableValueProviders.ParameterValueProvider;

namespace RevitSleeves.Models;

internal class RevitRepository {
    private readonly RevitClashDetective.Models.RevitRepository _clashRepository;

    public RevitRepository(UIApplication uiApplication, RevitClashDetective.Models.RevitRepository clashRepository) {
        UIApplication = uiApplication ?? throw new System.ArgumentNullException(nameof(uiApplication));
        _clashRepository = clashRepository ?? throw new System.ArgumentNullException(nameof(clashRepository));
    }

    public UIApplication UIApplication { get; }

    public UIDocument ActiveUIDocument => UIApplication.ActiveUIDocument;

    public Application Application => UIApplication.Application;

    public Document Document => ActiveUIDocument.Document;


    public IList<ParameterValueProvider> GetParameters(Document doc, Category category) {
        return _clashRepository.GetParameters(doc, [category]);
    }

    public RevitClashDetective.Models.RevitRepository GetClashRevitRepository() {
        return _clashRepository;
    }

    public Category GetCategory(BuiltInCategory category) {
        return Category.GetCategory(Document, category);
    }

    public ICollection<ElementId> GetLinkedElementIds<T>(RevitLinkInstance link) where T : Element {
        return new FilteredElementCollector(link.Document)
            .WhereElementIsNotElementType()
            .OfClass(typeof(T))
            .ToElementIds();
    }

    public ICollection<SleeveModel> GetSleeves() {
        return [.. new FilteredElementCollector(Document)
            .WhereElementIsNotElementType()
            .OfClass(typeof(FamilyInstance))
            .OfCategory(BuiltInCategory.OST_PipeFitting)
            .OfType<FamilyInstance>()
            .Where(f => f.Symbol.FamilyName.Equals(
                NamesProvider.FamilyNameSleeve,
                System.StringComparison.InvariantCultureIgnoreCase))
            .Select(f => new SleeveModel(f))];
    }
}
