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
    internal class FinishingCeiling : FinishingElement {
        private readonly ElementId _wallCategory = new ElementId(BuiltInCategory.OST_Walls);
        private readonly ElementId _ceilingCategory = new ElementId(BuiltInCategory.OST_Ceilings);

        public FinishingCeiling(Element element, FinishingCalculator calculator)
            : base(element, calculator) {
        }

        public override void UpdateCategoryParameters() {
            FinishingType finishingType = _calculator.RoomsByFinishingType[Rooms.First().RoomFinishingType];

            SharedParamsConfig paramConfig = SharedParamsConfig.Instance;

            if(_revitElement.Category.Id == _wallCategory) {
                _revitElement.SetParamValue(paramConfig.SizeLengthAdditional,
                    _revitElement.GetParamValue<double>(BuiltInParameter.CURVE_ELEM_LENGTH));
            }
            if(_revitElement.Category.Id == _ceilingCategory) {
                _revitElement.SetParamValue(paramConfig.SizeLengthAdditional,
                    _revitElement.GetParamValue<double>(BuiltInParameter.HOST_PERIMETER_COMPUTED));
            }

            _revitElement.SetParamValue(paramConfig.CeilingFinishingOrder,
                                        finishingType.GetCeilingOrder(_revitElement.Name));
        }
    }
}
