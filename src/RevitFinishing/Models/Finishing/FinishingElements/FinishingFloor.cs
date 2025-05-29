using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

namespace RevitFinishing.Models.Finishing
{
    internal class FinishingFloor : FinishingElement {
        public FinishingFloor(Element element, FinishingCalculator calculator, ParamCalculationService paramService) 
            : base(element, calculator, paramService) {
        }

        public override void UpdateCategoryParameters() {
            FinishingType finishingType = _calculator.RoomsByFinishingType[Rooms.First().RoomFinishingType];

            UpdateFromInstParam(_paramConfig.SizeLengthAdditional, BuiltInParameter.HOST_PERIMETER_COMPUTED);
            UpdateOrderParam(_paramConfig.FloorFinishingOrder, finishingType.GetFloorOrder(_revitElement.Name));
        }
    }
}
