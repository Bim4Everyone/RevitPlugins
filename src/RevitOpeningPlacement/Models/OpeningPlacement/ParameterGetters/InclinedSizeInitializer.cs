
using Autodesk.Revit.DB;

using RevitClashDetective.Models.Value;

using RevitOpeningPlacement.Models.Configs;
using RevitOpeningPlacement.Models.Extensions;
using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.OpeningPlacement.DirGetters;
using RevitOpeningPlacement.Models.OpeningPlacement.ValueGetters;

namespace RevitOpeningPlacement.Models.OpeningPlacement.ParameterGetters {
    internal class InclinedSizeInitializer {
        private readonly MepCurveClash<Wall> _clash;
        private readonly MepCategory _mepCategory;

        public InclinedSizeInitializer(MepCurveClash<Wall> clash, MepCategory mepCategory) {
            _clash = clash;
            _mepCategory = mepCategory;
        }

        public IValueGetter<DoubleParamValue> GetRoundMepHeightGetter() {
            var plane = _clash.Element2.GetVerticalNormalPlane();

            var heightDirsGetter = new RoundMepDirsGetter(_clash);
            return new InclinedSizeValueGetter(_clash, new DiameterValueGetter(_clash.Element1, _mepCategory), plane, heightDirsGetter, _mepCategory);
        }

        public IValueGetter<DoubleParamValue> GetRoundMepWidthGetter() {
            var plane = _clash.Element2.GetHorizontalNormalPlane();

            var widthDirsGetters = new RoundMepDirsGetter(_clash);
            return new InclinedSizeValueGetter(_clash, new DiameterValueGetter(_clash.Element1, _mepCategory), plane, widthDirsGetters, _mepCategory);
        }

        public IValueGetter<DoubleParamValue> GetRectangleMepHeightGetter() {
            var plane = _clash.Element2.GetVerticalNormalPlane();

            var heightDirsGetter = new RectangleDirsGetter(_clash);
            return new InclinedSizeValueGetter(_clash, new DiagonalValueGetter(_clash, plane, _mepCategory), plane, heightDirsGetter, _mepCategory);
        }

        public IValueGetter<DoubleParamValue> GetRectangleMepWidthGetter() {
            var plane = _clash.Element2.GetHorizontalNormalPlane();

            var widthDirsGetters = new RectangleDirsGetter(_clash);
            return new InclinedSizeValueGetter(_clash, new DiagonalValueGetter(_clash, plane, _mepCategory), plane, widthDirsGetters, _mepCategory);
        }
    }
}
