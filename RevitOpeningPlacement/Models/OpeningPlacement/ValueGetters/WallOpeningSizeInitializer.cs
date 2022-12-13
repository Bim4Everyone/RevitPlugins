
using Autodesk.Revit.DB;

using RevitClashDetective.Models.Value;

using RevitOpeningPlacement.Models.Configs;
using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.OpeningPlacement.ValueGetters {
    internal class WallOpeningSizeInitializer {
        private readonly BoundingBoxXYZ _bb;
        private readonly MepCategory[] _mepCategories;

        public WallOpeningSizeInitializer(Solid solid, params MepCategory[] mepCategories) {
            _bb = solid.GetBoundingBox();
            _mepCategories = mepCategories;
        }
        public IValueGetter<DoubleParamValue> GetWidth() {
            return new OpeningSizeGetter(_bb.Max.X, _bb.Min.X, _mepCategories);
        }

        public IValueGetter<DoubleParamValue> GetHeight() {
            return new OpeningSizeGetter(_bb.Max.Z, _bb.Min.Z, _mepCategories);
        }

        public IValueGetter<DoubleParamValue> GetThickness() {
            return new OpeningSizeGetter(_bb.Max.Y, _bb.Min.Y);
        }
    }
}
