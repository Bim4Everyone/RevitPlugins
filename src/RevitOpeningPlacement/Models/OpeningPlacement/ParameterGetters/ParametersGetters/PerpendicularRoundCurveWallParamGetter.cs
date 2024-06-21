using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitClashDetective.Models.Value;

using RevitOpeningPlacement.Models.Configs;
using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.OpeningPlacement.ValueGetters;

namespace RevitOpeningPlacement.Models.OpeningPlacement.ParameterGetters {
    internal class PerpendicularRoundCurveWallParamGetter : IParametersGetter {
        private readonly MepCurveClash<Wall> _clash;
        private readonly MepCategory _mepCategory;
        private readonly IPointFinder _pointFinder;
        private readonly OpeningType _openingType;
        private readonly ILevelFinder _levelFinder;

        public PerpendicularRoundCurveWallParamGetter(MepCurveClash<Wall> clash, MepCategory mepCategory, IPointFinder pointFinder, OpeningType openingType, ILevelFinder levelFinder) {
            _clash = clash ?? throw new System.ArgumentNullException(nameof(clash));
            _mepCategory = mepCategory ?? throw new System.ArgumentNullException(nameof(mepCategory));
            _pointFinder = pointFinder ?? throw new System.ArgumentNullException(nameof(pointFinder));
            _levelFinder = levelFinder ?? throw new System.ArgumentNullException(nameof(levelFinder));
            _openingType = openingType;
        }

        public IEnumerable<ParameterValuePair> GetParamValues() {
            var openingSizeGetter = new DiameterValueGetter(_clash.Element1, _mepCategory);
            var openingTaskIsRound = OpeningTaskIsRound();
            //габариты отверстия
            if(openingTaskIsRound) {
                // отверстие круглое
                yield return new DoubleParameterGetter(RevitRepository.OpeningDiameter, openingSizeGetter).GetParamValue();

            } else {
                // отверстие прямоугольное (квадратное)
                yield return new DoubleParameterGetter(RevitRepository.OpeningHeight, openingSizeGetter).GetParamValue();
                yield return new DoubleParameterGetter(RevitRepository.OpeningWidth, openingSizeGetter).GetParamValue();
            }
            yield return new DoubleParameterGetter(RevitRepository.OpeningThickness, new WallThicknessValueGetter(_clash.Element2)).GetParamValue();

            //отметки отверстия
            IValueGetter<DoubleParamValue> bottomOffsetValueGetter;
            if(openingTaskIsRound) {
                //отверстие круглое
                yield return new DoubleParameterGetter(RevitRepository.OpeningOffsetCenter, new CenterOffsetValueGetter(_pointFinder)).GetParamValue();
                bottomOffsetValueGetter = new BottomOffsetValueGetter(_pointFinder, openingSizeGetter);
                yield return new DoubleParameterGetter(RevitRepository.OpeningOffsetBottom, bottomOffsetValueGetter).GetParamValue();
            } else {
                // отверстие прямоугольное (квадратное)
                yield return new DoubleParameterGetter(RevitRepository.OpeningOffsetCenter, new CenterOffsetOfRectangleOpeningInWallValueGetter(_pointFinder, openingSizeGetter)).GetParamValue();
                bottomOffsetValueGetter = new BottomOffsetOfRectangleOpeningInWallValueGetter(_pointFinder);
                yield return new DoubleParameterGetter(RevitRepository.OpeningOffsetBottom, bottomOffsetValueGetter).GetParamValue();
            }
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
            return _openingType == OpeningType.WallRound;
        }
    }
}
