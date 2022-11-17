using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitOpeningPlacement.Models.Configs;
using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.OpeningPlacement.ValueGetters;

namespace RevitOpeningPlacement.Models.OpeningPlacement.ParameterGetters {
    internal class InclinedFloorParameterGetter<T> : IParametersGetter where T : Element {
        private readonly Clash<T, CeilingAndFloor> _clash;
        private readonly MepCategory[] _mepCategories;

        public InclinedFloorParameterGetter(Clash<T, CeilingAndFloor> clash, params MepCategory[] mepCategories) {
            _clash = clash;
            _mepCategories = mepCategories;
        }

        public IEnumerable<ParameterValuePair> GetParamValues() {
            var sizeInit = new FloorOpeningSizeInitializer(_clash.GetIntersection(), _mepCategories);
            yield return new DoubleParameterGetter(RevitRepository.OpeningHeight, sizeInit.GetHeight()).GetParamValue();
            yield return new DoubleParameterGetter(RevitRepository.OpeningWidth, sizeInit.GetWidth()).GetParamValue();
            yield return new DoubleParameterGetter(RevitRepository.OpeningThickness, sizeInit.GetThickness()).GetParamValue();
        }
    }
}
