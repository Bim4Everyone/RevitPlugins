
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
        private readonly MepCurveClash<Wall> _clash;
        private readonly MepCategory _mepCategory;
        private readonly Line _wallLine;

        public InclinedSizeInitializer(MepCurveClash<Wall> clash, MepCategory mepCategory) {
            _clash = clash;
            _mepCategory = mepCategory;
            _wallLine = _clash.Element.GetСentralLine()
           .GetTransformedLine(_clash.ElementTransform);
        }

        public IValueGetter<DoubleParamValue> GetRoundMepHeightGetter() {
            var plane = _clash.Element.GetVerticalNormalPlane();

            var heightDirsGetter = new RoundMepDirsGetter(_clash);
            return new InclinedSizeValueGetter(_clash, new DiameterValueGetter(_clash.Curve, _mepCategory), plane, heightDirsGetter);
        }

        public IValueGetter<DoubleParamValue> GetRoundMepWidthGetter() {
            var plane = _clash.Element.GetHorizontalNormalPlane();

            var widthDirsGetters = new RoundMepDirsGetter(_clash);
            return new InclinedSizeValueGetter(_clash, new DiameterValueGetter(_clash.Curve, _mepCategory), plane, widthDirsGetters);
        }

        public IValueGetter<DoubleParamValue> GetRectangleMepHeightGetter() {
            var plane = _clash.Element.GetVerticalNormalPlane();

            var heightDirsGetter = new RectangleDirsGetter(_clash);
            return new InclinedSizeValueGetter(_clash, new DiagonalValueGetter(_clash, plane, _mepCategory), plane, heightDirsGetter);
        }

        public IValueGetter<DoubleParamValue> GetRectangleMepWidthGetter() {
            var plane = _clash.Element.GetHorizontalNormalPlane();

            var widthDirsGetters = new RectangleDirsGetter(_clash);
            return new InclinedSizeValueGetter(_clash, new DiagonalValueGetter(_clash, plane, _mepCategory), plane, widthDirsGetters);
        }
    }
}
