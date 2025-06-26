using System;
using System.Linq;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;

using dosymep.Revit;
using dosymep.Revit.Geometry;
using dosymep.SimpleServices;

using RevitSleeves.Models;
using RevitSleeves.Models.Placing;
using RevitSleeves.Services.Core;

namespace RevitSleeves.Services.Placing.PointFinder;
internal class PipeFloorPointFinder : IPointFinder<ClashModel<Pipe, Floor>> {
    private readonly RevitRepository _revitRepository;
    private readonly IPlacingErrorsService _errorsService;
    private readonly ILocalizationService _localizationService;
    private readonly IGeometryUtils _geometryUtils;

    public PipeFloorPointFinder(
        RevitRepository revitRepository,
        IPlacingErrorsService errorsService,
        ILocalizationService localizationService,
        IGeometryUtils geometryUtils) {

        _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
        _errorsService = errorsService ?? throw new ArgumentNullException(nameof(errorsService));
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
        _geometryUtils = geometryUtils ?? throw new ArgumentNullException(nameof(geometryUtils));
    }


    public XYZ GetPoint(ClashModel<Pipe, Floor> param) {
        try {
            if(!_geometryUtils.FloorIsHorizontal(param.StructureElement)) {
                _errorsService.AddError(new ErrorModel(
                    [param.MepElement, param.StructureElement],
                    _localizationService.GetLocalizedString("Exceptions.FloorIsNotHorizontal")));
                throw new InvalidOperationException();
            }
            var bboxSolid = param.StructureElement
                .GetBoundingBox()
                .TransformBoundingBox(param.StructureTransform)
                .CreateSolid();
            var topFace = GetTopFace(bboxSolid);
            var bottomFace = GetBottomFace(bboxSolid);

            return ((LocationCurve) param.MepElement.Location).Curve.GetEndPoint(0); // TODO

        } catch(Autodesk.Revit.Exceptions.ApplicationException) {
            _errorsService.AddError(new ErrorModel(
                [param.MepElement, param.StructureElement],
                _localizationService.GetLocalizedString("TODO")));
            throw new InvalidOperationException();
        }
    }

    private PlanarFace GetTopFace(Solid solid) {
        return solid.Faces.OfType<PlanarFace>().First(face => face.FaceNormal.IsAlmostEqualTo(XYZ.BasisZ));
    }

    private PlanarFace GetBottomFace(Solid solid) {
        return solid.Faces.OfType<PlanarFace>().First(face => face.FaceNormal.IsAlmostEqualTo(XYZ.BasisZ.Negate()));
    }
}
