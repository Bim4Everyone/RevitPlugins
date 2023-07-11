using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitOpeningPlacement.Models.Configs;
using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.OpeningPlacement.ValueGetters;

namespace RevitOpeningPlacement.Models.OpeningPlacement.ParameterGetters {
    internal class PerpendicularRoundCurveWallParamGetter : IParametersGetter {
        private readonly MepCurveClash<Wall> _clash;
        private readonly MepCategory _mepCategory;
        private readonly IPointFinder _pointFinder;

        public PerpendicularRoundCurveWallParamGetter(MepCurveClash<Wall> clash, MepCategory mepCategory, IPointFinder pointFinder) {
            _clash = clash;
            _mepCategory = mepCategory;
            _pointFinder = pointFinder;
        }

        public IEnumerable<ParameterValuePair> GetParamValues() {
            var openingDiameterGetter = new DiameterValueGetter(_clash.Element1, _mepCategory);
            //габариты отверстия
            yield return new DoubleParameterGetter(RevitRepository.OpeningDiameter, openingDiameterGetter).GetParamValue();
            yield return new DoubleParameterGetter(RevitRepository.OpeningThickness, new WallThicknessValueGetter(_clash)).GetParamValue();

            //отметки отверстия
            yield return new DoubleParameterGetter(RevitRepository.OpeningOffsetCenter, new CenterOffsetValueGetter(_pointFinder)).GetParamValue();
            yield return new DoubleParameterGetter(RevitRepository.OpeningOffsetBottom, new BottomOffsetValueGetter(_pointFinder, openingDiameterGetter)).GetParamValue();

            //текстовые данные отверстия
            yield return new StringParameterGetter(RevitRepository.OpeningDate, new DateValueGetter()).GetParamValue();
            yield return new StringParameterGetter(RevitRepository.OpeningDescription, new DescriptionValueGetter(_clash.Element1, _clash.Element2)).GetParamValue();
            yield return new StringParameterGetter(RevitRepository.OpeningMepSystem, new MepSystemValueGetter(_clash.Element1)).GetParamValue();
        }
    }
}
