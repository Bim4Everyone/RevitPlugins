using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SystemParams;

namespace RevitClashDetective.Models.Utils {
    internal class DoubleValueParser {

#if REVIT_2020_OR_LESS
        public static bool TryParse(string value, UnitType unitType, out double result) {

            if(unitType == UnitType.UT_Undefined) {
                return double.TryParse(value, out result);
            }
            return UnitFormatUtils.TryParse(new Units(UnitSystem.Metric), unitType, value, out result);
        }
#else
        public static bool TryParse(string value, ForgeTypeId unitType, out double result) {
            if(string.IsNullOrEmpty(unitType.TypeId)) {
                return double.TryParse(value, out result);
            }
            return UnitFormatUtils.TryParse(new Units(UnitSystem.Metric), unitType, value, out result);
        }
#endif
    }
}
