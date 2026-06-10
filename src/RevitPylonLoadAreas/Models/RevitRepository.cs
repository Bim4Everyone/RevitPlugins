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

    public FilledRegionType GetFilledRegionTypeOrDefault(string name = "") {
        var types = new FilteredElementCollector(Document)
            .OfClass(typeof(FilledRegionType))
            .OfType<FilledRegionType>()
            .ToArray();
        return types.FirstOrDefault(t => t.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase))
               ?? types.FirstOrDefault();
    }

    public double GetArea(params CurveLoop[] loops) {
        if(loops.Length == 0) {
            throw new ArgumentOutOfRangeException(nameof(loops));
        }

        var solid = CreateSolid(loops);
        return GetTopFace(solid).Area;
    }

    public Face GetTopFace(Solid solid) {
        return solid.Faces
            .OfType<PlanarFace>()
            .First(f => f.FaceNormal.IsAlmostEqualTo(XYZ.BasisZ));
    }

    public Solid CreateSolid(params CurveLoop[] loops) {
        if(loops.Length == 0) {
            throw new ArgumentOutOfRangeException(nameof(loops));
        }

        return GeometryCreationUtilities.CreateExtrusionGeometry(loops, XYZ.BasisZ, 1);
    }

    public Solid Intersect(Solid left, Solid right) {
        return BooleanOperationsUtils.ExecuteBooleanOperation(
            left,
            right,
            BooleanOperationsType.Intersect);
    }

    public Solid Unite(Solid left, Solid right) {
        return BooleanOperationsUtils.ExecuteBooleanOperation(
            left,
            right,
            BooleanOperationsType.Union);
    }
}
