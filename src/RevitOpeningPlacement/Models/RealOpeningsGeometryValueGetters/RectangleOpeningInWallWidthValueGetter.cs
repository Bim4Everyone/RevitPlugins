using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitClashDetective.Models.Value;

using RevitOpeningPlacement.Models.Extensions;
using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.RealOpeningsGeometryValueGetters;
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
    /// <param name="rounding">Округление размеров отверстия в мм</param>
    /// <exception cref="ArgumentNullException">Исключение, если обязательный параметр null</exception>
    public RectangleOpeningInWallWidthValueGetter(IOpeningTaskIncoming incomingTask, int rounding)
        : base(rounding, rounding, rounding) {
        if(incomingTask == null) { throw new ArgumentNullException(nameof(incomingTask)); }
        _incomingTasks = new IOpeningTaskIncoming[] { incomingTask };
    }

    /// <summary>
    /// Конструктор класса, предоставляющего значение ширины чистового прямоугольного отверстия АР/КР в стене в единицах Revit
    /// </summary>
    /// <param name="incomingTasks">Входящие задания на отверстия</param>
    /// <param name="wall">Стена-основа для чистового отверстия АР/КР</param>
    /// <param name="rounding">Округление размеров отверстия в мм</param>
    /// <exception cref="ArgumentNullException">Исключение, если обязательный параметр null</exception>
    /// <exception cref="ArgumentOutOfRangeException">Исключение, если количество элементов в коллекции меньше 1</exception>
    public RectangleOpeningInWallWidthValueGetter(ICollection<IOpeningTaskIncoming> incomingTasks, Wall wall, int rounding)
        : base(rounding, rounding, rounding) {
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
            width = (wall.Location as LocationCurve).Curve is Line line
                ? GetWidthByPointProjections(line, incomingTasks)
                : GetWidthByBoxAndOrientation(orientation, box);
        }
        return width;
    }

    /// <summary>
    /// Получает ширину чистового отверстия в футах из размеров бокса, ограничивающего задания на отверстия.
    /// Размеры бокса берутся в горизонтальной плоскости на основе вектора нормали к стене
    /// </summary>
    /// <param name="wallOrientation">Вектор нормали к стене</param>
    /// <param name="unitedBox">Объединенный бокс входящих заданий на отверстия</param>
    private double GetWidthByBoxAndOrientation(XYZ wallOrientation, BoundingBoxXYZ unitedBox) {
        // вектор нормали к поверхности стены
        // AngleTo возвращает значение от 0 до Pi
        double angleToX = XYZ.BasisX.AngleTo(wallOrientation);
        // Ширину брать по длине бокса по оси ОY,
        // если угол между ориентацией стены и осью ОХ документа в интервалах [0; pi/4] или [3p/4; pi],
        // иначе брать длину бокса по оси OX:
        double width = (angleToX is (>= 0 and <= (Math.PI / 4)) or (>= (3 * Math.PI / 4) and <= Math.PI))
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
    /// <exception cref="ArgumentException">Исключение, если в коллекции входящих заданий на отверстия меньше 1 элемента</exception>
    private double GetWidthByPointProjections(Line line, ICollection<IOpeningTaskIncoming> incomingTasks) {
        if(incomingTasks.Count < 1) { throw new ArgumentException(nameof(incomingTasks)); }

        List<XYZ> taskSidePoints = [];
        foreach(var task in incomingTasks) {
            double openingWidth = GetOpeningWidth(task);
            var center = task.Location;

            var forwardOffset = center + line.Direction * (openingWidth / 2);
            var forwardProjectedOffset = GetProjectionOfPointOntoLine(line, forwardOffset);
            taskSidePoints.Add(forwardProjectedOffset);

            var backwardOffset = center - line.Direction * (openingWidth / 2);
            var backwardProjectedOffset = GetProjectionOfPointOntoLine(line, backwardOffset);
            taskSidePoints.Add(backwardProjectedOffset);
        }
        var lineProjectedStart = line.GetEndPoint(0).ProjectOnXoY();
        var orderedPoints = taskSidePoints.OrderBy(point => point.DistanceTo(lineProjectedStart)).ToArray();
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
        var lineDirectionXoY = line.Direction.ProjectOnXoY().Normalize();
        var pointXoY = point.ProjectOnXoY();

        var lineProjectedStart = line.GetEndPoint(0).ProjectOnXoY();
        var startToPointVector = pointXoY - lineProjectedStart;
        double angle = lineDirectionXoY.AngleTo(startToPointVector);

        double distance = lineProjectedStart.DistanceTo(pointXoY);

        var projectedPoint = lineProjectedStart + Math.Cos(angle) * distance * lineDirectionXoY;
        return projectedPoint;
    }
}
