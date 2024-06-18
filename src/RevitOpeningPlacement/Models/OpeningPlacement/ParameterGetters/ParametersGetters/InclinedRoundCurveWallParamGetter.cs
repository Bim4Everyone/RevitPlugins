using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitOpeningPlacement.Models.Configs;
using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.OpeningPlacement.ValueGetters;

namespace RevitOpeningPlacement.Models.OpeningPlacement.ParameterGetters {
    internal class InclinedRoundCurveWallParamGetter : IParametersGetter {
        private readonly MepCurveClash<Wall> _clash;
        private readonly MepCategory _mepCategory;
        private readonly IPointFinder _pointFinder;
        private readonly ILevelFinder _levelFinder;

        public InclinedRoundCurveWallParamGetter(MepCurveClash<Wall> clash, MepCategory mepCategory, IPointFinder pointFinder, ILevelFinder levelFinder) {
            _clash = clash ?? throw new System.ArgumentNullException(nameof(clash));
            _mepCategory = mepCategory ?? throw new System.ArgumentNullException(nameof(mepCategory));
            _pointFinder = pointFinder ?? throw new System.ArgumentNullException(nameof(pointFinder));
            _levelFinder = levelFinder ?? throw new System.ArgumentNullException(nameof(levelFinder));
        }

        //Получение ширины и высоты задания на отверстия с учетом наклона систем в горизонтальном и вертикальном направлении.

        //Размеры получаются следующим образом: осевую линию инженерной системы смещают на размер (например, радиус) (в положительную и отрицательную стороны)
        //в плоскостях перпендикулярных стене (вертикальной и горизонтальной). Таким образом, инженерная система проецируется на заданные плоскости.
        //Далее для каждой плоскости находятся точки пересечения смещенных линий системы с гранями стены, затем из этих точек выбираются те,
        //которые находятся на максимальном расстоянии друг от друга, далее по теореме Пифагора производится расчет размера.
        public IEnumerable<ParameterValuePair> GetParamValues() {
            var heightValueGetter = new InclinedSizeInitializer(_clash, _mepCategory).GetRoundMepHeightGetter();

            //габариты отверстия
            yield return new DoubleParameterGetter(RevitRepository.OpeningHeight, heightValueGetter).GetParamValue();
            yield return new DoubleParameterGetter(RevitRepository.OpeningWidth, new InclinedSizeInitializer(_clash, _mepCategory).GetRoundMepWidthGetter()).GetParamValue();
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
