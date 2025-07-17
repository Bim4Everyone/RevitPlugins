using System.Linq;

using Autodesk.Revit.DB;

using RevitFinishing.Services;

namespace RevitFinishing.Models.Finishing;
internal class FinishingFloor : FinishingElement {
    public FinishingFloor(Element element, ParamCalculationService paramService)
        : base(element, paramService) {
    }

    public override void UpdateCategoryParameters(FinishingCalculator calculator) {
        FinishingType finishingType = calculator.RoomsByFinishingType[Rooms.First().RoomFinishingType];

        UpdateFromInstParam(_paramConfig.SizeLengthAdditional, BuiltInParameter.HOST_PERIMETER_COMPUTED);
        UpdateOrderParam(_paramConfig.FloorFinishingOrder, finishingType.GetFloorOrder(_revitElement.Name));
    }
}
