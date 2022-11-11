
using Autodesk.Revit.DB;

using RevitClashDetective.Models.Clashes;

using RevitOpeningPlacement.Models.Configs;
using RevitOpeningPlacement.Models.Extensions;
using RevitOpeningPlacement.Models.OpeningPlacement.AngleFinders;
using RevitOpeningPlacement.Models.OpeningPlacement.ParameterGetters;
using RevitOpeningPlacement.Models.OpeningPlacement.PointFinders;


namespace RevitOpeningPlacement.Models.OpeningPlacement.PlacerInitializers {
    internal class RoundMepFloorPlacerInitializer {
        public static OpeningPlacer GetPlacer(RevitRepository revitRepository, ClashModel clashModel, MepCategory categoryOption) {
            var clash = new MepCurveClash<CeilingAndFloor>(revitRepository, clashModel);
            var placer = new OpeningPlacer(revitRepository) {
                Clash = clashModel,
                PointFinder = new FloorPointFinder(clash),
            };

            if(clash.Element.IsHorizontal() && clash.Curve.IsVertical()) {
                placer.AngleFinder = new FloorAngleFinder(clash.Curve);
                placer.Type = revitRepository.GetOpeningType(OpeningType.FloorRound);
                placer.ParameterGetter = new PerpendicularRoundCurveFloorParamterGetter(clash, categoryOption);
            } else {
                placer.AngleFinder = new ZeroAngleFinder();
                placer.Type = revitRepository.GetOpeningType(OpeningType.FloorRectangle);
                placer.ParameterGetter = new InclinedCurveFloorParameterGetter(clash, categoryOption);
            }

            return placer;
        }
    }
}
