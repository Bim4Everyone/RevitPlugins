
using Autodesk.Revit.DB;

using RevitClashDetective.Models.Value;

using RevitOpeningPlacement.Models.Configs;
using RevitOpeningPlacement.Models.Exceptions;
using RevitOpeningPlacement.Models.Extensions;
using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.OpeningPlacement.DirGetters;
using RevitOpeningPlacement.Models.OpeningPlacement.ValueGetters;

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

        public IValueGetter<DoubleParamValue> GetRoundMepHeightGetter() {
            var plane = _clash.Wall.GetVerticalNormalPlane();

            var heightDirsGetter = new RoundMepDirsGetter(_clash);
            return new InclinedSizeValueGetter(_clash, new DiameterValueGetter(_clash, _mepCategory), plane, heightDirsGetter);
        }

        public IValueGetter<DoubleParamValue> GetRoundMepWidthGetter() {
            var plane = _clash.Wall.GetHorizontalNormalPlane();

            var widthDirsGetters = new RoundMepDirsGetter(_clash);
            return new InclinedSizeValueGetter(_clash, new DiameterValueGetter(_clash, _mepCategory), plane, widthDirsGetters);
        }

        public IValueGetter<DoubleParamValue> GetRectangleMepHeightGetter() {
            var plane = _clash.Wall.GetVerticalNormalPlane();

            var heightDirsGetter = new RectangleDirsGetter(_clash);
            return new InclinedSizeValueGetter(_clash, new DiagonalValueGetter(_clash, plane, _mepCategory), plane, heightDirsGetter);
        }

        public IValueGetter<DoubleParamValue> GetRectangleMepWidthGetter() {
            var plane = _clash.Wall.GetHorizontalNormalPlane();

            var widthDirsGetters = new RectangleDirsGetter(_clash);
            return new InclinedSizeValueGetter(_clash, new DiagonalValueGetter(_clash, plane, _mepCategory), plane, widthDirsGetters);
        }
    }
}
