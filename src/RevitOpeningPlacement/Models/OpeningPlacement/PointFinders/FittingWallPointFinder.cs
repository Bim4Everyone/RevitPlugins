
using System;

using Autodesk.Revit.DB;

using RevitClashDetective.Models.Extensions;
using RevitClashDetective.Models.Value;

using RevitOpeningPlacement.Models.Extensions;
using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.OpeningPlacement.ValueGetters;

namespace RevitOpeningPlacement.Models.OpeningPlacement.PointFinders {
    /// <summary>
    /// Класс, предоставляющий точку вставки для пересечения экземпляра семейства элемента инженерной системы из активного файла и конструкции из связи
    /// </summary>
    internal class FittingWallPointFinder : RoundValueGetter, IPointFinder {
        private readonly FittingClash<Wall> _clash;
        private readonly IAngleFinder _angleFinder;
        private readonly IValueGetter<DoubleParamValue> _sizeGetter;
        /// <summary>
        /// Округление высотной отметки отверстия в мм
        /// </summary>
        private const int _heightRound = 10;

        public FittingWallPointFinder(FittingClash<Wall> clash, IAngleFinder angleFinder, IValueGetter<DoubleParamValue> sizeGetter = null) {
            _clash = clash;
            _angleFinder = angleFinder;
            _sizeGetter = sizeGetter;
        }

        public XYZ GetPoint() {
            //получение центра пересечения по BoundingBox-у
            var transformedSolid = _clash.GetRotatedIntersection(_angleFinder);
            var outline = transformedSolid.GetOutline();
            var transformedCenter = outline.MinimumPoint + (outline.MaximumPoint - outline.MinimumPoint) / 2;
            var clashCenter = Transform.Identity.GetRotationMatrixAroundZ(_angleFinder.GetAngle().Z).OfPoint(transformedCenter);

            //получение z-координаты точки вставки
            var solid = _clash.GetIntersection();
            var z = solid.GetOutline().MinimumPoint.Z;

            if(_sizeGetter != null) {
                z = clashCenter.Z - _sizeGetter.GetValue().TValue / 2;
            }
            z = RoundFeetToMillimeters(z, _heightRound);

            // Т.к. в местах пересечений FamilyInstances и Wall формируются прямоугольные задания на отверстия толщина которых равна толщине стены,
            // то нужно скорректировать полученный центр пересечения под точку вставки семейства.
            // Точка вставки прямоугольного задания на отверстие в стене находится на середине переднего ребра нижней грани экземпляра семейства.

            //проекция центра на грань стены
            XYZ pointProjectedOnWallCenter = ProjectPointOntoWallCenter(_clash, clashCenter);
            double wallThickness = _clash.Element2.Width;
            // если смотреть на стену на плане, когда начало стены слева, а конец справа,
            // то ориентация стены - вектор, перпендикулярный осевой поверхности стены и направленный от стены вверх экрана
            var point = pointProjectedOnWallCenter - _clash.Element2Transform.OfVector(_clash.Element2.Orientation) * wallThickness / 2;

            return new XYZ(point.X, point.Y, z);
        }


        /// <summary>
        /// Проецирует точку на осевую линию стены из пересечения
        /// </summary>
        /// <param name="fittingWallClash">Пересечение элемента инженерной системы из активного документа и стены из связи</param>
        /// <param name="point">Точка, которую надо спроецировать на осевую линию стены в координатах активного документа</param>
        /// <returns></returns>
        private XYZ ProjectPointOntoWallCenter(FittingClash<Wall> fittingWallClash, XYZ point) {
            // предполагаем, что стена прямая и строим линию по начальной и конечной точке
            Curve wallCurve = (fittingWallClash.Element2.Location as LocationCurve).Curve;
            XYZ startPoint = fittingWallClash.Element2Transform.OfPoint(wallCurve.GetEndPoint(0));
            XYZ endPoint = fittingWallClash.Element2Transform.OfPoint(wallCurve.GetEndPoint(1));
            XYZ startToThePointVector = (point - startPoint);
            XYZ startToEndVector = endPoint - startPoint;
            double angle = startToThePointVector.AngleTo(startToEndVector);
            double projectedDistance = startToThePointVector.GetLength() * Math.Cos(angle);
            return startPoint + startToEndVector.Normalize() * projectedDistance;
        }
    }
}
