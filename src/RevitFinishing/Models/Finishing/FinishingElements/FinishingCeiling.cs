using System.Linq;

using Autodesk.Revit.DB;

using RevitFinishing.Services;

namespace RevitFinishing.Models.Finishing;
internal class FinishingCeiling : FinishingElement {
    private readonly ElementId _wallCategory = new(BuiltInCategory.OST_Walls);
    private readonly ElementId _ceilingCategory = new(BuiltInCategory.OST_Ceilings);

    public FinishingCeiling(Element element, FinishingCalculator calculator, ParamCalculationService paramService)
        : base(element, calculator, paramService) {
    }

    public override void UpdateCategoryParameters() {
        FinishingType finishingType = _calculator.RoomsByFinishingType[Rooms.First().RoomFinishingType];

        if(_revitElement.Category.Id == _wallCategory) {
            UpdateFromInstParam(_paramConfig.SizeLengthAdditional, BuiltInParameter.CURVE_ELEM_LENGTH);
        }
        if(_revitElement.Category.Id == _ceilingCategory) {
            UpdateFromInstParam(_paramConfig.SizeLengthAdditional, BuiltInParameter.HOST_PERIMETER_COMPUTED);
        }

        UpdateOrderParam(_paramConfig.CeilingFinishingOrder, finishingType.GetCeilingOrder(_revitElement.Name));
    }
}
