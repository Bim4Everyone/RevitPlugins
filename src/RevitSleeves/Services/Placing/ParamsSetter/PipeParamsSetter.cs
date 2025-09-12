using System;
using System.Linq;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;

using dosymep.Bim4Everyone;
using dosymep.Revit;

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


    protected DiameterRange GetSleeveDiameterRange(Pipe pipe) {
        double pipeDiameter = _revitRepository.ConvertFromInternal(
            pipe.GetParamValue<double>(BuiltInParameter.RBS_PIPE_OUTER_DIAMETER));
        return _config.PipeSettings.DiameterRanges
            .FirstOrDefault(d => d.StartMepSize <= pipeDiameter && pipeDiameter <= d.EndMepSize);
    }

    protected double GetSleeveDiameter(DiameterRange diameterRange) {
        return _revitRepository.ConvertToInternal(diameterRange.SleeveDiameter);
    }

    protected double GetSleeveThickness(DiameterRange diameterRange) {
        return _revitRepository.ConvertToInternal(diameterRange.SleeveThickness);
    }

    protected void SetStringParameters(FamilyInstance sleeve, Pipe pipe) {
        sleeve.SetParamValue(NamesProvider.ParameterSleeveSystem,
            pipe.GetParamValue<string>(NamesProvider.ParameterMepSystem));
        sleeve.SetParamValue(NamesProvider.ParameterSleeveEconomic,
            pipe.GetParamValue<string>(NamesProvider.ParameterMepEconomic));
        sleeve.SetParamValue(NamesProvider.ParameterSleeveDescription,
            pipe.Name);
    }
}
