using System;

using Autodesk.Revit.DB;

using RevitOpeningPlacement.Models.Configs;
using RevitOpeningPlacement.Models.Extensions;
using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.OpeningPlacement.Providers {
    /// <summary>
    /// Класс, предоставляющий тип проема задания на отверстие для круглых элементов инженерных систем, пересекающихся с перекрытием перпендикулярно ему
    /// </summary>
    internal class RoundMepFloorOpeningTaskTypeProvider : IOpeningTaskTypeProvider {
        private readonly MEPCurve _mepCurve;
        private readonly MepCategory _categoryOption;

        public RoundMepFloorOpeningTaskTypeProvider(MEPCurve mepCurve, MepCategory categoryOption) {
            _mepCurve = mepCurve ?? throw new ArgumentNullException(nameof(mepCurve));
            _categoryOption = categoryOption ?? throw new ArgumentNullException(nameof(categoryOption));
        }


        public OpeningType GetOpeningTaskType() {
            double diameter = _mepCurve.GetDiameter();
            Offset offset = _categoryOption.GetOffsetTransformedToInternalUnit(diameter);

            var selectedTypeName = offset?.OpeningTypeName ?? string.Empty;
            if(!string.IsNullOrWhiteSpace(selectedTypeName)
                && selectedTypeName.Equals(RevitRepository.OpeningTaskTypeName[OpeningType.FloorRound])) {

                return OpeningType.FloorRound;

            } else {
                return OpeningType.FloorRectangle;
            }
        }
    }
}
