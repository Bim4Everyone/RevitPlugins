using System;
using System.Linq;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;

using dosymep.Revit;

using RevitSleeves.Models;
using RevitSleeves.Models.Config;

namespace RevitSleeves.Services.Placing.ParamsSetter;
internal abstract class PipeParamsSetter : ParamsSetter {
    protected readonly RevitRepository _revitRepository;
    protected readonly IPlacingErrorsService _errorsService;
    protected readonly SleevePlacementSettingsConfig _config;

    protected PipeParamsSetter(
        RevitRepository revitRepository,
        IPlacingErrorsService errorsService,
        SleevePlacementSettingsConfig config) {

        _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
        _errorsService = errorsService ?? throw new ArgumentNullException(nameof(errorsService));
        _config = config ?? throw new ArgumentNullException(nameof(config));
    }


    protected double GetSleeveDiameter(Pipe pipe) {
        try {
            double pipeDiameter = _revitRepository.ConvertFromInternal(
                pipe.GetParamValue<double>(BuiltInParameter.RBS_PIPE_OUTER_DIAMETER));
            return _revitRepository.ConvertToInternal(_config.PipeSettings.DiameterRanges
                .First(d => d.StartMepSize <= pipeDiameter && pipeDiameter <= d.EndMepSize)
                .SleeveDiameter);
        } catch(ArgumentNullException) {
            _errorsService.AddError([pipe], "Exceptions.CannotFindDiameterRange");
            throw new InvalidOperationException();
        }
    }
}
