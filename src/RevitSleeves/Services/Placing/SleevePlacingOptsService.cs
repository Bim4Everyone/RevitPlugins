using System;
using System.Collections.Generic;
using System.Threading;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;

using Ninject;
using Ninject.Syntax;

using RevitSleeves.Models;
using RevitSleeves.Models.Config;
using RevitSleeves.Models.Placing;
using RevitSleeves.Services.Core;

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


    public ICollection<SleevePlacingOpts> GetOpts(IProgress<int> progress, CancellationToken ct) {
        var pipeWall = _resolutionRoot.Get<IClashFinder<Pipe, Wall>>().FindClashes(null, default);

        throw new NotImplementedException();
    }


}
