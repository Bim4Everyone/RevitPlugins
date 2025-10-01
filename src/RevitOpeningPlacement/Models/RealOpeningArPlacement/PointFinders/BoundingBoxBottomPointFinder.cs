using System;

using Autodesk.Revit.DB;

using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.OpeningPlacement.ValueGetters;

namespace RevitOpeningPlacement.Models.RealOpeningArPlacement.PointFinders;
/// <summary>
/// Класс, предоставляющий точку вставки чистового отверстия по заданному боксу. Использовать для получения точки вставки чистового отверстия по нескольким заданиям на отверстия в стене
/// </summary>
internal class BoundingBoxBottomPointFinder : RoundValueGetter, IPointFinder {
    private readonly BoundingBoxXYZ _bbox;
    private readonly int _roundingMm;


    /// <summary>
    /// Конструктор класса, предоставляющего точку вставки чистового отверстия по заданному боксу. Использовать для получения точки вставки чистового отверстия по нескольким заданиям на отверстия в стене
    /// </summary>
    /// <param name="bbox">Бокс</param>
    /// <param name="roundingMm">Округление высотной отметки в мм</param>
    /// <exception cref="ArgumentNullException">Исключение, если обязательный параметр null</exception>
    public BoundingBoxBottomPointFinder(BoundingBoxXYZ bbox, int roundingMm) {
        _bbox = bbox ?? throw new ArgumentNullException(nameof(bbox));
        _roundingMm = roundingMm;
    }


    public XYZ GetPoint() {
        double x = (_bbox.Max.X + _bbox.Min.X) / 2;
        double y = (_bbox.Max.Y + _bbox.Min.Y) / 2;
        double z = RoundToFloorFeetToMillimeters(_bbox.Min.Z, _roundingMm);
        return new XYZ(x, y, z);
    }
}
