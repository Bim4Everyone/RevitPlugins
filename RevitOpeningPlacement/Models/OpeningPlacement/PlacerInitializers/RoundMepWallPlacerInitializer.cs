using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using RevitClashDetective.Models;
using RevitClashDetective.Models.Clashes;

using RevitOpeningPlacement.Models.Configs;
using RevitOpeningPlacement.Models.Extensions;
using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.OpeningPlacement.AngleFinders;
using RevitOpeningPlacement.Models.OpeningPlacement.DirGetters;
using RevitOpeningPlacement.Models.OpeningPlacement.ParameterGetters;
using RevitOpeningPlacement.Models.OpeningPlacement.PointFinders;
using RevitOpeningPlacement.Models.OpeningPlacement.Projectors;

namespace RevitOpeningPlacement.Models.OpeningPlacement.PlacerInitializers {
    internal class RoundMepWallPlacerInitializer {
        public static OpeningPlacer GetPlacer(RevitRepository revitRepository, ClashModel clashModel, MepCategory categoryOption) {
            var clash = new MepCurveWallClash(revitRepository, clashModel);

            var placer = new OpeningPlacer(revitRepository) {
                Clash = clashModel,
                AngleFinder = new WallAngleFinder(clash.Wall, clash.WallTransform)
            };
            if(clash.Curve.IsPerpendicular(clash.Wall)) {
                placer.PointFinder = new HorizontalPointFinder(clash);
                placer.ParameterGetter = new PerpendicularRoundCurveWallParamterGetter(clash, categoryOption);
                placer.Type = revitRepository.GetOpeningType(OpeningType.WallRound);
            } else {
                placer.PointFinder = new HorizontalPointFinder(clash, new InclinedSizeInitializer(clash, categoryOption).GetRoundMepHeightGetter());
                placer.ParameterGetter = new InclinedRoundCurveWallParameterGetter(clash, categoryOption);
                placer.Type = revitRepository.GetOpeningType(OpeningType.WallRectangle);
            };

            return placer;
        }
    }
}
