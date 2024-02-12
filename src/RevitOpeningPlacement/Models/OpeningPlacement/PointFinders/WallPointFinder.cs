using System;

using Autodesk.Revit.DB;

using RevitClashDetective.Models.Value;

using RevitOpeningPlacement.Models.Exceptions;
using RevitOpeningPlacement.Models.Extensions;
using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.OpeningPlacement.ValueGetters;

namespace RevitOpeningPlacement.Models.OpeningPlacement.PointFinders {
    internal class WallPointFinder : RoundValueGetter, IPointFinder {
        private readonly MepCurveClash<Wall> _clash;
        private readonly IValueGetter<DoubleParamValue> _rectangleHeightGetter;
        /// <summary>
        /// Округление высотной отметки отверстия в мм
        /// </summary>
        private const int _heightRound = 10;

        /// <summary>
        /// Конструктор класса, предоставляющего точку вставки задания на отверстие в стене
        /// </summary>
        /// <param name="clash">Пересечение линейного инженерного элемента со стеной</param>
        /// <param name="rectangleHeightGetter">
        /// Высота прямоугольного отверстия - опциональный параметр для прямоугольных отверстий.
        /// Нужен для корректировки точки вставки, т.к. точка вставки прямоугольного отверстия в стене находится у нижней грани.
        /// </param>
        public WallPointFinder(MepCurveClash<Wall> clash, IValueGetter<DoubleParamValue> rectangleHeightGetter = null) {
            _clash = clash;
            _rectangleHeightGetter = rectangleHeightGetter;
        }

        public XYZ GetPoint() {
            var mepLine = _clash.Element1.GetLine();
            //удлинена осевая линия инженерной системы на 5 м в обе стороны
            var elongatedMepLine = Line.CreateBound(mepLine.GetEndPoint(0) - mepLine.Direction * 16.5,
                                      mepLine.GetEndPoint(1) + mepLine.Direction * 16.5);

            //получена линия, идущая вдоль стены и расположенная точно по центру (т.е. линия равноудалена от внутренней и наружной граней стены), и удлинена на 5 м в обе стороны
            var wallLine = _clash.Element2.GetСentralLine();
            var elongatedWallLine = Line.CreateBound(wallLine.GetEndPoint(0) - wallLine.Direction * 16.5,
                                     wallLine.GetEndPoint(1) + wallLine.Direction * 16.5);

            //трансформация линии стены в координаты основного файла
            var transformedWallLine = Line.CreateBound(_clash.Element2Transform.OfPoint(elongatedWallLine.GetEndPoint(0)), _clash.Element2Transform.OfPoint(elongatedWallLine.GetEndPoint(1)));
            try {
                var dir = _clash.Element2Transform.OfVector(_clash.Element2.Orientation);

                var point = elongatedMepLine.GetPointFromLineEquation(transformedWallLine) - dir * _clash.Element2.Width / 2;
                //получение точки вставки из уравнения линии 
                if(_rectangleHeightGetter != null) {
                    point -= _rectangleHeightGetter.GetValue().TValue / 2 * XYZ.BasisZ;
                }
                var zRoundCoordinate = RoundFeetToMillimeters(point.Z, _heightRound);
                return new XYZ(point.X, point.Y, zRoundCoordinate);
            } catch(NullReferenceException) {
                throw IntersectionNotFoundException.GetException(_clash.Element1, _clash.Element2);
            }
        }
    }
}
