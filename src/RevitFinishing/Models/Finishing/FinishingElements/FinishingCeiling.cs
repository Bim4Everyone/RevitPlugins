using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SharedParams;
using dosymep.Revit;

namespace RevitFinishing.Models.Finishing.FinishingElements {
    internal class FinishingCeiling : FinishingElement {
        public FinishingCeiling(Element element, FinishingCalculator calculator)
            : base(element, calculator) {
        }

        public override void UpdateCategoryParameters() {
            FinishingType finishingType = _calculator.RoomsByFinishingType[Rooms.First().RoomFinishingType];

            SharedParamsConfig paramConfig = SharedParamsConfig.Instance;

            _revitElement.SetParamValue(paramConfig.SizeLengthAdditional,
                _revitElement.GetParamValue<double>(BuiltInParameter.HOST_PERIMETER_COMPUTED));
            _revitElement.SetParamValue(paramConfig.CeilingFinishingOrder,
                                        finishingType.GetCeilingOrder(_revitElement.Name));
        }
    }
}
