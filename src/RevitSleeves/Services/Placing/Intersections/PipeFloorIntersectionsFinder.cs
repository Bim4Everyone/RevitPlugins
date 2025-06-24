using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;

using RevitSleeves.Models;
using RevitSleeves.Models.Config;
using RevitSleeves.Models.Placing;
using RevitSleeves.Services.Core;

namespace RevitSleeves.Services.Placing.Intersections;
internal class PipeFloorIntersectionsFinder : MepStructureCollisionFinder, IClashFinder<Pipe, Floor> {
    private readonly RevitRepository _repository;
    private readonly IMepElementsProvider _mepElementsProvider;
    private readonly IStructureLinksProvider _structureLinksProvider;
    private readonly SleevePlacementSettingsConfig _config;

    public PipeFloorIntersectionsFinder(
        RevitRepository revitRepository,
        IMepElementsProvider mepElementsProvider,
        IStructureLinksProvider structureLinksProvider,
        SleevePlacementSettingsConfig config) {

        _repository = revitRepository
            ?? throw new ArgumentNullException(nameof(revitRepository));
        _mepElementsProvider = mepElementsProvider
            ?? throw new ArgumentNullException(nameof(mepElementsProvider));
        _structureLinksProvider = structureLinksProvider
            ?? throw new ArgumentNullException(nameof(structureLinksProvider));
        _config = config
            ?? throw new ArgumentNullException(nameof(config));
    }

    public ICollection<ClashModel<Pipe, Floor>> FindClashes() {
        return [.. FindClashes(_repository,
            _mepElementsProvider,
            _structureLinksProvider,
            _config.PipeSettings,
            _config.PipeSettings.FloorSettings)
            .Select(clash => new ClashModel<Pipe, Floor>(_repository, clash))];
    }
}
