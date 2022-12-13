using Autodesk.Revit.DB;

using RevitClashDetective.Models.Clashes;

using RevitOpeningPlacement.Models.Configs;
using RevitOpeningPlacement.Models.Extensions;
using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.OpeningPlacement.AngleFinders;
using RevitOpeningPlacement.Models.OpeningPlacement.DirGetters;
using RevitOpeningPlacement.Models.OpeningPlacement.LevelFinders;
using RevitOpeningPlacement.Models.OpeningPlacement.ParameterGetters;
using RevitOpeningPlacement.Models.OpeningPlacement.PointFinders;
using RevitOpeningPlacement.Models.OpeningPlacement.ValueGetters;

namespace RevitOpeningPlacement.Models.OpeningPlacement.PlacerInitializers {
    internal class RectangleMepWallPlacerInitializer : IMepCurvePlacerInitializer {
        public OpeningPlacer GetPlacer(RevitRepository revitRepository, ClashModel clashModel, MepCategory categoryOption) {
            var clash = new MepCurveClash<Wall>(revitRepository, clashModel);

            var placer = new OpeningPlacer(revitRepository) {
                LevelFinder = new ClashLevelFinder(revitRepository, clashModel),
                AngleFinder = new WallAngleFinder(clash.Element2, clash.Element2Transform),
                Type = revitRepository.GetOpeningType(OpeningType.WallRectangle),
            };
            if(clash.Element1.IsPerpendicular(clash.Element2)) {
                placer.ParameterGetter = new PerpendicularRectangleCurveWallParamGetter(clash, categoryOption);
                placer.PointFinder = new WallPointFinder(clash, new HeightValueGetter(clash.Element1, categoryOption));
            } else {
                placer.ParameterGetter = new InclinedRectangleCurveWallParameterGetter(clash, categoryOption);
                placer.PointFinder = new WallPointFinder(clash, new InclinedSizeInitializer(clash, categoryOption).GetRectangleMepHeightGetter());
            };

            return placer;
        }
    }
}
