﻿using Autodesk.Revit.DB;

using RevitClashDetective.Models.Clashes;

using RevitOpeningPlacement.Models.Configs;
using RevitOpeningPlacement.Models.Extensions;
using RevitOpeningPlacement.Models.OpeningPlacement.AngleFinders;
using RevitOpeningPlacement.Models.OpeningPlacement.DirGetters;
using RevitOpeningPlacement.Models.OpeningPlacement.ParameterGetters;
using RevitOpeningPlacement.Models.OpeningPlacement.PointFinders;
using RevitOpeningPlacement.Models.OpeningPlacement.ValueGetters;

namespace RevitOpeningPlacement.Models.OpeningPlacement.PlacerInitializers {
    internal class RectangleMepWallPlacerInitializer {
        public static OpeningPlacer GetPlacer(RevitRepository revitRepository, ClashModel clashModel, MepCategory categoryOption) {
            var clash = new MepCurveWallClash(revitRepository, clashModel);

            var placer = new OpeningPlacer(revitRepository) {
                Clash = clashModel,
                AngleFinder = new WallAngleFinder(clash.Wall, clash.WallTransform),
                Type = revitRepository.GetOpeningType(OpeningType.WallRectangle),
            };
            if(clash.Curve.IsPerpendicular(clash.Wall)) {
                placer.ParameterGetter = new PerpendicularRectangleCurveWallParamterGetter(clash, categoryOption);
                placer.PointFinder = new HorizontalPointFinder(clash, new HeightValueGetter(clash, categoryOption));
            } else {
                placer.ParameterGetter = new InclinedRectangleCurveWallParameterGetter(clash, categoryOption);
                placer.PointFinder = new HorizontalPointFinder(clash, new InclinedSizeInitializer(clash, categoryOption).GetRectangleMepHeightGetter());
            };

            return placer;
        }
    }
}