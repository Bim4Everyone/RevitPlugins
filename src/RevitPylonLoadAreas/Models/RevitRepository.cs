using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

using dosymep.Bim4Everyone.SharedParams;
using dosymep.Revit;

using RevitPylonLoadAreas.Models.Geometry;
using RevitPylonLoadAreas.Models.Selection;

namespace RevitPylonLoadAreas.Models;

internal class RevitRepository {

    public RevitRepository(UIApplication uiApplication) {
        UIApplication = uiApplication ?? throw new ArgumentNullException(nameof(uiApplication));
    }

    public SharedParam LoadAreaParam { get; } = SharedParamsConfig.Instance.CargoArea;
    public SharedParam LandThicknessParam { get; } = SharedParamsConfig.Instance.LandscapingThickness;
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

    public ICollection<Floor> PickFloors(string statusPrompt) {
        var reference = ActiveUIDocument.Selection.PickObjects(
            ObjectType.Element,
            new FloorSelectionFilter(),
            statusPrompt);
        return reference.Select(r => (Floor) Document.GetElement(r)).ToArray();
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

        var solid = CreateSolid(1, loops);
        return GetBottomFace(solid).Area;
    }

    public PlanarFace GetBottomFace(Solid solid) {
        return solid.Faces
            .OfType<PlanarFace>()
            .First(f => f.FaceNormal.IsAlmostEqualTo(-XYZ.BasisZ));
    }

    public IList<PlanarFace> GetBottomFaces(Solid solid) {
        return solid.Faces
            .OfType<PlanarFace>()
            .Where(f => f.FaceNormal.IsAlmostEqualTo(-XYZ.BasisZ))
            .ToArray();
    }

    /// <summary>
    /// Для каждой петли создает призму заданной высоты с основанием в плоскости петли
    /// </summary>
    /// <param name="height">Высота призмы</param>
    /// <param name="loops">Петли</param>
    /// <returns>Солид, образованный призмами петель</returns>
    public Solid CreateSolid(double height = 1, params CurveLoop[] loops) {
        if(loops.Length == 0) {
            throw new ArgumentOutOfRangeException(nameof(loops));
        }

        if(height < 0) {
            throw new ArgumentException(nameof(height));
        }

        return GeometryCreationUtilities.CreateExtrusionGeometry(loops, XYZ.BasisZ, height);
    }

    /// <summary>
    /// Строит косую призму с основанием в плоскости XOY и с верхней гранью по полигону
    /// </summary>
    /// <param name="polygon">Полигон, который должен лежать выше плоскости XOY</param>
    /// <returns>Косая призма</returns>
    public Solid CreateSolid(Polygon3D polygon) {
        var xyLoop = polygon.AsPolygon2D().AsCurveLoop();
        var xyzLoop = polygon.AsCurveLoop();
        var vertexPairs = Enumerable.Range(0, xyLoop.Count()).Select(i => new VertexPair(i, i)).ToArray();
        return GeometryCreationUtilities.CreateBlendGeometry(xyLoop, xyzLoop, vertexPairs);
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

    /// <summary>
    /// Показывает и выделяет заданные элементы на активном виде
    /// </summary>
    public void ShowElements(params Element[] elements) {
        if(elements is null || elements.Length == 0) {
            return;
        }

        ElementId[] elementIds = elements
            .Select(item => item.Id)
            .ToArray();

        ActiveUIDocument.ShowElements(elementIds);
        ActiveUIDocument.Selection.SetElementIds(elementIds);
    }

    /// <summary>
    /// Проверяет, что в документе у заданной категории присутствует общий параметр
    /// </summary>
    public bool CategoryHasParam(BuiltInCategory category, SharedParam param) {
        return param.IsExistsParam(Document)
               && param.GetParamBinding(Document)
                   .Binding
                   .GetCategories()
                   .Select(c => c.GetBuiltInCategory())
                   .Contains(category);
    }
}
