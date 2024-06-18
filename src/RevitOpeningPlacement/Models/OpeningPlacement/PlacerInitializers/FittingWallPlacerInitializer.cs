
using Autodesk.Revit.DB;

using RevitClashDetective.Models.Clashes;

using RevitOpeningPlacement.Models.Configs;
using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.OpeningPlacement.AngleFinders;
using RevitOpeningPlacement.Models.OpeningPlacement.LevelFinders;
using RevitOpeningPlacement.Models.OpeningPlacement.ParameterGetters;
using RevitOpeningPlacement.Models.OpeningPlacement.PointFinders;
using RevitOpeningPlacement.Models.OpeningPlacement.SolidProviders;
using RevitOpeningPlacement.Models.OpeningPlacement.ValueGetters;

namespace RevitOpeningPlacement.Models.OpeningPlacement.PlacerInitializers {
    internal class FittingWallPlacerInitializer : IFittingPlacerInitializer {
        public OpeningPlacer GetPlacer(RevitRepository revitRepository, ClashModel clashModel, params MepCategory[] categoryOptions) {
            var clash = new FittingClash<Wall>(revitRepository, clashModel);
            var angleFinder = new WallAngleFinder(clash.Element2, clash.Element2Transform);
            var solidProvider = new FittingClashSolidProvider<Wall>(clash, angleFinder);
            var heightGetter = new WallOpeningSizeInitializer(solidProvider.GetSolid(), categoryOptions).GetHeight();
            var pointFinder = new FittingWallPointFinder(clash, angleFinder, heightGetter);
            var levelFinder = new ClashLevelFinder(revitRepository, clashModel);

            return new OpeningPlacer(revitRepository, clashModel) {
                Type = revitRepository.GetOpeningTaskType(OpeningType.WallRectangle),
                LevelFinder = levelFinder,
                AngleFinder = angleFinder,
                ParameterGetter = new WallSolidParameterGetter(solidProvider, pointFinder, levelFinder, clash.Element1, clash.Element2, categoryOptions),
                PointFinder = pointFinder
            };
        }
    }
}
