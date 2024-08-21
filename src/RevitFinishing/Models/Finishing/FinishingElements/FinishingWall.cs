using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SharedParams;
using dosymep.Revit;

namespace RevitFinishing.Models.Finishing.FinishingElements {
    internal class FinishingWall : FinishingElement {
        public FinishingWall(Element element, FinishingCalculator calculator) 
            : base(element, calculator) {
        }

        public override void UpdateCategoryParameters() {
            FinishingType finishingType = _calculator.RoomsByFinishingType[Rooms.First().RoomFinishingType];

            SharedParamsConfig paramConfig = SharedParamsConfig.Instance;

            _revitElement.SetParamValue(paramConfig.SizeLengthAdditional,
                            _revitElement.GetParamValue<double>(BuiltInParameter.CURVE_ELEM_LENGTH));
            _revitElement.SetParamValue(paramConfig.WallFinishingOrder,
                                        finishingType.GetWallOrder(_revitElement.Name));
        }
    }
}
