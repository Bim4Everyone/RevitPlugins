using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;

using dosymep.Bim4Everyone;
using dosymep.Revit;

using RevitSleeves.Models;
using RevitSleeves.Models.Config;
using RevitSleeves.Models.Placing;
using RevitSleeves.Services.Core;
using RevitSleeves.Services.Placing.ParamsSetter;

namespace RevitSleeves.Services.Update;
internal class SleeveUpdaterService : ISleeveUpdaterService {
    private readonly RevitRepository _repository;
    private readonly SleevePlacementSettingsConfig _config;
    private readonly IErrorsService _errorsService;
    private readonly IParamsSetter _paramsSetter;
    private readonly IMepElementsProvider _mepElementsProvider;

    public SleeveUpdaterService(
        RevitRepository repository,
        SleevePlacementSettingsConfig config,
        IErrorsService errorsService,
        IParamsSetter paramsSetter,
        IMepElementsProvider mepElementsProvider) {

        _repository = repository
            ?? throw new ArgumentNullException(nameof(repository));
        _config = config
            ?? throw new ArgumentNullException(nameof(config));
        _errorsService = errorsService
            ?? throw new ArgumentNullException(nameof(errorsService));
        _paramsSetter = paramsSetter
            ?? throw new ArgumentNullException(nameof(paramsSetter));
        _mepElementsProvider = mepElementsProvider
            ?? throw new ArgumentNullException(nameof(mepElementsProvider));
    }


    public void UpdateSleeves(
        ICollection<SleeveModel> sleeves,
        IProgress<int> progress,
        CancellationToken ct) {

        var allPipesIds = _mepElementsProvider.GetMepElementIds(BuiltInCategory.OST_PipeCurves);

        int i = 0;
        foreach(var sleeve in sleeves) {
            ct.ThrowIfCancellationRequested();
            var pipes = GetIntersectingPipes(sleeve, allPipesIds);
            if(pipes.Count == 0) {
                _errorsService.AddError([sleeve.GetFamilyInstance()], "UpdateErrors.SleeveNotIntersectPipe");
            }
            if(pipes.Count == 1) {
                SetParamValues(sleeve, pipes.First());
            }
            if(pipes.Count > 1) {
                _errorsService.AddError([sleeve.GetFamilyInstance()], "UpdateErrors.SleeveIntersectsManyPipes");
            }
            progress?.Report(++i);
        }
    }

    private ICollection<Pipe> GetIntersectingPipes(SleeveModel sleeve, ICollection<ElementId> pipesIds) {
        var famInst = sleeve.GetFamilyInstance();
        var bbox = famInst.GetBoundingBox();
        return new FilteredElementCollector(_repository.Document, pipesIds)
            .WherePasses(new BoundingBoxIntersectsFilter(new Outline(bbox.Min, bbox.Max)))
            .WherePasses(new ElementIntersectsElementFilter(famInst))
            .ToElements()
            .OfType<Pipe>()
            .ToArray();
    }

    private void SetParamValues(SleeveModel sleeve, Pipe pipe) {
        var famInst = sleeve.GetFamilyInstance();

        var diameterRange = GetDiameterRange(pipe);
        if(diameterRange is not null) {
            SetSleeveDiameter(famInst, diameterRange);
            SetSleeveThickness(famInst, diameterRange);
        } else {
            _errorsService.AddError([famInst, pipe], "UpdateErrors.SleeveDiameterRangeNotFound");
        }
        famInst.SetParamValue(NamesProvider.ParameterSleeveSystem,
            pipe.GetParamValue<string>(NamesProvider.ParameterSleeveSystem));
        famInst.SetParamValue(NamesProvider.ParameterSleeveEconomic,
            pipe.GetParamValue<string>(NamesProvider.ParameterSleeveEconomic));
        famInst.SetParamValue(NamesProvider.ParameterSleeveDescription,
            pipe.Name);
    }

    private DiameterRange GetDiameterRange(Pipe pipe) {
        double pipeDiameter = _repository.ConvertFromInternal(
            pipe.GetParamValue<double>(BuiltInParameter.RBS_PIPE_OUTER_DIAMETER));
        return _config.PipeSettings.DiameterRanges
            .FirstOrDefault(d => d.StartMepSize <= pipeDiameter && pipeDiameter <= d.EndMepSize);
    }

    private void SetSleeveDiameter(FamilyInstance sleeve, DiameterRange diameterRange) {
        double sleeveDiameter = _repository.ConvertToInternal(diameterRange.SleeveDiameter);
        sleeve.SetParamValue(NamesProvider.ParameterSleeveDiameter, sleeveDiameter);
    }

    private void SetSleeveThickness(FamilyInstance sleeve, DiameterRange diameterRange) {
        double sleeveThickness = _repository.ConvertToInternal(diameterRange.SleeveThickness);
        sleeve.SetParamValue(NamesProvider.ParameterSleeveThickness, sleeveThickness);
    }
}
