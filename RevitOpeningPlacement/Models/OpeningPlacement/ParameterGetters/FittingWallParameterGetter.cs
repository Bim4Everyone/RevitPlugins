using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitOpeningPlacement.Models.Configs;
using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.OpeningPlacement.ValueGetters;

namespace RevitOpeningPlacement.Models.OpeningPlacement.ParameterGetters {
    internal class FittingWallParameterGetter : IParametersGetter {
        private readonly FittingClash<Wall> _clash;
        private readonly IAngleFinder _angleFinder;
        private readonly MepCategory[] _mepCategories;

        public FittingWallParameterGetter(FittingClash<Wall> clash, IAngleFinder angleFinder, params MepCategory[] mepCategories) {
            _clash = clash;
            _angleFinder = angleFinder;
            _mepCategories = mepCategories;
        }
        public IEnumerable<ParameterValuePair> GetParamValues() {
            var sizeInit = new WallOpeningSizeInitializer(_clash.GetRotatedIntersection(_angleFinder), _mepCategories);
            yield return new DoubleParameterGetter(RevitRepository.OpeningHeight, sizeInit.GetHeight()).GetParamValue();
            yield return new DoubleParameterGetter(RevitRepository.OpeningWidth, sizeInit.GetWidth()).GetParamValue();
            yield return new DoubleParameterGetter(RevitRepository.OpeningThickness, sizeInit.GetThickness()).GetParamValue();
        }
    }
}
