using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitOpeningPlacement.Models.Configs;
using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.OpeningPlacement.ParameterGetters {
    internal class InclinedRoundCurveWallParameterGetter : IParametersGetter {
        private readonly MepCurveWallClash _clash;
        private readonly MepCategory _mepCategory;

        public InclinedRoundCurveWallParameterGetter(MepCurveWallClash clash, MepCategory mepCategory) {
            _clash = clash;
            _mepCategory = mepCategory;
        }

        //Получение ширины и высоты задания на отверстия с учетом наклона систем в горизонтальном и вертикальном направлении.

        //Размеры получаются следующим образом: осевую линию инженерной системы смещают на размер (например, радиус) (в положительную и отрицательную стороны)
        //в плоскостях yOz (для получения высоты) и в xOy (для получения ширины).
        //Далее для каждой плоскости находятся точки пересечения смещенных линий системы с гранями стены, затем из этих точек выбираются те,
        //которые находятся на максимальном расстоянии друг от друга, далее по теореме Пифагора производится расчет размера.
        public IEnumerable<ParameterValuePair> GetParamValues() {
            yield return new InclinedSizeInitializer(_clash, _mepCategory).GetRoundMepHeightGetter().GetParamValue();
            yield return new InclinedSizeInitializer(_clash, _mepCategory).GetRoundMepWidthGetter().GetParamValue();
            yield return new ThicknessGetter(_clash).GetParamValue();
        }
    }
}
