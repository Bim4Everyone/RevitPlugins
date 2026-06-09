using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.IFC;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

using dosymep.Revit.Geometry;

using RevitPylonLoadAreas.Models.Selection;

namespace RevitPylonLoadAreas.Models;

internal class RevitRepository {
    public RevitRepository(UIApplication uiApplication) {
        UIApplication = uiApplication ?? throw new ArgumentNullException(nameof(uiApplication));
    }

    public UIApplication UIApplication { get; }
    public UIDocument ActiveUIDocument => UIApplication.ActiveUIDocument;
    public Application Application => UIApplication.Application;
    public Document Document => ActiveUIDocument.Document;
    public View ActiveView => ActiveUIDocument.ActiveView;

    public Floor PickFloor(string statusPrompt) {
        var reference = ActiveUIDocument.Selection.PickObject(
            ObjectType.Element,
            new FloorSelectionFilter(),
            statusPrompt);
        return (Floor) Document.GetElement(reference);
    }

    public ICollection<FamilyInstance> PickStructuralColumns(string statusPrompt) {
        var refs = ActiveUIDocument.Selection.PickObjects(
            ObjectType.Element,
            new StructuralColumnSelectionFilter(),
            statusPrompt);
        return refs.Select(r => (FamilyInstance) Document.GetElement(r)).ToList();
    }

    public ICollection<Wall> PickWalls(string statusPrompt) {
        var refs = ActiveUIDocument.Selection.PickObjects(
            ObjectType.Element,
            new WallSelectionFilter(),
            statusPrompt);
        return refs.Select(r => (Wall) Document.GetElement(r)).ToArray();
    }

    public FilledRegionType GetFirstFilledRegionType() {
        return new FilteredElementCollector(Document)
            .OfClass(typeof(FilledRegionType))
            .OfType<FilledRegionType>()
            .FirstOrDefault();
    }
}
