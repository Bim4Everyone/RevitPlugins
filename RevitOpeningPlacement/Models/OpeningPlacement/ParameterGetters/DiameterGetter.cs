using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using RevitClashDetective.Models.Value;

using RevitOpeningPlacement.Models.Extensions;
using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.OpeningPlacement.ParameterGetters {
    internal class DiameterGetter : IParameterGetter<DoubleParamValue> {
        private readonly MepCurveWallClash _clash;

        public DiameterGetter(MepCurveWallClash clash) {
            _clash = clash;
        }

        public ParameterValuePair<DoubleParamValue> GetParamValue() {
            return new ParameterValuePair<DoubleParamValue>() {
                ParamName = RevitRepository.OpeningDiameter,
                TValue = new DoubleParamValue(_clash.Curve.GetDiameter())
            };
        }
    }
}
