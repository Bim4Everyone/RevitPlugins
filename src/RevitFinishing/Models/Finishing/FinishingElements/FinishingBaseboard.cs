using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using RevitFinishing.Services;

namespace RevitFinishing.Models.Finishing
{
    internal class FinishingBaseboard : FinishingElement {
        public FinishingBaseboard(Element element, FinishingCalculator calculator, ParamCalculationService paramService)
            : base(element, calculator, paramService) {
        }

        public override void UpdateCategoryParameters() {
            FinishingType finishingType = _calculator.RoomsByFinishingType[Rooms.First().RoomFinishingType];

            UpdateFromInstParam(_paramConfig.SizeLengthAdditional, BuiltInParameter.CURVE_ELEM_LENGTH);
            UpdateOrderParam(_paramConfig.BaseboardFinishingOrder, finishingType.GetBaseboardOrder(_revitElement.Name));
        }
    }
}
