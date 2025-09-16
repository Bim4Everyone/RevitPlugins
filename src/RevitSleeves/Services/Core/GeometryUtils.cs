using System;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitSleeves.Models;

namespace RevitSleeves.Services.Core;
internal class GeometryUtils : IGeometryUtils {
    private readonly RevitRepository _revitRepository;

    public GeometryUtils(RevitRepository revitRepository) {
        _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
    }


    public double GetFloorThickness(Floor floor) {
        if(floor is null) {
            throw new ArgumentNullException(nameof(floor));
        }

        return floor.FloorType.GetCompoundStructure().GetWidth();
    }

    public bool IsHorizontal(Floor floor) {
        if(floor is null) {
            throw new ArgumentNullException(nameof(floor));
        }

        var bbox = floor.GetBoundingBox();
        double bboxHeight = bbox.Max.Z - bbox.Min.Z;
        double floorHeight = GetFloorThickness(floor);
        return Math.Abs(bboxHeight - floorHeight) <= _revitRepository.Application.ShortCurveTolerance;
    }

    public bool IsVertical(MEPCurve curve) {
        if(curve is null) {
            throw new ArgumentNullException(nameof(curve));
        }

        return ((LocationCurve) curve.Location).Curve is Line line
            && (line.Direction.AngleTo(XYZ.BasisZ) < _revitRepository.Application.AngleTolerance
                || line.Direction.AngleTo(XYZ.BasisZ.Negate()) < _revitRepository.Application.AngleTolerance);
    }

    public bool IsHorizontal(MEPCurve curve) {
        if(curve is null) {
            throw new ArgumentNullException(nameof(curve));
        }

        return ((LocationCurve) curve.Location).Curve is Line line
            && (Math.Abs(line.Direction.Z) < _revitRepository.Application.ShortCurveTolerance);
    }

    public Solid CreateWallSolid(Wall wall, Transform transform = null) {
        if(wall is null) {
            throw new ArgumentNullException(nameof(wall));
        }
        var wallBbox = wall.GetBoundingBox();
        double wallHeight = wallBbox.Max.Z - wallBbox.Min.Z;
        var loop = GetWallLoop(wall);
        loop = transform is null ? loop : CurveLoop.CreateViaTransform(loop, transform);
        return GeometryCreationUtilities.CreateExtrusionGeometry([loop], XYZ.BasisZ, wallHeight);
    }

    private CurveLoop GetWallLoop(Wall wall) {
        var wallLine = (Line) ((LocationCurve) wall.Location).Curve;
        var wallLineStart = wallLine.GetEndPoint(0);
        var wallLineEnd = wallLine.GetEndPoint(1);
        double wallBottomZ = wall.GetBoundingBox().Min.Z;
        var lineStart = new XYZ(wallLineStart.X, wallLineStart.Y, wallBottomZ);
        var lineEnd = new XYZ(wallLineEnd.X, wallLineEnd.Y, wallBottomZ);

        double width = wall.Width;
        var wallNormal = wall.Orientation;
        var offsetVector = wallNormal * width / 2;
        var leftTop = lineStart + offsetVector;
        var rightTop = lineEnd + offsetVector;
        var rightBottom = lineEnd - offsetVector;
        var leftBottom = lineStart - offsetVector;

        return CurveLoop.Create([
            Line.CreateBound(leftTop, rightTop),
            Line.CreateBound(rightTop, rightBottom),
            Line.CreateBound(rightBottom, leftBottom),
            Line.CreateBound(leftBottom, leftTop)
        ]);
    }

    public Solid CreateCylinder(XYZ bottomPoint, XYZ topDir, double radius, double height) {
        var plane = Plane.CreateByNormalAndOrigin(topDir, bottomPoint);

        var circle = CurveLoop.Create([
            Arc.Create(plane, radius, 0, Math.PI),
            Arc.Create(plane, radius, Math.PI, Math.PI * 2)]);
        return GeometryCreationUtilities.CreateExtrusionGeometry([circle], topDir, height);
    }
}
