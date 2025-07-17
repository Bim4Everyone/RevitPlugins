using System.Linq;

using Autodesk.Revit.DB;

using RevitFinishing.Services;

namespace RevitFinishing.Models.Finishing;
internal class FinishingBaseboard : FinishingElement {
    public FinishingBaseboard(Element element, ParamCalculationService paramService)
        : base(element, paramService) {
    }

    public override void UpdateCategoryParameters(FinishingCalculator calculator) {
        FinishingType finishingType = calculator.RoomsByFinishingType[Rooms.First().RoomFinishingType];

        UpdateFromInstParam(_paramConfig.SizeLengthAdditional, BuiltInParameter.CURVE_ELEM_LENGTH);
        UpdateOrderParam(_paramConfig.BaseboardFinishingOrder, finishingType.GetBaseboardOrder(_revitElement.Name));
    }
}
