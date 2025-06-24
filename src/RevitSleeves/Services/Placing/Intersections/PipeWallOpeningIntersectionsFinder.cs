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
internal class PipeWallOpeningIntersectionsFinder : MepOpeningCollisionFinder, IClashFinder<Pipe, FamilyInstance> {
    private readonly IMepElementsProvider _mepElementsProvider;
    private readonly IStructureLinksProvider _structureLinksProvider;
    private readonly SleevePlacementSettingsConfig _config;

    public PipeWallOpeningIntersectionsFinder(
        RevitRepository revitRepository,
        IOpeningGeometryProvider openingGeometryProvider,
        IMepElementsProvider mepElementsProvider,
        IStructureLinksProvider structureLinksProvider,
        SleevePlacementSettingsConfig config)
        : base(revitRepository, openingGeometryProvider) {

        _mepElementsProvider = mepElementsProvider
            ?? throw new ArgumentNullException(nameof(mepElementsProvider));
        _structureLinksProvider = structureLinksProvider
            ?? throw new ArgumentNullException(nameof(structureLinksProvider));
        _config = config
            ?? throw new ArgumentNullException(nameof(config));
    }

    public ICollection<ClashModel<Pipe, FamilyInstance>> FindClashes() {
        return [.. FindClashes(_mepElementsProvider,
            _structureLinksProvider,
            _config.PipeSettings,
            _config.PipeSettings.WallSettings,
            _structureLinksProvider.GetOpeningFamilyNames())
            .Select(clash => new ClashModel<Pipe, FamilyInstance>(_revitRepository, clash))];
    }
}
