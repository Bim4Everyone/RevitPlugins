
using Autodesk.Revit.DB;

using RevitClashDetective.Models.Clashes;

using RevitOpeningPlacement.Models.Configs;
using RevitOpeningPlacement.Models.OpeningPlacement.AngleFinders;
using RevitOpeningPlacement.Models.OpeningPlacement.ParameterGetters;
using RevitOpeningPlacement.Models.OpeningPlacement.PointFinders;


namespace RevitOpeningPlacement.Models.OpeningPlacement.PlacerInitializers {
    internal class RoundMepFloorPlacerInitializer {
        public static OpeningPlacer GetPlacer(RevitRepository revitRepository, ClashModel clashModel, MepCategory categoryOption) {
            var clash = new MepCurveClash<CeilingAndFloor>(revitRepository, clashModel);
            var placer = new OpeningPlacer(revitRepository) {
                Clash = clashModel,
                AngleFinder = new FloorAngleFinder(clash.Curve),
                PointFinder = new FloorPointFinder(clash),
                Type = revitRepository.GetOpeningType(OpeningType.FloorRound),
                ParameterGetter = new PerpendicularRoundCurveFloorParamterGetter(clash, categoryOption)
            };

            return placer;
        }
    }
}
