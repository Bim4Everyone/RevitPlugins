using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitOpeningPlacement.Models.Configs;
using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.OpeningPlacement.ValueGetters;

namespace RevitOpeningPlacement.Models.OpeningPlacement.ParameterGetters {
    internal class PerpendicularRoundCurveWallParamterGetter : IParametersGetter {
        private readonly MepCurveClash<Wall> _clash;
        private readonly MepCategory _mepCategory;

        public PerpendicularRoundCurveWallParamterGetter(MepCurveClash<Wall> clash, MepCategory mepCategory) {
            _clash = clash;
            _mepCategory = mepCategory;
        }

        public IEnumerable<ParameterValuePair> GetParamValues() {
            yield return new DoubleParameterGetter(RevitRepository.OpeningDiameter, new DiameterValueGetter(_clash, _mepCategory)).GetParamValue();
            yield return new DoubleParameterGetter(RevitRepository.OpeningThickness, new ThicknessValueGetter(_clash)).GetParamValue();
        }
    }
}
