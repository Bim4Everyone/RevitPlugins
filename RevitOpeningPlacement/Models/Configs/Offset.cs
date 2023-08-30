
using Autodesk.Revit.DB;

namespace RevitOpeningPlacement.Models.Configs {
    internal class Offset {
        public double From { get; set; }
        public double To { get; set; }
        public double OffsetValue { get; set; }
        public string OpeningTypeName { get; set; }
        public Offset GetTransformedToInternalUnit() {
            return new Offset() {
                From = TransformToInternalUnits(From),
                To = TransformToInternalUnits(To),
                OffsetValue = TransformToInternalUnits(OffsetValue),
                OpeningTypeName = OpeningTypeName
            };
        }

#if REVIT_2020_OR_LESS
        private double TransformToInternalUnits(double mmValue) {
            return UnitUtils.ConvertToInternalUnits(mmValue, DisplayUnitType.DUT_MILLIMETERS);
        }
#else
        private double TransformToInternalUnits(double mmValue) {
            return UnitUtils.ConvertToInternalUnits(mmValue, UnitTypeId.Millimeters);
        }
#endif
    }
}
