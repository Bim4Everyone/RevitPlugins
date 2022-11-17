
using Autodesk.Revit.DB;

using RevitClashDetective.Models.Clashes;

using RevitOpeningPlacement.Models.Configs;
using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.OpeningPlacement.AngleFinders;
using RevitOpeningPlacement.Models.OpeningPlacement.ParameterGetters;
using RevitOpeningPlacement.Models.OpeningPlacement.PointFinders;


namespace RevitOpeningPlacement.Models.OpeningPlacement.PlacerInitializers {
    internal class FittingFloorPlacerInitializer : IFittingPlacerInitializer {
        public OpeningPlacer GetPlacer(RevitRepository revitRepository, ClashModel clashModel, params MepCategory[] categoryOptions) {
            var clash = new FittingClash<CeilingAndFloor>(revitRepository, clashModel);
            var placer = new OpeningPlacer(revitRepository) {
                Type = revitRepository.GetOpeningType(OpeningType.FloorRectangle),
                PointFinder = new FloorPointFinder<FamilyInstance>(clash),
                Clash = clashModel,
                AngleFinder = new ZeroAngleFinder(),
                ParameterGetter = new InclinedFloorParameterGetter<FamilyInstance>(clash, categoryOptions)
            };

            return placer;
        }
    }
}
