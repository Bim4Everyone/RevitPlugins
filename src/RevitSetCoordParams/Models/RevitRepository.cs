using System.Collections.Generic;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace RevitSetCoordParams.Models;

internal class RevitRepository {

    public RevitRepository(UIApplication uiApplication) {
        UIApplication = uiApplication;
    }

    public UIApplication UIApplication { get; }
    public UIDocument ActiveUIDocument => UIApplication.ActiveUIDocument;
    public Application Application => UIApplication.Application;
    public Document Document => ActiveUIDocument.Document;

    public ICollection<RevitElement> GetAllRevitElements(IEnumerable<RevitCategory> revitCategories) {
        return [];
    }

    public ICollection<RevitElement> GetCurrentViewRevitElements(IEnumerable<RevitCategory> revitCategories) {
        return [];
    }

    public ICollection<RevitElement> GetSelectedRevitElements(IEnumerable<RevitCategory> revitCategories) {
        return [];
    }

    public View GetCurrentView() {
        return ActiveUIDocument.ActiveView;
    }
}
