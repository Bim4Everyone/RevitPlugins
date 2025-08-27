using System;
using System.Linq;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;

using dosymep.Revit;
using dosymep.Revit.Geometry;

using RevitSleeves.Exceptions;
using RevitSleeves.Models;
using RevitSleeves.Models.Config;
using RevitSleeves.Models.Placing;
using RevitSleeves.Services.Core;

namespace RevitSleeves.Services.Placing.PointFinder;
internal class PipeFloorPointFinder : IPointFinder<ClashModel<Pipe, Floor>> {
    private readonly RevitRepository _revitRepository;
    private readonly IErrorsService _errorsService;
    private readonly IGeometryUtils _geometryUtils;
    private readonly SleevePlacementSettingsConfig _config;

    public PipeFloorPointFinder(
        RevitRepository revitRepository,
        IErrorsService errorsService,
        IGeometryUtils geometryUtils,
        SleevePlacementSettingsConfig config) {

        _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
        _errorsService = errorsService ?? throw new ArgumentNullException(nameof(errorsService));
        _geometryUtils = geometryUtils ?? throw new ArgumentNullException(nameof(geometryUtils));
        _config = config ?? throw new ArgumentNullException(nameof(config));
    }


    public XYZ GetPoint(ClashModel<Pipe, Floor> param) {
        try {
            Check(param);
            double zCoordinate = param.StructureElement
                .GetBoundingBox()
                .TransformBoundingBox(param.StructureTransform)
                .Max.Z;
            var mepPoint = ((LocationCurve) param.MepElement.Location).Curve.GetEndPoint(0);
            var topIntersectionPoint = new XYZ(mepPoint.X, mepPoint.Y, zCoordinate);
            double floorThickness = _geometryUtils.GetFloorThickness(param.StructureElement);
            var centerIntersectionPoint = topIntersectionPoint - XYZ.BasisZ * floorThickness / 2;

            double topOffsetMm = _config.PipeSettings.Offsets
                .First(o => o.OffsetType == OffsetType.FromSleeveEndToTopFloorFace).Value;
            return centerIntersectionPoint + XYZ.BasisZ * _revitRepository.ConvertToInternal(topOffsetMm) / 2;

        } catch(Autodesk.Revit.Exceptions.ApplicationException) {
            _errorsService.AddError([param.MepElement, param.StructureElement], "Exceptions.CannotFindPlacingPoint");
            throw new CannotCreateSleeveException();
        }
    }

    private void Check(ClashModel<Pipe, Floor> clash) {
        if(!_geometryUtils.IsHorizontal(clash.StructureElement)) {
            _errorsService.AddError([clash.MepElement, clash.StructureElement], "Exceptions.FloorIsNotHorizontal");
            throw new CannotCreateSleeveException();
        }
        if(!_geometryUtils.IsVertical(clash.MepElement)) {
            _errorsService.AddError([clash.MepElement, clash.StructureElement], "Exceptions.MepCurveIsNotVertical");
            throw new CannotCreateSleeveException();
        }
    }
}
