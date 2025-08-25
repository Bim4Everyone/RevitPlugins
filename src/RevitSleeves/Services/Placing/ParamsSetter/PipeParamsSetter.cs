using System;
using System.Linq;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;

using dosymep.Revit;

using RevitSleeves.Exceptions;
using RevitSleeves.Models;
using RevitSleeves.Models.Config;
using RevitSleeves.Services.Core;

namespace RevitSleeves.Services.Placing.ParamsSetter;
internal abstract class PipeParamsSetter : ParamsSetter {
    protected readonly RevitRepository _revitRepository;
    protected readonly IErrorsService _errorsService;
    protected readonly SleevePlacementSettingsConfig _config;

    protected PipeParamsSetter(
        RevitRepository revitRepository,
        IErrorsService errorsService,
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
                .FirstOrDefault(d => d.StartMepSize <= pipeDiameter && pipeDiameter <= d.EndMepSize)
                .SleeveDiameter);
        } catch(NullReferenceException) {
            _errorsService.AddError([pipe], "Exceptions.CannotFindDiameterRange");
            throw new CannotCreateSleeveException();
        }
    }
}
