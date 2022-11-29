
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
    internal class FittingWallPlacerInitializer : IFittingPlacerInitializer {
        public OpeningPlacer GetPlacer(RevitRepository revitRepository, ClashModel clashModel, params MepCategory[] categoryOptions) {
            var clash = new FittingClash<Wall>(revitRepository, clashModel);
            var angleFinder = new WallAngleFinder(clash.Element2, clash.Element2Transform);
            return new OpeningPlacer(revitRepository) {
                Type = revitRepository.GetOpeningType(OpeningType.WallRectangle),
                LevelFinder = new ClashLevelFinder(revitRepository, clashModel),
                AngleFinder = angleFinder,
                ParameterGetter = new WallSolidParameterGetter(new FittingClashSolidProvider<Wall>(clash, angleFinder), categoryOptions),
                PointFinder = new FittingWallPointFinder(clash, angleFinder)
            };
        }
    }
}
