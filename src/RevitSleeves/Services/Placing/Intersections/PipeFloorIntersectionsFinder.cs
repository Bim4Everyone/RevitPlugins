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
internal class PipeFloorIntersectionsFinder : MepOpeningCollisionFinder, IClashFinder<Pipe, Floor> {
    private readonly SleevePlacementSettingsConfig _config;

    public PipeFloorIntersectionsFinder(
        RevitRepository revitRepository,
        IMepElementsProvider mepElementsProvider,
        IStructureLinksProvider structureLinksProvider,
        IOpeningGeometryProvider openingGeometryProvider,
        SleevePlacementSettingsConfig config)
        : base(revitRepository, mepElementsProvider, structureLinksProvider, openingGeometryProvider) {

        _config = config ?? throw new ArgumentNullException(nameof(config));
    }

    public ICollection<ClashModel<Pipe, Floor>> FindClashes() {
        ICollection<ClashModel<Pipe, Floor>> structureClashes = [.. FindStructureClashes<Floor>(
            _config.PipeSettings.Category,
            _config.PipeSettings.MepFilterSet,
            _config.PipeSettings.FloorSettings.Category,
            _config.PipeSettings.FloorSettings.FilterSet)
            .Select(clash => new ClashModel<Pipe, Floor>(_revitRepository, clash))];

        ICollection<ClashModel<Pipe, Floor>> openingClashes = [.. FindOpeningClashes(
            _config.PipeSettings.Category,
            _config.PipeSettings.MepFilterSet,
            _config.PipeSettings.FloorSettings.Category,
            _config.PipeSettings.FloorSettings.FilterSet)
            .Select(clash => new ClashModel<Pipe, FamilyInstance>(_revitRepository, clash))
            .Select(openingClash => new ClashModel<Pipe, Floor>(
                _revitRepository, openingClash.MepElement,
                (Floor)openingClash.StructureElement.Host,
                openingClash.StructureTransform))];

        return [.. structureClashes, .. openingClashes];
    }
}
