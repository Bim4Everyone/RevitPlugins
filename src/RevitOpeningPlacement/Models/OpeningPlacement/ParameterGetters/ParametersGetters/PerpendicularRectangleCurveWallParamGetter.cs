using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitOpeningPlacement.Models.Configs;
using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.OpeningPlacement.ValueGetters;

namespace RevitOpeningPlacement.Models.OpeningPlacement.ParameterGetters {
    internal class PerpendicularRectangleCurveWallParamGetter : IParametersGetter {
        private readonly MepCurveClash<Wall> _clash;
        private readonly MepCategory _mepCategory;
        private readonly IPointFinder _pointFinder;
        private readonly ILevelFinder _levelFinder;

        public PerpendicularRectangleCurveWallParamGetter(MepCurveClash<Wall> clash, MepCategory mepCategory, IPointFinder pointFinder, ILevelFinder levelFinder) {
            _clash = clash ?? throw new System.ArgumentNullException(nameof(clash));
            _mepCategory = mepCategory ?? throw new System.ArgumentNullException(nameof(mepCategory));
            _pointFinder = pointFinder ?? throw new System.ArgumentNullException(nameof(pointFinder));
            _levelFinder = levelFinder ?? throw new System.ArgumentNullException(nameof(levelFinder));
        }

        public IEnumerable<ParameterValuePair> GetParamValues() {
            var heightValueGetter = new HeightValueGetter(_clash.Element1, _mepCategory);

            //габариты отверстия
            yield return new DoubleParameterGetter(RevitRepository.OpeningHeight, heightValueGetter).GetParamValue();
            yield return new DoubleParameterGetter(RevitRepository.OpeningWidth, new WidthValueGetter(_clash.Element1, _mepCategory)).GetParamValue();
            yield return new DoubleParameterGetter(RevitRepository.OpeningThickness, new WallThicknessValueGetter(_clash.Element2)).GetParamValue();

            //отметки отверстия
            yield return new DoubleParameterGetter(RevitRepository.OpeningOffsetCenter, new CenterOffsetOfRectangleOpeningInWallValueGetter(_pointFinder, heightValueGetter)).GetParamValue();
            var bottomOffsetValueGetter = new BottomOffsetOfRectangleOpeningInWallValueGetter(_pointFinder);
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
    }
}
