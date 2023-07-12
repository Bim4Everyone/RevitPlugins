
using Autodesk.Revit.DB;

using RevitClashDetective.Models.Clashes;

using RevitOpeningPlacement.Models.Configs;
using RevitOpeningPlacement.Models.Extensions;
using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.OpeningPlacement.AngleFinders;
using RevitOpeningPlacement.Models.OpeningPlacement.LevelFinders;
using RevitOpeningPlacement.Models.OpeningPlacement.ParameterGetters;
using RevitOpeningPlacement.Models.OpeningPlacement.PointFinders;
using RevitOpeningPlacement.Models.OpeningPlacement.SolidProviders;

namespace RevitOpeningPlacement.Models.OpeningPlacement.PlacerInitializers {
    internal class RoundMepFloorPlacerInitializer : IMepCurvePlacerInitializer {
        public OpeningPlacer GetPlacer(RevitRepository revitRepository, ClashModel clashModel, MepCategory categoryOption) {
            var clash = new MepCurveClash<CeilingAndFloor>(revitRepository, clashModel);
            var pointFinder = new FloorPointFinder<MEPCurve>(clash);
            var placer = new OpeningPlacer(revitRepository, clashModel) {
                LevelFinder = new ClashLevelFinder(revitRepository, clashModel),
                PointFinder = pointFinder,
            };

            if(clash.Element2.IsHorizontal() && clash.Element1.IsVertical()) {
                placer.AngleFinder = new FloorAngleFinder(clash.Element1);
                placer.Type = revitRepository.GetOpeningType(OpeningType.FloorRound);
                placer.ParameterGetter = new PerpendicularRoundCurveFloorParamGetter(clash, categoryOption, pointFinder);
            } else {
                placer.AngleFinder = new ZeroAngleFinder();
                placer.Type = revitRepository.GetOpeningType(OpeningType.FloorRectangle);
                placer.ParameterGetter = new FloorSolidParameterGetter(new MepCurveClashSolidProvider<CeilingAndFloor>(clash), pointFinder, clash.Element1, clash.Element2, categoryOption);
            }

            return placer;
        }
    }
}
