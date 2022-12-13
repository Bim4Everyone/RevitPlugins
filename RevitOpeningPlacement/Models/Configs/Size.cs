using Autodesk.Revit.DB;

namespace RevitOpeningPlacement.Models.Configs {
    internal class Size {
        public string Name { get; set; }
        public double Value { get; set; }

        public double GetConvertedValue() {
#if REVIT_2021_OR_GREATER
            return UnitUtils.ConvertToInternalUnits(Value, UnitTypeId.Millimeters);
#else
            return UnitUtils.ConvertToInternalUnits(Value, DisplayUnitType.DUT_MILLIMETERS);
#endif
        }
    }
}
