using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit.Geometry;

using RevitOpeningPlacement.Models.Extensions;
using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.OpeningPlacement;
using RevitOpeningPlacement.Models.OpeningPlacement.ParameterGetters;
using RevitOpeningPlacement.Models.RealOpeningArPlacement.ValueGetters;
using RevitOpeningPlacement.OpeningModels;

namespace RevitOpeningPlacement.Models.RealOpeningArPlacement.ParameterGetters {
    /// <summary>
    /// Класс для предоставляющий параметры для чистового прямоугольного отверстия в стене для нескольких заданий на отверстия
    /// </summary>
    internal class ManyOpeningTasksInWallParameterGetter : IParametersGetter {
        private readonly Wall _wall;
        private readonly ICollection<OpeningMepTaskIncoming> _incomingTasks;
        private readonly IPointFinder _pointFinder;


        /// <summary>
        /// Конструктор класса, предоставляющего параметры для чистового прямоугольного отверстия в стене для нескольких заданий на отверстия
        /// </summary>
        /// <param name="incomingTasks">Входящие задания на отверстия</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public ManyOpeningTasksInWallParameterGetter(Wall wall, ICollection<OpeningMepTaskIncoming> incomingTasks, IPointFinder pointFinder) {
            _wall = wall ?? throw new ArgumentNullException(nameof(wall));
            _incomingTasks = incomingTasks ?? throw new System.ArgumentNullException(nameof(incomingTasks));
            if(_incomingTasks.Count < 1) { throw new ArgumentException(nameof(incomingTasks)); }
            _pointFinder = pointFinder ?? throw new ArgumentNullException(nameof(pointFinder));
        }


        public IEnumerable<ParameterValuePair> GetParamValues() {
            var height = GetHeight(_incomingTasks);
            var width = GetWidth(_wall, _incomingTasks);

            // габариты отверстия
            yield return new DoubleParameterGetter(RealOpeningPlacer.RealOpeningArHeight, new DimensionValueGetter(height)).GetParamValue();
            yield return new DoubleParameterGetter(RealOpeningPlacer.RealOpeningArWidth, new DimensionValueGetter(width)).GetParamValue();

            // логические флаги для обозначений разделов отверстия
            var isEomValueGetter = new IsEomValueGetter(_incomingTasks);
            var isSsValueGetter = new IsSsValueGetter(_incomingTasks);
            var isOvValueGetter = new IsOvValueGetter(_incomingTasks);
            var isDuValueGetter = new IsDuValueGetter(_incomingTasks);
            var isVkValueGetter = new IsVkValueGetter(_incomingTasks);
            var isTsValueGetter = new IsTsValueGetter(_incomingTasks);
            yield return new IntegerParameterGetter(RealOpeningPlacer.RealOpeningIsEom, isEomValueGetter).GetParamValue();
            yield return new IntegerParameterGetter(RealOpeningPlacer.RealOpeningIsSs, isSsValueGetter).GetParamValue();
            yield return new IntegerParameterGetter(RealOpeningPlacer.RealOpeningIsOv, isOvValueGetter).GetParamValue();
            yield return new IntegerParameterGetter(RealOpeningPlacer.RealOpeningIsDu, isDuValueGetter).GetParamValue();
            yield return new IntegerParameterGetter(RealOpeningPlacer.RealOpeningIsVk, isVkValueGetter).GetParamValue();
            yield return new IntegerParameterGetter(RealOpeningPlacer.RealOpeningIsTs, isTsValueGetter).GetParamValue();
            yield return new IntegerParameterGetter(RealOpeningPlacer.RealOpeningIsManualBimModelPart, new IsManualBimModelPartValueGetter()).GetParamValue();

            // текстовые данные отверстия
            var manualBimModelPartValueGetter = new ManualBimModelPartValueGetter()
                .SetIsEom(isEomValueGetter)
                .SetIsSs(isSsValueGetter)
                .SetIsOv(isOvValueGetter)
                .SetIsDu(isDuValueGetter)
                .SetIsVk(isVkValueGetter)
                .SetIsTs(isTsValueGetter)
                ;
            yield return new StringParameterGetter(RealOpeningPlacer.RealOpeningTaskId, new TaskIdValueGetter(_incomingTasks)).GetParamValue();
            yield return new StringParameterGetter(RealOpeningPlacer.RealOpeningManualBimModelPart, manualBimModelPartValueGetter).GetParamValue();
        }

        /// <summary>
        /// Возвращает объединенный бокс по входящим заданиям на отверстия
        /// </summary>
        /// <param name="incomingTasks"></param>
        /// <returns></returns>
        private BoundingBoxXYZ GetUnitedBox(ICollection<OpeningMepTaskIncoming> incomingTasks) {
            return incomingTasks.Select(task => task.GetTransformedBBoxXYZ()).ToList().CreateUnitedBoundingBox();
        }

        /// <summary>
        /// Возвращает высоту чистового отверстия в футах
        /// </summary>
        /// <param name="incomingTasks">Коллекция входящих заданий на отверстия</param>
        /// <returns></returns>
        private double GetHeight(ICollection<OpeningMepTaskIncoming> incomingTasks) {
            var box = GetUnitedBox(incomingTasks);
            double zOffset = Math.Abs(box.Min.Z - _pointFinder.GetPoint().Z);
            var height = box.Max.Z - box.Min.Z + zOffset;
            return height;
        }

