using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

namespace RevitPylonDocumentation.Models {
    public class UnitUtilsHelper {
        public static double ConvertFromInternalValue(double value) {
            // Перевод во внутренние единицы Revit

#if REVIT_2021_OR_LESS
            DisplayUnitType unitType = DisplayUnitType.DUT_MILLIMETERS;
            double convertedValue = UnitUtils.ConvertFromInternalUnits(value, unitType);
#else
            ForgeTypeId unitType = UnitTypeId.Millimeters;
            double convertedValue = UnitUtils.ConvertFromInternalUnits(value, unitType);
#endif
            return convertedValue;
        }



        public static double ConvertToInternalValue(double value) {
            // Перевод во внутренние единицы Revit

#if D2020 || R2020
            DisplayUnitType unitType = DisplayUnitType.DUT_MILLIMETERS;
            double convertedValue = UnitUtils.ConvertToInternalUnits(value, unitType);
#elif D2021 || D2022 || R2021 || R2022
            ForgeTypeId unitType = UnitTypeId.Millimeters;
            double convertedValue = UnitUtils.ConvertToInternalUnits(value, unitType);
#else
                TaskDialog.Show("Ошибка!", ex.Message);
#endif
            return convertedValue;
        }
    }
}
