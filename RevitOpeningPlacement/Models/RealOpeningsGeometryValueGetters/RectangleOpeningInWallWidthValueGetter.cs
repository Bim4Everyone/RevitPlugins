using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitClashDetective.Models.Value;

using RevitOpeningPlacement.Models.Extensions;
using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.RealOpeningsGeometryValueGetters {
    /// <summary>
    /// Класс, предоставляющий значение ширины чистового прямоугольного отверстия АР/КР в стене в единицах Revit
    /// </summary>
    internal class RectangleOpeningInWallWidthValueGetter : RealOpeningSizeValueGetter, IValueGetter<DoubleParamValue> {
        private readonly ICollection<IOpeningTaskIncoming> _incomingTasks;
        private readonly Wall _wall;


        /// <summary>
        /// Конструктор класса, предоставляющего значение ширины чистового прямоугольного отверстия АР/КР в стене в единицах Revit
        /// </summary>
        /// <param name="incomingTask">Входящее задание на отверстие</param>
        /// <exception cref="ArgumentNullException"></exception>
        public RectangleOpeningInWallWidthValueGetter(IOpeningTaskIncoming incomingTask) {
            if(incomingTask == null) { throw new ArgumentNullException(nameof(incomingTask)); }
            _incomingTasks = new IOpeningTaskIncoming[] { incomingTask };
        }

        /// <summary>
        /// Конструктор класса, предоставляющего значение ширины чистового прямоугольного отверстия АР/КР в стене в единицах Revit
        /// </summary>
        /// <param name="incomingTasks">Входящие задания на отверстия</param>
        /// <param name="wall">Стена-основа для чистового отверстия АР/КР</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public RectangleOpeningInWallWidthValueGetter(ICollection<IOpeningTaskIncoming> incomingTasks, Wall wall) {
            if(incomingTasks is null) { throw new ArgumentNullException(nameof(incomingTasks)); }
            if(incomingTasks.Count < 1) { throw new ArgumentOutOfRangeException(nameof(incomingTasks)); }
            _wall = wall ?? throw new ArgumentNullException(nameof(wall));
            _incomingTasks = incomingTasks;
        }


        public DoubleParamValue GetValue() {
            double width = GetWidth(_incomingTasks, _wall);
            double roundWidthFeet = RoundToCeilingFeetToMillimeters(width, _widthRound);
            return new DoubleParamValue(roundWidthFeet);
        }


        private double GetWidth(ICollection<IOpeningTaskIncoming> incomingTasks, Wall wall) {
            double width;
            if(incomingTasks.Count == 1) {
                width = GetOpeningWidth(incomingTasks.First());
            } else {
                var orientation = wall.Orientation;
                var box = GetUnitedBox(incomingTasks);
                if((wall.Location as LocationCurve).Curve is Line line) {
                    width = GetWidthByPointProjections(line, incomingTasks);
                } else {
                    width = GetWidthByBoxAndOrientation(orientation, box);
                }
            }
            return width;
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
        /// Возвращает ширину чистового отверстия в футах на основе максимального расстояния между вспомогательными точками, 
        /// определяющими габариты заданий на отверстия в горизонтальной плоскости.
        /// </summary>
        /// <param name="line">Ось стены</param>
        /// <param name="incomingTasks">Входящие задания на отверстия</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Исключение, если в коллекции входящих заданий на отверстия меньше 1 элемента</exception>
        private double GetWidthByPointProjections(Line line, ICollection<IOpeningTaskIncoming> incomingTasks) {
            if(incomingTasks.Count < 1) { throw new ArgumentException(nameof(incomingTasks)); }

            List<XYZ> taskSidePoints = new List<XYZ>();
            foreach(var task in incomingTasks) {
                double openingWidth = GetOpeningWidth(task);
                XYZ center = task.Location;

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
    }
}
