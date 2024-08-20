
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
using RevitOpeningPlacement.Models.OpeningPlacement.SolidProviders;

namespace RevitOpeningPlacement.Models.OpeningPlacement.PlacerInitializers {
    internal class RoundMepFloorPlacerInitializer : IMepCurvePlacerInitializer {
        public OpeningPlacer GetPlacer(RevitRepository revitRepository, ClashModel clashModel, MepCategory categoryOption) {
            var clash = new MepCurveClash<CeilingAndFloor>(revitRepository, clashModel);
            var pointFinder = new FloorPointFinder<MEPCurve>(clash);
            var levelFinder = new ClashLevelFinder(revitRepository, clashModel);
            var placer = new OpeningPlacer(revitRepository, clashModel) {
                LevelFinder = levelFinder,
                PointFinder = pointFinder,
                AngleFinder = new ZeroAngleFinder()
            };

            if(clash.Element2.IsHorizontal() && clash.Element1.IsVertical()) {
                OpeningType openingType = new RoundMepFloorOpeningTaskTypeProvider(clash.Element1, categoryOption).GetOpeningTaskType();

                placer.Type = revitRepository.GetOpeningTaskType(openingType);
                placer.ParameterGetter = new PerpendicularRoundCurveFloorParamGetter(clash, categoryOption, pointFinder, openingType, levelFinder);
            } else {
                placer.Type = revitRepository.GetOpeningTaskType(OpeningType.FloorRectangle);
                placer.ParameterGetter = new FloorSolidParameterGetter(new MepCurveClashSolidProvider<CeilingAndFloor>(clash), pointFinder, levelFinder, clash.Element1, clash.Element2, categoryOption);
            }

            return placer;
        }
    }
}
