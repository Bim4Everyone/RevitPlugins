using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.IFC;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

using dosymep.Revit.Geometry;

using RevitPylonLoadAreas.Models.Geometry;
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

    public PlanarFace GetTopFace(Solid solid) {
        return solid.Faces
            .OfType<PlanarFace>()
            .First(f => f.FaceNormal.IsAlmostEqualTo(XYZ.BasisZ));
    }

    public IList<PlanarFace> GetTopFaces(Solid solid) {
        return solid.Faces
            .OfType<PlanarFace>()
            .Where(f => f.FaceNormal.IsAlmostEqualTo(XYZ.BasisZ))
            .ToArray();
    }

    public Solid CreateSolid(params CurveLoop[] loops) {
        if(loops.Length == 0) {
            throw new ArgumentOutOfRangeException(nameof(loops));
        }

        return GeometryCreationUtilities.CreateExtrusionGeometry(loops, XYZ.BasisZ, 1);
    }

    public Solid CreateSolid(Polygon3D polygon, Transform transform) {
        var xyLoop = CurveLoop.CreateViaTransform(
            polygon.ToPolygon2D().AsCurveLoop(),
            Transform.CreateTranslation(-5 * XYZ.BasisZ)
                .Multiply(transform));
        var xyzLoop = CurveLoop.CreateViaTransform(polygon.AsCurveLoop(), transform);
        var vertexPairs = Enumerable.Range(0, xyLoop.Count()).Select(i => new VertexPair(i, i)).ToArray();
        return GeometryCreationUtilities.CreateBlendGeometry(xyLoop, xyzLoop, vertexPairs);
    }

    public void CreateDirectShape(Solid solid) {
        DirectShape ds = DirectShape.CreateElement(
            Document,
            new ElementId(BuiltInCategory.OST_GenericModel));
        ds.SetShape(new GeometryObject[] { solid });
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

    /// <summary>
    /// Возвращает все видимые несущие колонны с активного вида
    /// </summary>
    public ICollection<FamilyInstance> GetPylonsFromView() {
        return new FilteredElementCollector(Document, Document.ActiveView.Id)
            .WhereElementIsNotElementType()
            .OfClass(typeof(FamilyInstance))
            .OfCategory(BuiltInCategory.OST_StructuralColumns)
            .OfType<FamilyInstance>()
            .ToArray();
    }

    /// <summary>
    /// Возвращает все видимые стены с активного вида
    /// </summary>
    public ICollection<Wall> GetWallsFromView() {
        return new FilteredElementCollector(Document, Document.ActiveView.Id)
            .WhereElementIsNotElementType()
            .OfClass(typeof(Wall))
            .OfType<Wall>()
            .ToArray();
    }
}
