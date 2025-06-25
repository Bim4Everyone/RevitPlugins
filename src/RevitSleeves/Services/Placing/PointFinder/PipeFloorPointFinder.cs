using System;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;

using dosymep.SimpleServices;

using RevitSleeves.Models.Placing;

namespace RevitSleeves.Services.Placing.PointFinder;
internal class PipeFloorPointFinder : IPointFinder<ClashModel<Pipe, Floor>> {
    private readonly IPlacingErrorsService _errorsService;
    private readonly ILocalizationService _localizationService;

    public PipeFloorPointFinder(IPlacingErrorsService errorsService, ILocalizationService localizationService) {
        _errorsService = errorsService ?? throw new ArgumentNullException(nameof(errorsService));
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
    }


    public XYZ GetPoint(ClashModel<Pipe, Floor> param) {
        try {
            return ((LocationCurve) param.MepElement.Location).Curve.GetEndPoint(0); // TODO
        } catch(Autodesk.Revit.Exceptions.ApplicationException) {
            _errorsService.AddError(new ErrorModel(
                [param.MepElement, param.StructureElement],
                _localizationService.GetLocalizedString("TODO")));
            throw new InvalidOperationException();
        }
    }
}
