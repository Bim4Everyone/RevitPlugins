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
        private readonly OpeningType _openingType;
        private readonly ILevelFinder _levelFinder;

        public PerpendicularRoundCurveFloorParamGetter(MepCurveClash<CeilingAndFloor> clash, MepCategory mepCategory, IPointFinder pointFinder, OpeningType openingType, ILevelFinder levelFinder) {
            _clash = clash ?? throw new System.ArgumentNullException(nameof(clash));
            _mepCategory = mepCategory ?? throw new System.ArgumentNullException(nameof(mepCategory));
            _pointFinder = pointFinder ?? throw new System.ArgumentNullException(nameof(pointFinder));
            _levelFinder = levelFinder ?? throw new System.ArgumentNullException(nameof(levelFinder));
            _openingType = openingType;
        }

        public IEnumerable<ParameterValuePair> GetParamValues() {
            var floorThicknessValueGetter = new FloorThicknessValueGetter(_clash.Element2);
            var diameterValueGetter = new DiameterValueGetter(_clash.Element1, _mepCategory);

            //габариты отверстия
            if(OpeningTaskIsRound()) {
                // круглое задание на отверстие
                yield return new DoubleParameterGetter(RevitRepository.OpeningDiameter, diameterValueGetter).GetParamValue();

            } else {
                // прямоугольное (квадратное) задание на отверстие
                yield return new DoubleParameterGetter(RevitRepository.OpeningHeight, diameterValueGetter).GetParamValue();
                yield return new DoubleParameterGetter(RevitRepository.OpeningWidth, diameterValueGetter).GetParamValue();
            }
            yield return new DoubleParameterGetter(RevitRepository.OpeningThickness, floorThicknessValueGetter).GetParamValue();

            //отметки отверстия
            var bottomOffsetValueGetter = new BottomOffsetOfOpeningInFloorValueGetter(_pointFinder, floorThicknessValueGetter);
            yield return new DoubleParameterGetter(RevitRepository.OpeningOffsetBottom, bottomOffsetValueGetter).GetParamValue();
            yield return new DoubleParameterGetter(RevitRepository.OpeningOffsetBottomAdsk, new BottomOffsetInFeetValueGetter(bottomOffsetValueGetter)).GetParamValue();
            yield return new DoubleParameterGetter(RevitRepository.OpeningOffsetFromLevelAdsk, new BottomOffsetFromLevelValueGetter(bottomOffsetValueGetter, _levelFinder)).GetParamValue();
            yield return new DoubleParameterGetter(RevitRepository.OpeningLevelOffsetAdsk, new LevelOffsetValueGetter(_levelFinder)).GetParamValue();

            //текстовые данные отверстия
            yield return new StringParameterGetter(RevitRepository.OpeningDate, new DateValueGetter()).GetParamValue();
            yield return new StringParameterGetter(RevitRepository.OpeningDescription, new DescriptionValueGetter(_clash.Element1, _clash.Element2)).GetParamValue();
            yield return new StringParameterGetter(RevitRepository.OpeningMepSystem, new MepSystemValueGetter(_clash.Element1)).GetParamValue();
            yield return new StringParameterGetter(RevitRepository.OpeningAuthor, new UsernameGetter(_clash.Element1.Document.Application)).GetParamValue();

            //флаг для автоматической расстановки
            yield return new IntegerParameterGetter(RevitRepository.OpeningIsManuallyPlaced, new IsManuallyPlacedValueGetter()).GetParamValue();
        }

        /// <summary>
        /// True - если размещаемое задание на отверстие - круглое, иначе False - задание на отверстие прямоугольное (квадратное)
        /// </summary>
        /// <returns></returns>
        private bool OpeningTaskIsRound() {
            return _openingType == OpeningType.FloorRound;
        }
    }
}
