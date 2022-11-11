
using Autodesk.Revit.DB;

using RevitClashDetective.Models.Value;

using RevitOpeningPlacement.Models.Configs;
using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.OpeningPlacement.ValueGetters {

    internal class FloorOpeningSizeInitializer {
        private readonly BoundingBoxXYZ _bb;
        private readonly MepCategory _mepCategory;

        public FloorOpeningSizeInitializer(Solid solid, MepCategory mepCategory) {
            _bb = solid.GetBoundingBox();
            _mepCategory = mepCategory;
        }

        public IValueGetter<DoubleParamValue> GetWidth() {
            return new OpeningInFloorSizeGetter(_bb.Max.X, _bb.Min.X, _mepCategory);
        }

        public IValueGetter<DoubleParamValue> GetHeight() {
            return new OpeningInFloorSizeGetter(_bb.Max.Y, _bb.Min.Y, _mepCategory);
        }

        public IValueGetter<DoubleParamValue> GetThickness() {
            return new OpeningInFloorSizeGetter(_bb.Max.Z, _bb.Min.Z);
        }
    }
}
