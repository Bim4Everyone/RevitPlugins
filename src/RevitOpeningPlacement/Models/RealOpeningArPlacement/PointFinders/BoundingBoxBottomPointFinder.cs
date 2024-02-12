using System;

using Autodesk.Revit.DB;

using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.OpeningPlacement.ValueGetters;

namespace RevitOpeningPlacement.Models.RealOpeningArPlacement.PointFinders {
    /// <summary>
    /// Класс, предоставляющий точку вставки чистового отверстия по заданному боксу. Использовать для получения точки вставки чистового отверстия по нескольким заданиям на отверстия в стене
    /// </summary>
    internal class BoundingBoxBottomPointFinder : RoundValueGetter, IPointFinder {
        private readonly BoundingBoxXYZ _bbox;


        /// <summary>
        /// Конструктор класса, предоставляющего точку вставки чистового отверстия по заданному боксу. Использовать для получения точки вставки чистового отверстия по нескольким заданиям на отверстия в стене
        /// </summary>
        /// <param name="bbox"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public BoundingBoxBottomPointFinder(BoundingBoxXYZ bbox) {
            _bbox = bbox ?? throw new ArgumentNullException(nameof(bbox));
        }


        public XYZ GetPoint() {
            var x = (_bbox.Max.X + _bbox.Min.X) / 2;
            var y = (_bbox.Max.Y + _bbox.Min.Y) / 2;
            var z = RoundToFloorFeetToMillimeters(_bbox.Min.Z, 10);
            return new XYZ(x, y, z);
        }
    }
}
