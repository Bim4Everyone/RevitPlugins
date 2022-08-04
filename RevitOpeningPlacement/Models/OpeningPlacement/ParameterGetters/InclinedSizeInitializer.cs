
using Autodesk.Revit.DB;

using RevitOpeningPlacement.Models.Configs;
using RevitOpeningPlacement.Models.Extensions;
using RevitOpeningPlacement.Models.OpeningPlacement.DirGetters;

namespace RevitOpeningPlacement.Models.OpeningPlacement.ParameterGetters {
    internal class InclinedSizeInitializer {
        private readonly MepCurveWallClash _clash;
        private readonly MepCategory _mepCategory;
        private readonly Line _wallLine;

        public InclinedSizeInitializer(MepCurveWallClash clash, MepCategory mepCategory) {
            _clash = clash;
            _mepCategory = mepCategory;
            _wallLine = _clash.Wall.GetСentralLine()
           .GetTransformedLine(_clash.WallTransform);
        }

        public InclinedSizeGetter GetRoundMepHeightGetter() {
            var plane = _clash.WallTransform.OfPlane(_clash.Wall.GetVerticalNormalPlane());
            var heightDirsGetter = new RoundMepDirsGetter(_clash, plane);
            return new InclinedSizeGetter(_clash, new DiameterGetter(_clash, _mepCategory), plane, heightDirsGetter, RevitRepository.OpeningHeight);
        }

        public InclinedSizeGetter GetRoundMepWidthGetter() {
            Plane plane = _clash.WallTransform.OfPlane(_clash.Wall.GetHorizontalNormalPlane());
            var widthDirsGetters = new RoundMepDirsGetter(_clash, plane);
            return new InclinedSizeGetter(_clash, new DiameterGetter(_clash, _mepCategory), plane, widthDirsGetters, RevitRepository.OpeningWidth);
        }

        public InclinedSizeGetter GetRectangleMepHeightGetter() {
            var plane = _clash.WallTransform.OfPlane(_clash.Wall.GetVerticalNormalPlane());
            var heightDirsGetter = new RectangleDirsGetter(_clash, plane);
            return new InclinedSizeGetter(_clash, new DiagonalGetter(_clash, plane, _mepCategory), plane, heightDirsGetter, RevitRepository.OpeningHeight);
        }

        public InclinedSizeGetter GetRectangleMepWidthGetter() {
            var plane = _clash.WallTransform.OfPlane(_clash.Wall.GetHorizontalNormalPlane());
            var widthDirsGetters = new RectangleDirsGetter(_clash, plane);
            return new InclinedSizeGetter(_clash, new DiagonalGetter(_clash, plane, _mepCategory), plane, widthDirsGetters, RevitRepository.OpeningWidth);
        }
    }
}
