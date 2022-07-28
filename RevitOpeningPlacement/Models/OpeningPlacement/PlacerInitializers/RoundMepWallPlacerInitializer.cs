using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using RevitClashDetective.Models;
using RevitClashDetective.Models.Clashes;

using RevitOpeningPlacement.Models.Extensions;
using RevitOpeningPlacement.Models.OpeningPlacement.AngleFinders;
using RevitOpeningPlacement.Models.OpeningPlacement.ParameterGetters;
using RevitOpeningPlacement.Models.OpeningPlacement.PointFinders;

namespace RevitOpeningPlacement.Models.OpeningPlacement.PlacerInitializers {
    internal class RoundMepWallPlacerInitializer {
        public static OpeningPlacer GetPlacer(RevitRepository revitRepository, IEnumerable<DocInfo> docInfos, ClashModel clashModel) {
            var mepCurve = (MEPCurve) clashModel.MainElement.GetElement(docInfos);
            var wall = (Wall) clashModel.OtherElement.GetElement(docInfos);
            var transform = GetTransform(revitRepository, docInfos, wall);
            var clash = new MepCurveWallClash(mepCurve, wall, transform);

            var placer = new OpeningPlacer(revitRepository) {
                Clash = clashModel,
                AngleFinder = new WallAngleFinder(wall, transform)
            };
            if(mepCurve.IsPerpendicular(wall)) {
                placer.PointFinder = new HorizontalPointFinder(clash);
                placer.ParameterGetter = new PerpendicularRoundCurveWallParamterGetter(clash);
                placer.Type = revitRepository.GetOpeningType(OpeningType.WallRound);
            } else {
                placer.PointFinder = new HorizontalPointFinder(clash, new InclinedHeightGetter(clash, new DiameterGetter(clash)));
                placer.ParameterGetter = new InclinedRoundCurveWallParameterGetter(clash);
                placer.Type = revitRepository.GetOpeningType(OpeningType.WallRectangle);
            };

            return placer;
        }

        private static Transform GetTransform(RevitRepository clashRevitRepository, IEnumerable<DocInfo> docInfos, Element element) {
            return docInfos.FirstOrDefault(item => item.Name.Equals(clashRevitRepository.GetDocumentName(element.Document), StringComparison.CurrentCultureIgnoreCase))?.Transform
                ?? Transform.Identity;
        }
    }
}