        /// <summary>
        /// Возвращает ширину чистового отверстия в стене в футах
        /// </summary>
        /// <param name="wall">Стена для размещения чистового отверстия</param>
        /// <param name="incomingTasks">Коллекция входящих заданий на отверстия</param>
        /// <returns></returns>
        private double GetWidth(Wall wall, ICollection<OpeningMepTaskIncoming> incomingTasks) {
            var orientation = wall.Orientation;
            var box = GetUnitedBox(incomingTasks);

            if((wall.Location as LocationCurve).Curve is Line line) {
                return GetWidthByPointProjections(line, incomingTasks);

            } else {
                return GetWidthByBoxAndOrientation(orientation, box);
            }
        }

        /// <summary>
        /// Возвращает ширину чистового отверстия в футах на основе максимального расстояния между вспомогательными точками, 
        /// определяющими габариты заданий на отверстия в горизонтальной плоскости.
        /// </summary>
        /// <param name="line">Ось стены</param>
        /// <param name="incomingTasks">Входящие задания на отверстия</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Исключение, если в коллекции входящих заданий на отверстия меньше 1 элемента</exception>
        private double GetWidthByPointProjections(Line line, ICollection<OpeningMepTaskIncoming> incomingTasks) {
            if(incomingTasks.Count < 1) { throw new ArgumentException(nameof(incomingTasks)); }

            List<XYZ> taskSidePoints = new List<XYZ>();
            foreach(var task in incomingTasks) {
                double openingWidth = GetOpeningWidth(task);
                XYZ center = task.GetSolid().ComputeCentroid();

                XYZ forwardOffset = center + line.Direction * (openingWidth / 2);
                XYZ forwardProjectedOffset = GetProjectionOfPointOntoLine(line, forwardOffset);
                taskSidePoints.Add(forwardProjectedOffset);

                XYZ backwardOffset = center - line.Direction * (openingWidth / 2);
                XYZ backwardProjectedOffset = GetProjectionOfPointOntoLine(line, backwardOffset);
                taskSidePoints.Add(backwardProjectedOffset);
            }
            XYZ lineProjectedStart = line.GetEndPoint(0).ProjectOnXoY();
            XYZ[] orderedPoints = taskSidePoints.OrderBy(point => point.DistanceTo(lineProjectedStart)).ToArray();
            var nearestToStartPoint = orderedPoints.First();
            var farthestToStartPoint = orderedPoints.Last();
            return nearestToStartPoint.DistanceTo(farthestToStartPoint);
        }

        /// <summary>
        /// Получает ширину чистового отверстия в футах из размеров бокса, ограничивающего задания на отверстия.
        /// Размеры бокса берутся в горизонтальной плоскости на основе вектора нормали к стене
        /// </summary>
        /// <param name="wallOrientation">Вектор нормали к стене</param>
        /// <param name="unitedBox">Объединенный бокс входящих заданий на отверстия</param>
        /// <returns></returns>
        private double GetWidthByBoxAndOrientation(XYZ wallOrientation, BoundingBoxXYZ unitedBox) {
            // вектор нормали к поверхности стены
            // AngleTo возвращает значение от 0 до Pi
            var angleToX = XYZ.BasisX.AngleTo(wallOrientation);
            // Ширину брать по длине бокса по оси ОY,
            // если угол между ориентацией стены и осью ОХ документа в интервалах [0; pi/4] или [3p/4; pi],
            // иначе брать длину бокса по оси OX:
            var width = (((0 <= angleToX) && (angleToX <= (Math.PI / 4)))
                || (((3 * Math.PI / 4) <= angleToX) && (angleToX <= Math.PI)))
                ? (unitedBox.Max.Y - unitedBox.Min.Y)
                : (unitedBox.Max.X - unitedBox.Min.X);
            return width;
        }

        /// <summary>
        /// Получает проекцию точки на линию в плоскости XOY и возвращает это новую точку
        /// </summary>
        /// <param name="line">Линия, на проекцию которой в плоскости XOY нужно спроецировать точку</param>
        /// <param name="point">Точка для проецирования</param>
        /// <returns>Точка в плоскости XOY, спроецированная на проекцию заданной линии по XOY</returns>
        private XYZ GetProjectionOfPointOntoLine(Line line, XYZ point) {
            XYZ lineDirectionXoY = line.Direction.ProjectOnXoY().Normalize();
            XYZ pointXoY = point.ProjectOnXoY();

            XYZ lineProjectedStart = line.GetEndPoint(0).ProjectOnXoY();
            XYZ startToPointVector = pointXoY - lineProjectedStart;
            double angle = lineDirectionXoY.AngleTo(startToPointVector);

            double distance = lineProjectedStart.DistanceTo(pointXoY);

            XYZ projectedPoint = lineProjectedStart + Math.Cos(angle) * distance * lineDirectionXoY;
            return projectedPoint;
        }

        /// <summary>
        /// Возвращает ширину задания на отверстие
        /// </summary>
        /// <param name="opening"></param>
        /// <returns></returns>
        private double GetOpeningWidth(OpeningMepTaskIncoming opening) {
            return (opening.Width > 0) ? opening.Width : opening.Diameter;
        }
    }
}
