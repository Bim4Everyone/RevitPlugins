using System.Collections.Generic;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

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
}
