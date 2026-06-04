using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace RevitPylonLoadAreas.Models;

/// <summary>
/// Класс доступа к документу и приложению Revit:
/// выбор пилонов/стен/плит пользователем, доступ к активному виду, транзакции.
/// </summary>
internal class RevitRepository {
    public RevitRepository(UIApplication uiApplication) {
        UIApplication = uiApplication;
    }

    public UIApplication UIApplication { get; }
    public UIDocument ActiveUIDocument => UIApplication.ActiveUIDocument;
    public Application Application => UIApplication.Application;
    public Document Document => ActiveUIDocument.Document;
    public View ActiveView => ActiveUIDocument.ActiveView;

    public IReadOnlyList<Floor> PickFloors(string statusPrompt) {
        var refs = ActiveUIDocument.Selection.PickObjects(
            ObjectType.Element, new FloorFilter(), statusPrompt);
        return refs.Select(r => (Floor) Document.GetElement(r)).ToList();
    }

    public IReadOnlyList<FamilyInstance> PickStructuralColumns(string statusPrompt) {
        var refs = ActiveUIDocument.Selection.PickObjects(
            ObjectType.Element, new StructuralColumnFilter(), statusPrompt);
        return refs.Select(r => (FamilyInstance) Document.GetElement(r)).ToList();
    }

    public IReadOnlyList<Wall> PickWalls(string statusPrompt) {
        var refs = ActiveUIDocument.Selection.PickObjects(
            ObjectType.Element, new WallFilter(), statusPrompt);
        return refs.Select(r => (Wall) Document.GetElement(r)).ToList();
    }

    private sealed class FloorFilter : ISelectionFilter {
        public bool AllowElement(Element elem) => elem is Floor;
        public bool AllowReference(Reference reference, XYZ position) => true;
    }

    private sealed class StructuralColumnFilter : ISelectionFilter {
        private static readonly ElementId _structuralColumnsId =
            new ElementId(BuiltInCategory.OST_StructuralColumns);

        public bool AllowElement(Element elem) {
            return elem is FamilyInstance fi
                   && fi.Category != null
                   && fi.Category.Id == _structuralColumnsId;
        }
        public bool AllowReference(Reference reference, XYZ position) => true;
    }

    private sealed class WallFilter : ISelectionFilter {
        public bool AllowElement(Element elem) => elem is Wall;
        public bool AllowReference(Reference reference, XYZ position) => true;
    }
}
