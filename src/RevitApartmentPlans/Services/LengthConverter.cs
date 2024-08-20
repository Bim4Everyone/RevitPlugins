using Autodesk.Revit.DB;

namespace RevitApartmentPlans.Services {
    internal class LengthConverter : ILengthConverter {
        public LengthConverter() { }


        public double ConvertFromInternal(double feetValue) {
#if REVIT_2020_OR_LESS
            return UnitUtils.ConvertFromInternalUnits(feetValue, DisplayUnitType.DUT_MILLIMETERS);
#else
            return UnitUtils.ConvertFromInternalUnits(feetValue, UnitTypeId.Millimeters);
#endif
        }

        public double ConvertToInternal(double mmValue) {
#if REVIT_2020_OR_LESS
            return UnitUtils.ConvertToInternalUnits(mmValue, DisplayUnitType.DUT_MILLIMETERS);
#else
            return UnitUtils.ConvertToInternalUnits(mmValue, UnitTypeId.Millimeters);
#endif
        }
    }
}
