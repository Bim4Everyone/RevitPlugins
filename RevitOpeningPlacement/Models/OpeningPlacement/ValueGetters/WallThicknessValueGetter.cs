
using Autodesk.Revit.DB;

using RevitClashDetective.Models.Value;

using RevitOpeningPlacement.Models.Exceptions;
using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.OpeningPlacement.ValueGetters {
    internal class WallThicknessValueGetter : IValueGetter<DoubleParamValue> {
        private readonly Wall _wall;

        /// <summary>
        /// Минимальное значение габарита задания на отверстие в футах (5 мм)
        /// </summary>
        private const double _minGeometryFeetSize = 0.015;

        public WallThicknessValueGetter(Wall wall) {
            _wall = wall ?? throw new System.ArgumentNullException(nameof(wall));
        }

        public DoubleParamValue GetValue() {
            //проверка на недопустимо малые габариты
            if(_wall.Width < _minGeometryFeetSize) {
                throw new SizeTooSmallException("Заданный габарит отверстия слишком мал");
            }

            return new DoubleParamValue(_wall.Width);
        }
    }
}
