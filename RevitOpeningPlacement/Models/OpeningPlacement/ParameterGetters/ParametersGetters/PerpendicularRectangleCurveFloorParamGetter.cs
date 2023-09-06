using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitOpeningPlacement.Models.Configs;
using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.OpeningPlacement.ValueGetters;

namespace RevitOpeningPlacement.Models.OpeningPlacement.ParameterGetters {
    internal class PerpendicularRectangleCurveFloorParamGetter : IParametersGetter {
        private readonly MepCurveClash<CeilingAndFloor> _clash;
        private readonly MepCategory _mepCategory;
        private readonly IPointFinder _pointFinder;

        public PerpendicularRectangleCurveFloorParamGetter(MepCurveClash<CeilingAndFloor> clash, MepCategory mepCategory, IPointFinder pointFinder) {
            _clash = clash;
            _mepCategory = mepCategory;
            _pointFinder = pointFinder;
        }

        public IEnumerable<ParameterValuePair> GetParamValues() {
            var floorThicknessValueGetter = new FloorThicknessValueGetter(_clash);

            //габариты отверстия
            yield return new DoubleParameterGetter(RevitRepository.OpeningHeight, new HeightValueGetter(_clash.Element1, _mepCategory)).GetParamValue();
            yield return new DoubleParameterGetter(RevitRepository.OpeningWidth, new WidthValueGetter(_clash.Element1, _mepCategory)).GetParamValue();
            yield return new DoubleParameterGetter(RevitRepository.OpeningThickness, floorThicknessValueGetter).GetParamValue();

            //отметки отверстия
            yield return new DoubleParameterGetter(RevitRepository.OpeningOffsetBottom, new BottomOffsetOfOpeningInFloorValueGetter(_pointFinder, floorThicknessValueGetter)).GetParamValue();

            //текстовые данные отверстия
            yield return new StringParameterGetter(RevitRepository.OpeningDate, new DateValueGetter()).GetParamValue();
            yield return new StringParameterGetter(RevitRepository.OpeningDescription, new DescriptionValueGetter(_clash.Element1, _clash.Element2)).GetParamValue();
            yield return new StringParameterGetter(RevitRepository.OpeningMepSystem, new MepSystemValueGetter(_clash.Element1)).GetParamValue();
            yield return new StringParameterGetter(RevitRepository.OpeningAuthor, new UsernameGetter(_clash.Element1.Document.Application)).GetParamValue();

            //флаг для автоматической расстановки
            yield return new IntegerParameterGetter(RevitRepository.OpeningIsManuallyPlaced, new IsManuallyPlacedValueGetter()).GetParamValue();
        }
    }
}
