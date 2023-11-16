using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

namespace RevitRemoveRoomTags.Models {
    internal class UnitUtilsHelper {
        public static double ConvertFromInternalValue(double value) {
            // Перевод во внутренние единицы Revit

#if REVIT_2021_OR_GREATER
            ForgeTypeId unitType = UnitTypeId.Millimeters;
#else
            DisplayUnitType unitType = DisplayUnitType.DUT_MILLIMETERS;
#endif
            return UnitUtils.ConvertFromInternalUnits(value, unitType);
        }


        public static double ConvertToInternalValue(double value) {
            // Перевод во внутренние единицы Revit

#if REVIT_2021_OR_GREATER
            ForgeTypeId unitType = UnitTypeId.Millimeters;
#else
            DisplayUnitType unitType = DisplayUnitType.DUT_MILLIMETERS;
#endif
            return UnitUtils.ConvertToInternalUnits(value, unitType);
        }
    }
}
