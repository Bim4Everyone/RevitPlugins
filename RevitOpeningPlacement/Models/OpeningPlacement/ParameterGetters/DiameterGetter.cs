using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using RevitClashDetective.Models.Value;

using RevitOpeningPlacement.Models.Configs;
using RevitOpeningPlacement.Models.Extensions;
using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.OpeningPlacement.ParameterGetters {
    internal class DiameterGetter : IParameterGetter<DoubleParamValue> {
        private readonly MepCurveWallClash _clash;
        private readonly MepCategory _categoryOptions;

        public DiameterGetter(MepCurveWallClash clash, MepCategory categoryOptions) {
            _clash = clash;
            _categoryOptions = categoryOptions;
        }

        public ParameterValuePair<DoubleParamValue> GetParamValue() {
            var diameter = _clash.Curve.GetDiameter();
            var offset = _categoryOptions.Offsets
                .Select(item=>new Offset() { 
                    From = TransformToInternalUnits(item.From),
                    To = TransformToInternalUnits(item.To),
                    OffsetValue = TransformToInternalUnits(item.OffsetValue)})
                .FirstOrDefault(item => item.From <= diameter && item.To >= diameter);
            if(offset != null) {
                diameter += offset.OffsetValue * 2;
            }

            return new ParameterValuePair<DoubleParamValue>() {
                ParamName = RevitRepository.OpeningDiameter,
                TValue = new DoubleParamValue(diameter)
            };
        }
#if REVIT_2020_OR_LESS
        private double TransformToInternalUnits(double mmValue) {
            return UnitUtils.ConvertToInternalUnits(mmValue, DisplayUnitType.DUT_MILLIMETERS);
        }
#else
        private double TransformToInternalUnits(double mmValue) {
            return UnitUtils.ConvertToInternalUnits(mmValue, UnitTypeId.Millimeters);
        }
#endif
    }
}
