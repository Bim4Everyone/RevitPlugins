using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitOpeningPlacement.Models.Configs;
using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.OpeningPlacement.ValueGetters;

namespace RevitOpeningPlacement.Models.OpeningPlacement.ParameterGetters {
    internal class PerpendicularRoundCurveFloorParamGetter : IParametersGetter {
        private readonly MepCurveClash<CeilingAndFloor> _clash;
        private readonly MepCategory _mepCategory;
        private readonly IPointFinder _pointFinder;

        public PerpendicularRoundCurveFloorParamGetter(MepCurveClash<CeilingAndFloor> clash, MepCategory mepCategory, IPointFinder pointFinder) {
            _clash = clash;
            _mepCategory = mepCategory;
            _pointFinder = pointFinder;
        }

        public IEnumerable<ParameterValuePair> GetParamValues() {
            //габариты отверстия
            var sizeInit = new FloorOpeningSizeInitializer(_clash.GetIntersection(), _mepCategory);
            yield return new DoubleParameterGetter(RevitRepository.OpeningDiameter, new DiameterValueGetter(_clash.Element1, _mepCategory)).GetParamValue();
            yield return new DoubleParameterGetter(RevitRepository.OpeningThickness, sizeInit.GetThickness()).GetParamValue();

            //отметки отверстия
            yield return new DoubleParameterGetter(RevitRepository.OpeningOffsetBottom, new BottomOffsetOfOpeningInFloorValueGetter(_pointFinder, sizeInit.GetThickness())).GetParamValue();

            //текстовые данные отверстия
            yield return new StringParameterGetter(RevitRepository.OpeningDate, new DateValueGetter()).GetParamValue();
            yield return new StringParameterGetter(RevitRepository.OpeningDescription, new DescriptionValueGetter(_clash.Element1, _clash.Element2)).GetParamValue();
            yield return new StringParameterGetter(RevitRepository.OpeningMepSystem, new MepSystemValueGetter(_clash.Element1)).GetParamValue();
            yield return new StringParameterGetter(RevitRepository.OpeningAuthor, new UsernameGetter(_clash.Element1.Document.Application)).GetParamValue();
        }
    }
}
