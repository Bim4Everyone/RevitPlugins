using System;
using System.Linq;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;

using RevitSleeves.Exceptions;
using RevitSleeves.Models;
using RevitSleeves.Models.Placing;
using RevitSleeves.Services.Core;

namespace RevitSleeves.Services.Placing.PointFinder;
internal class PipeWallPointFinder : IPointFinder<ClashModel<Pipe, Wall>> {
    private readonly RevitRepository _revitRepository;
    private readonly IGeometryUtils _geometryUtils;
    private readonly IPlacingErrorsService _errorsService;

    public PipeWallPointFinder(
        RevitRepository revitRepository,
        IGeometryUtils geometryUtils,
        IPlacingErrorsService errorsService) {

        _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
        _geometryUtils = geometryUtils ?? throw new ArgumentNullException(nameof(geometryUtils));
        _errorsService = errorsService ?? throw new ArgumentNullException(nameof(errorsService));
    }


    public XYZ GetPoint(ClashModel<Pipe, Wall> param) {
        Check(param);
        (var frontFace, var backFace) = GetFaces(param.StructureElement, param.StructureTransform);
        var mepCurve = CreateIncreasedMepCurve(param.MepElement, param.StructureElement);

        var frontIntersect = frontFace.Intersect(mepCurve, out var frontResults);
        var backIntersect = backFace.Intersect(mepCurve, out var backResults);
        if(frontIntersect != SetComparisonResult.Overlap || backIntersect != SetComparisonResult.Overlap) {
            _errorsService.AddError([param.MepElement, param.StructureElement], "Exceptions.CannotFindPlacingPoint");
            throw new CannotCreateSleeveException();
        }

        var frontPoint = frontResults.get_Item(0).XYZPoint;
        var backPoint = backResults.get_Item(0).XYZPoint;

        return (frontPoint + backPoint) / 2;
    }

    private void Check(ClashModel<Pipe, Wall> param) {
        if(_geometryUtils.IsVertical(param.MepElement)) {
            _errorsService.AddError([param.MepElement, param.StructureElement], "Exceptions.MepCurveIsVertical");
            throw new CannotCreateSleeveException();
        }
        if(((LocationCurve) param.StructureElement.Location).Curve is not Line wallLine) {
            _errorsService.AddError([param.MepElement, param.StructureElement], "Exceptions.WallLocationIsNotLine");
            throw new CannotCreateSleeveException();
        }
        if(((Line) ((LocationCurve) param.MepElement.Location).Curve).Direction.IsAlmostEqualTo(wallLine.Direction)) {
            _errorsService.AddError([param.MepElement, param.StructureElement], "Exceptions.MepCurveIsParallelToWall");
            throw new CannotCreateSleeveException();
        }
    }

    private (Face FrontFace, Face BackFace) GetFaces(Wall wall, Transform transform) {
        var wallSolid = _geometryUtils.CreateWallSolid(wall, transform);
        var faces = wallSolid.Faces.OfType<PlanarFace>().ToArray();
        var frontFace = faces.First(f => f.FaceNormal.IsAlmostEqualTo(wall.Orientation));
        var backFace = faces.First(f => f.FaceNormal.Negate().IsAlmostEqualTo(wall.Orientation));
        return (frontFace, backFace);
    }

    private Line CreateIncreasedMepCurve(Pipe pipe, Wall wall) {
        var line = (Line) ((LocationCurve) pipe.Location).Curve;
        var offset = line.Direction * wall.Width * 2;
        return Line.CreateBound(line.GetEndPoint(0) - offset, line.GetEndPoint(1) + offset);
    }
}
