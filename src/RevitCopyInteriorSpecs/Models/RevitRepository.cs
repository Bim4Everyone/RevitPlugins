using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace RevitCopyInteriorSpecs.Models;
internal class RevitRepository {
    public RevitRepository(UIApplication uiApplication) {
        UIApplication = uiApplication;
    }

    public UIApplication UIApplication { get; }
    public UIDocument ActiveUIDocument => UIApplication.ActiveUIDocument;
    public Application Application => UIApplication.Application;
    public Document Document => ActiveUIDocument.Document;

    internal List<ViewSchedule> GetSelectedSpecs() {
        return ActiveUIDocument.Selection.GetElementIds()
            .Select(Document.GetElement)
            .Where(element => element is ViewSchedule)
            .Cast<ViewSchedule>()
            .ToList();
    }

    internal List<T> GetElements<T>() {
        return new FilteredElementCollector(Document)
        .OfClass(typeof(T))
        .WhereElementIsNotElementType()
        .Cast<T>()
        .ToList();
    }

    public ViewSchedule GetSpecByName(string specName) {
        return new FilteredElementCollector(Document)
            .OfClass(typeof(ViewSchedule))
            .OfType<ViewSchedule>()
            .FirstOrDefault(o => o.Name.Equals(specName));
    }

    public Level GetLevelByName(string specName) {
        return new FilteredElementCollector(Document)
        .OfClass(typeof(Level))
        .Cast<Level>()
        .FirstOrDefault(o => o.Name.Equals(specName));
    }
}
