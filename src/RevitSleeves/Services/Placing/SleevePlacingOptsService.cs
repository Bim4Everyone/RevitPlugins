using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;

using Ninject;
using Ninject.Syntax;

using RevitSleeves.Models.Config;
using RevitSleeves.Models.Placing;
using RevitSleeves.Services.Placing.Intersections;
using RevitSleeves.Services.Placing.PlacingOptsProvider;

namespace RevitSleeves.Services.Placing;
internal class SleevePlacingOptsService : ISleevePlacingOptsService {
    private readonly IResolutionRoot _resolutionRoot;
    private readonly SleevePlacementSettingsConfig _config;

    public SleevePlacingOptsService(
        IResolutionRoot resolutionRoot,
        SleevePlacementSettingsConfig config) {

        _resolutionRoot = resolutionRoot
            ?? throw new ArgumentNullException(nameof(resolutionRoot));
        _config = config
            ?? throw new ArgumentNullException(nameof(config));
    }


    public ICollection<SleevePlacingOpts> GetOpts() {
        List<SleevePlacingOpts> opts = [];
        if(_config.PipeSettings.WallSettings.IsEnabled) {
            var pipeWallClashes = _resolutionRoot.Get<IClashFinder<Pipe, Wall>>().FindClashes();
            opts.AddRange(GetOpts(pipeWallClashes));
        }
        if(_config.PipeSettings.FloorSettings.IsEnabled) {
            var pipeFloorClashes = _resolutionRoot.Get<IClashFinder<Pipe, Floor>>().FindClashes();
            opts.AddRange(GetOpts(pipeFloorClashes));
        }

        return opts;
    }

    private ICollection<SleevePlacingOpts> GetOpts<T>(ICollection<T> @params) where T : class {
        return _resolutionRoot.Get<IPlacingOptsProvider<T>>().GetOpts(@params);
    }
}
