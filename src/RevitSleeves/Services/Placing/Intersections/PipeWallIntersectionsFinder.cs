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
internal class PipeWallIntersectionsFinder : MepOpeningCollisionFinder, IClashFinder<Pipe, Wall> {
    private readonly SleevePlacementSettingsConfig _config;

    public PipeWallIntersectionsFinder(
        RevitRepository revitRepository,
        IMepElementsProvider mepElementsProvider,
        IStructureLinksProvider structureLinksProvider,
        IOpeningGeometryProvider openingGeometryProvider,
        SleevePlacementSettingsConfig config)
        : base(revitRepository, mepElementsProvider, structureLinksProvider, openingGeometryProvider) {

        _config = config ?? throw new ArgumentNullException(nameof(config));
    }


    public ICollection<ClashModel<Pipe, Wall>> FindClashes() {
        ICollection<ClashModel<Pipe, Wall>> structureClashes = [.. FindStructureClashes<Wall>(
            _config.PipeSettings.Category,
            _config.PipeSettings.MepFilterSet,
            _config.PipeSettings.WallSettings.Category,
            _config.PipeSettings.WallSettings.FilterSet)
            .Select(clash => new ClashModel<Pipe, Wall>(_revitRepository, clash))];

        ICollection<ClashModel<Pipe, Wall>> openingClashes = [.. FindOpeningClashes(
            _config.PipeSettings.Category,
            _config.PipeSettings.MepFilterSet,
            _config.PipeSettings.WallSettings.Category,
            _config.PipeSettings.WallSettings.FilterSet)
            .Select(clash => new ClashModel<Pipe, FamilyInstance>(_revitRepository, clash))
            .Select(openingClash => new ClashModel<Pipe, Wall>(
                _revitRepository, openingClash.MepElement,
                (Wall)openingClash.StructureElement.Host,
                openingClash.StructureTransform))];

        return [.. structureClashes, .. openingClashes];
    }
}
