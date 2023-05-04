
using Autodesk.Revit.DB;

using RevitClashDetective.Models.Clashes;

using RevitOpeningPlacement.Models.Configs;
using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.OpeningPlacement.AngleFinders;
using RevitOpeningPlacement.Models.OpeningPlacement.LevelFinders;
using RevitOpeningPlacement.Models.OpeningPlacement.ParameterGetters;
using RevitOpeningPlacement.Models.OpeningPlacement.PointFinders;
using RevitOpeningPlacement.Models.OpeningPlacement.SolidProviders;

namespace RevitOpeningPlacement.Models.OpeningPlacement.PlacerInitializers {
    internal class FittingFloorPlacerInitializer : IFittingPlacerInitializer {
        public OpeningPlacer GetPlacer(RevitRepository revitRepository, ClashModel clashModel, params MepCategory[] categoryOptions) {
            var clash = new FittingClash<CeilingAndFloor>(revitRepository, clashModel);
            return new OpeningPlacer(revitRepository, clashModel) {
                Type = revitRepository.GetOpeningType(OpeningType.FloorRectangle),
                PointFinder = new FloorPointFinder<FamilyInstance>(clash),
                LevelFinder = new ClashLevelFinder(revitRepository, clashModel),
                AngleFinder = new ZeroAngleFinder(),
                ParameterGetter = new FloorSolidParameterGetter(new FittingClashSolidProvider<CeilingAndFloor>(clash, new ZeroAngleFinder()), categoryOptions)
            };
        }
    }
}
