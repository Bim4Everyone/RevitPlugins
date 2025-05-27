using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SharedParams;
using dosymep.Revit;

namespace RevitFinishing.Models.Finishing
{
    internal class FinishingFloor : FinishingElement {
        public FinishingFloor(Element element, FinishingCalculator calculator) : base(element, calculator) {
        }

        public override void UpdateCategoryParameters() {
            FinishingType finishingType = _calculator.RoomsByFinishingType[Rooms.First().RoomFinishingType];

            SharedParamsConfig paramConfig = SharedParamsConfig.Instance;

            _revitElement.SetParamValue(paramConfig.SizeLengthAdditional,
                _revitElement.GetParamValue<double>(BuiltInParameter.HOST_PERIMETER_COMPUTED));
            _revitElement.SetParamValue(paramConfig.FloorFinishingOrder,
                                        finishingType.GetFloorOrder(_revitElement.Name));
        }
    }
}
