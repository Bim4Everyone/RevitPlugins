using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitOpeningPlacement.Models.Configs;
using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.OpeningPlacement.ValueGetters;

namespace RevitOpeningPlacement.Models.OpeningPlacement.ParameterGetters {
    internal class PerpendicularRoundCurveFloorParamterGetter : IParametersGetter {
        private readonly MepCurveClash<CeilingAndFloor> _clash;
        private readonly MepCategory _mepCategory;

        public PerpendicularRoundCurveFloorParamterGetter(MepCurveClash<CeilingAndFloor> clash, MepCategory mepCategory) {
            _clash = clash;
            _mepCategory = mepCategory;
        }

        public IEnumerable<ParameterValuePair> GetParamValues() {
            var sizeInit = new FloorOpeningSizeInitializer(new IntersectionGetter<CeilingAndFloor>(_clash).GetIntersection(), _mepCategory);
            yield return new DoubleParameterGetter(RevitRepository.OpeningDiameter, new DiameterValueGetter(_clash.Curve, _mepCategory)).GetParamValue();
            yield return new DoubleParameterGetter(RevitRepository.OpeningThickness, sizeInit.GetThickness()).GetParamValue();
        }
    }
}
