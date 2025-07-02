using System.Linq;

using Autodesk.Revit.DB;

using RevitFinishing.Services;

namespace RevitFinishing.Models.Finishing;
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
