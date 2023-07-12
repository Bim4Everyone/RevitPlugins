using Autodesk.Revit.DB;

using RevitClashDetective.Models.Clashes;

using RevitOpeningPlacement.Models.Configs;
using RevitOpeningPlacement.Models.Extensions;
using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.OpeningPlacement.AngleFinders;
using RevitOpeningPlacement.Models.OpeningPlacement.LevelFinders;
using RevitOpeningPlacement.Models.OpeningPlacement.ParameterGetters;
using RevitOpeningPlacement.Models.OpeningPlacement.PointFinders;


namespace RevitOpeningPlacement.Models.OpeningPlacement.PlacerInitializers {
    internal class RoundMepWallPlacerInitializer : IMepCurvePlacerInitializer {
        public OpeningPlacer GetPlacer(RevitRepository revitRepository, ClashModel clashModel, MepCategory categoryOption) {
            var clash = new MepCurveClash<Wall>(revitRepository, clashModel);

            var placer = new OpeningPlacer(revitRepository, clashModel) {
                LevelFinder = new ClashLevelFinder(revitRepository, clashModel),
                AngleFinder = new WallAngleFinder(clash.Element2, clash.Element2Transform)
            };
            if(clash.Element1.IsPerpendicular(clash.Element2)) {
                var pointFinder = new WallPointFinder(clash);
                placer.PointFinder = pointFinder;
                placer.ParameterGetter = new PerpendicularRoundCurveWallParamGetter(clash, categoryOption, pointFinder);
                placer.Type = revitRepository.GetOpeningType(OpeningType.WallRound);
            } else {
                var pointFinder = new WallPointFinder(clash, new InclinedSizeInitializer(clash, categoryOption).GetRoundMepHeightGetter());
                placer.PointFinder = pointFinder;
                placer.ParameterGetter = new InclinedRoundCurveWallParamGetter(clash, categoryOption, pointFinder);
                placer.Type = revitRepository.GetOpeningType(OpeningType.WallRectangle);
            };

            return placer;
        }
    }
}
