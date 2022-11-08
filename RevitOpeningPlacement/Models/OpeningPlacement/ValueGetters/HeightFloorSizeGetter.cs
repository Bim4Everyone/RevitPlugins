
using Autodesk.Revit.DB;

using RevitClashDetective.Models.Value;

using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.OpeningPlacement.ValueGetters {

    internal class FloorOpeningSizeInitializer {
        private readonly BoundingBoxXYZ _bb;
        public FloorOpeningSizeInitializer(Solid solid) {
            _bb = solid.GetBoundingBox();
        }

        public IValueGetter<DoubleParamValue> GetWidth() {
            return new OpeningInFloorSizeGetter(_bb.Max.X, _bb.Min.X);
        }

        public IValueGetter<DoubleParamValue> GetHeight() {
            return new OpeningInFloorSizeGetter(_bb.Max.Y, _bb.Min.Y);
        }

        public IValueGetter<DoubleParamValue> GetThickness() {
            return new OpeningInFloorSizeGetter(_bb.Max.Z, _bb.Min.Z);
        }
    }
}
