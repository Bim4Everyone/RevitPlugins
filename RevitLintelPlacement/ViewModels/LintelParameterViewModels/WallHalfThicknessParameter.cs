using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitLintelPlacement.ViewModels.Interfaces;

namespace RevitLintelPlacement.ViewModels.LintelParameterViewModels {
    internal class WallHalfThicknessParameter : ILintelParameterViewModel {
        public void SetTo(FamilyInstance lintel, FamilyInstance elementInWall) {
            if(lintel is null) {
                throw new ArgumentNullException(nameof(lintel));
            }

            if(elementInWall is null) {
                throw new ArgumentNullException(nameof(elementInWall));
            }
            var elementWidth = elementInWall.GetParamValueOrDefault("ADSK_Размер_Ширина");
            if(elementWidth == null) {
                elementWidth = elementInWall.GetParamValueOrDefault(BuiltInParameter.FAMILY_WIDTH_PARAM);
            }
            lintel.SetParamValue("ЭЛМТ_ширина проема", (double)elementWidth);

        }
    }
}
