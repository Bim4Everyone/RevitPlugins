using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;

using Ninject;
using Ninject.Syntax;

using RevitSleeves.Models;
using RevitSleeves.Models.Config;
using RevitSleeves.Models.Placing;
using RevitSleeves.Services.Core;
using RevitSleeves.Services.Placing.Intersections;

namespace RevitSleeves.Services.Placing;
internal class SleevePlacingOptsService : ISleevePlacingOptsService {
    private readonly IResolutionRoot _resolutionRoot;
    private readonly RevitRepository _repository;
    private readonly IMepElementsProvider _mepElementsProvider;
    private readonly IStructureLinksProvider _structureLinksProvider;
    private readonly SleevePlacementSettingsConfig _config;

    public SleevePlacingOptsService(
        IResolutionRoot resolutionRoot,
        RevitRepository repository,
        IMepElementsProvider mepElementsProvider,
        IStructureLinksProvider structureLinksProvider,
        SleevePlacementSettingsConfig config) {

        _resolutionRoot = resolutionRoot
            ?? throw new ArgumentNullException(nameof(resolutionRoot));
        _repository = repository
            ?? throw new ArgumentNullException(nameof(repository));
        _mepElementsProvider = mepElementsProvider
            ?? throw new ArgumentNullException(nameof(mepElementsProvider));
        _structureLinksProvider = structureLinksProvider
            ?? throw new ArgumentNullException(nameof(structureLinksProvider));
        _config = config
            ?? throw new ArgumentNullException(nameof(config));
    }


    public ICollection<SleevePlacingOpts> GetOpts() {
        List<SleevePlacingOpts> opts = [];
        if(_config.PipeSettings.WallSettings.IsEnabled) {

            var pipeWallClashes = _resolutionRoot.Get<IClashFinder<Pipe, Wall>>().FindClashes();
            var pipeOpeningWallClashes = _resolutionRoot.Get<PipeWallOpeningIntersectionsFinder>().FindClashes();

            opts.AddRange(GetOpts(pipeWallClashes));
            opts.AddRange(GetOpts(pipeOpeningWallClashes));
        }
        if(_config.PipeSettings.FloorSettings.IsEnabled) {
            var pipeFloorClashes = _resolutionRoot.Get<IClashFinder<Pipe, Floor>>().FindClashes();
            var pipeOpeningFloorClashes = _resolutionRoot.Get<PipeFloorOpeningIntersectionsFinder>().FindClashes();

            opts.AddRange(GetOpts(pipeFloorClashes));
            opts.AddRange(GetOpts(pipeOpeningFloorClashes));
        }

        return opts;
    }

    private ICollection<SleevePlacingOpts> GetOpts<T>(ICollection<T> @params) where T : class {
        return _resolutionRoot.Get<IPlacingOptsProvider<T>>().GetOpts(@params);
    }
}
