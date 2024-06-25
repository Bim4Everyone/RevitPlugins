using Autodesk.Revit.DB;

using RevitClashDetective.Models.Clashes;

using RevitOpeningPlacement.Models.Configs;
using RevitOpeningPlacement.Models.Extensions;
using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.OpeningPlacement.AngleFinders;
using RevitOpeningPlacement.Models.OpeningPlacement.LevelFinders;
using RevitOpeningPlacement.Models.OpeningPlacement.ParameterGetters;
using RevitOpeningPlacement.Models.OpeningPlacement.PointFinders;
using RevitOpeningPlacement.Models.OpeningPlacement.Providers;
using RevitOpeningPlacement.Models.OpeningPlacement.ValueGetters;

namespace RevitOpeningPlacement.Models.OpeningPlacement.PlacerInitializers {
    internal class RoundMepWallPlacerInitializer : IMepCurvePlacerInitializer {
        public OpeningPlacer GetPlacer(RevitRepository revitRepository, ClashModel clashModel, MepCategory categoryOption) {
            var clash = new MepCurveClash<Wall>(revitRepository, clashModel);
            var levelFinder = new ClashLevelFinder(revitRepository, clashModel);
            var placer = new OpeningPlacer(revitRepository, clashModel) {
                LevelFinder = levelFinder,
                AngleFinder = new WallAngleFinder(clash.Element2, clash.Element2Transform)
            };
            if(clash.Element1.IsPerpendicular(clash.Element2)) {
                OpeningType openingType = new RoundMepWallOpeningTaskTypeProvider(clash.Element1, categoryOption).GetOpeningTaskType();
                var pointFinder =
                    openingType == OpeningType.WallRound
                    ? new WallPointFinder(clash)
                    : new WallPointFinder(clash, new DiameterValueGetter(clash.Element1, categoryOption));

                placer.PointFinder = pointFinder;
                placer.ParameterGetter = new PerpendicularRoundCurveWallParamGetter(clash, categoryOption, pointFinder, openingType, levelFinder);
                placer.Type = revitRepository.GetOpeningTaskType(openingType);
            } else {
                var pointFinder = new WallPointFinder(clash, new InclinedSizeInitializer(clash, categoryOption).GetRoundMepHeightGetter());

                placer.PointFinder = pointFinder;
                placer.ParameterGetter = new InclinedRoundCurveWallParamGetter(clash, categoryOption, pointFinder, levelFinder);
                placer.Type = revitRepository.GetOpeningTaskType(OpeningType.WallRectangle);
            };

            return placer;
        }
    }
}
