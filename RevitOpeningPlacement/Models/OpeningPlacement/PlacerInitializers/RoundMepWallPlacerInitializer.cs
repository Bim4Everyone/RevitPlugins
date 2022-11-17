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


namespace RevitOpeningPlacement.Models.OpeningPlacement.PlacerInitializers {
    internal class RoundMepWallPlacerInitializer : IMepCurvePlacerInitializer {
        public OpeningPlacer GetPlacer(RevitRepository revitRepository, ClashModel clashModel, MepCategory categoryOption) {
            var clash = new MepCurveClash<Wall>(revitRepository, clashModel);

            var placer = new OpeningPlacer(revitRepository) {
                Clash = clashModel,
                AngleFinder = new WallAngleFinder(clash.Element2, clash.Element2Transform)
            };
            if(clash.Element1.IsPerpendicular(clash.Element2)) {
                placer.PointFinder = new WallPointFinder(clash);
                placer.ParameterGetter = new PerpendicularRoundCurveWallParamGetter(clash, categoryOption);
                placer.Type = revitRepository.GetOpeningType(OpeningType.WallRound);
            } else {
                placer.PointFinder = new WallPointFinder(clash, new InclinedSizeInitializer(clash, categoryOption).GetRoundMepHeightGetter());
                placer.ParameterGetter = new InclinedRoundCurveWallParamGetter(clash, categoryOption);
                placer.Type = revitRepository.GetOpeningType(OpeningType.WallRectangle);
            };

            return placer;
        }
    }
}
