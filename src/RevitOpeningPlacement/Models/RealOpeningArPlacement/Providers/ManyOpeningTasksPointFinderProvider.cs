using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit.Geometry;

using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.RealOpeningArPlacement.PointFinders;
using RevitOpeningPlacement.OpeningModels;

namespace RevitOpeningPlacement.Models.RealOpeningArPlacement.Providers;
/// <summary>
/// Класс, предоставляющий <see cref="IPointFinder"/> для размещения чистового отверстия по нескольким заданиям на отверстия
/// </summary>
internal class ManyOpeningTasksPointFinderProvider {
    private readonly Element _host;
    private readonly ICollection<OpeningMepTaskIncoming> _incomingTasks;
    private readonly int _rounding;


    /// <summary>
    /// Конструктор класса, предоставляющего <see cref="IPointFinder"/> для размещения чистового отверстия по нескольким заданиям на отверстия
    /// </summary>
    /// <param name="host">Хост для чистового отверстия</param>
    /// <param name="incomingTasks">Входящие задания на отверстия</param>
    /// <param name="rounding">Округление высотной отметки</param>
    /// <exception cref="ArgumentNullException">Исключение, если обязательный параметр null</exception>
    /// <exception cref="ArgumentException">Исключение, если в коллекции меньше 1 элемента</exception>
    public ManyOpeningTasksPointFinderProvider(Element host, ICollection<OpeningMepTaskIncoming> incomingTasks, int rounding) {
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
            return GetWallPointFinder(box);
        } else {
            return _host is Floor ? GetFloorPointFinder(box) : throw new ArgumentException("Тип хоста отверстия не поддерживается");
        }
    }

    private IPointFinder GetFloorPointFinder(BoundingBoxXYZ unitedBBox) {
        return new BoundingBoxCenterPointFinder(unitedBBox);
    }

    private IPointFinder GetWallPointFinder(BoundingBoxXYZ unitedBBox) {
        return new BoundingBoxBottomPointFinder(unitedBBox, _rounding);
    }

    private BoundingBoxXYZ GetUnitedBBox(ICollection<OpeningMepTaskIncoming> incomingTasks) {
        return incomingTasks.Select(task => task.GetTransformedBBoxXYZ()).ToList().CreateUnitedBoundingBox();
    }
}
