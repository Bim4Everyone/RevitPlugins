
using System.Linq;

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
    internal class RectangleMepFloorPlacerInitializer : IMepCurvePlacerInitializer {
        public OpeningPlacer GetPlacer(RevitRepository revitRepository, ClashModel clashModel, MepCategory categoryOption) {
            var clash = new MepCurveClash<CeilingAndFloor>(revitRepository, clashModel);
            var placer = new OpeningPlacer(revitRepository) {
                LevelFinder = new ClashLevelFinder(revitRepository, clashModel),
                PointFinder = new FloorPointFinder<MEPCurve>(clash),
                Type = revitRepository.GetOpeningType(OpeningType.FloorRectangle),
            };

            if(clash.Element2.IsHorizontal() && clash.Element1.IsVertical()) {
                placer.AngleFinder = new FloorAngleFinder(clash.Element1);
                placer.ParameterGetter = new PerpendicularRectangleCurveFloorParamGetter(clash, categoryOption);
            } else {
                placer.AngleFinder = new ZeroAngleFinder();
                placer.ParameterGetter = new FloorSolidParameterGetter(new MepCurveClashSolidProvider<CeilingAndFloor>(clash), categoryOption);
            }

            return placer;
        }
    }
}
