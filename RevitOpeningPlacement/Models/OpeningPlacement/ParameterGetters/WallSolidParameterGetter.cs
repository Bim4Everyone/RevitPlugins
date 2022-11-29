using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitOpeningPlacement.Models.Configs;
using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.OpeningPlacement.ValueGetters;

namespace RevitOpeningPlacement.Models.OpeningPlacement.ParameterGetters {
    internal class WallSolidParameterGetter : IParametersGetter {
        private readonly ISolidProvider _solidProvider;
        private readonly MepCategory[] _mepCategories;

        public WallSolidParameterGetter(ISolidProvider solidProvider, params MepCategory[] mepCategories) {
            _solidProvider = solidProvider;
            _mepCategories = mepCategories;
        }
        public IEnumerable<ParameterValuePair> GetParamValues() {
            var sizeInit = new WallOpeningSizeInitializer(_solidProvider.GetSolid(), _mepCategories);
            yield return new DoubleParameterGetter(RevitRepository.OpeningHeight, sizeInit.GetHeight()).GetParamValue();
            yield return new DoubleParameterGetter(RevitRepository.OpeningWidth, sizeInit.GetWidth()).GetParamValue();
            yield return new DoubleParameterGetter(RevitRepository.OpeningThickness, sizeInit.GetThickness()).GetParamValue();
        }
    }
}
