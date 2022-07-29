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
            diameter += _categoryOptions.GetOffset(diameter);

            return new ParameterValuePair<DoubleParamValue>() {
                ParamName = RevitRepository.OpeningDiameter,
                TValue = new DoubleParamValue(diameter)
            };
        }

    }
}
