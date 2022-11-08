using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitOpeningPlacement.Models.Configs;
using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.OpeningPlacement.ValueGetters;

namespace RevitOpeningPlacement.Models.OpeningPlacement.ParameterGetters {
    internal class InclinedRoundCurveWallParamGetter : IParametersGetter {
        private readonly MepCurveClash<Wall> _clash;
        private readonly MepCategory _mepCategory;

        public InclinedRoundCurveWallParamGetter(MepCurveClash<Wall> clash, MepCategory mepCategory) {
            _clash = clash;
            _mepCategory = mepCategory;
        }

        //Получение ширины и высоты задания на отверстия с учетом наклона систем в горизонтальном и вертикальном направлении.

        //Размеры получаются следующим образом: осевую линию инженерной системы смещают на размер (например, радиус) (в положительную и отрицательную стороны)
        //в плоскостях перпендикулярных стене (вертикальной и горизонтальной). Таким образом, инженерная система проецируется на задаынные плоскости.
        //Далее для каждой плоскости находятся точки пересечения смещенных линий системы с гранями стены, затем из этих точек выбираются те,
        //которые находятся на максимальном расстоянии друг от друга, далее по теореме Пифагора производится расчет размера.
        public IEnumerable<ParameterValuePair> GetParamValues() {
            yield return new DoubleParameterGetter(RevitRepository.OpeningHeight, new InclinedSizeInitializer(_clash, _mepCategory).GetRoundMepHeightGetter()).GetParamValue();
            yield return new DoubleParameterGetter(RevitRepository.OpeningWidth, new InclinedSizeInitializer(_clash, _mepCategory).GetRoundMepWidthGetter()).GetParamValue();
            yield return new DoubleParameterGetter(RevitRepository.OpeningThickness, new WallThicknessValueGetter(_clash)).GetParamValue();
        }
    }
}
