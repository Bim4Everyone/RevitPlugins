using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit.Geometry;

using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.RealOpeningArPlacement.PointFinders;

namespace RevitOpeningPlacement.Models.RealOpeningKrPlacement.Providers;
/// <summary>
/// Класс, предоставляющий интерфейс для получения точки вставки отверстия КР, размещаемого по нескольким входящим заданиям
/// </summary>
internal class ManyOpeningArTasksPointFinderProvider {
    private readonly Element _host;
    private readonly ICollection<IOpeningTaskIncoming> _incomingTasks;
    private readonly int _rounding;


    /// <summary>
    /// Конструктор класса, предоставляющего интерфейс для получения точки вставки отверстия КР, размещаемого по нескольким входящим заданиям
    /// </summary>
    /// <param name="host">Основа для размещаемого отверстия КР</param>
    /// <param name="incomingTasks">Входящие задания на отверстия</param>
    /// <param name="rounding">Округление отметки отверстия в мм</param>
    /// <exception cref="ArgumentNullException">Исключение, если обязательный параметр null</exception>
    /// <exception cref="ArgumentException">Исключение, если тип хоста не поддерживается или количество элементов в коллизии меньше 1</exception>
    public ManyOpeningArTasksPointFinderProvider(Element host, ICollection<IOpeningTaskIncoming> incomingTasks, int rounding) {
        if(host is null) { throw new ArgumentNullException(nameof(host)); }
        if(host is not (Wall or Floor)) { throw new ArgumentException(nameof(host)); }
        if(incomingTasks is null) { throw new ArgumentNullException(nameof(incomingTasks)); }
        if(incomingTasks.Count < 1) { throw new ArgumentException(nameof(incomingTasks)); }

        _host = host;
        _incomingTasks = incomingTasks;
        _rounding = rounding;
    }


    public IPointFinder GetPointFinder() {
        var box = GetUnitedBBox(_incomingTasks);
        if(_host is Wall) {
            // упрощенное получение точки вставки по боксу
            return new BoundingBoxBottomPointFinder(box, _rounding);
        } else {
            return _host is Floor ? (IPointFinder) new BoundingBoxCenterPointFinder(box) : throw new ArgumentException(nameof(_host));
        }
    }


    private BoundingBoxXYZ GetUnitedBBox(ICollection<IOpeningTaskIncoming> incomingTasks) {
        return incomingTasks.Select(task => task.GetTransformedBBoxXYZ()).ToList().CreateUnitedBoundingBox();
    }
}
