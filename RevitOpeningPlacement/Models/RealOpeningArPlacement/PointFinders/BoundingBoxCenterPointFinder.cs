using System;

using Autodesk.Revit.DB;

using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.RealOpeningArPlacement.PointFinders {
    /// <summary>
    /// Класс, предоставляющий точку вставки чистового отверстия в центре заданного бокса. Использовать для получения точки вставки чистового отверстия по нескольким заданиям на отверстия в перекрытии
    /// </summary>
    internal class BoundingBoxCenterPointFinder : IPointFinder {
        private readonly BoundingBoxXYZ _bbox;


        /// <summary>
        /// Конструктор класса, предоставляющего точку вставки чистового отверстия в центре заданного бокса. Использовать для получения точки вставки чистового отверстия по нескольким заданиям на отверстия в перекрытии
        /// </summary>
        /// <param name="bbox"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public BoundingBoxCenterPointFinder(BoundingBoxXYZ bbox) {
            if(bbox is null) { throw new ArgumentNullException(nameof(bbox)); }

            _bbox = bbox;
        }


        public XYZ GetPoint() {
            var x = (_bbox.Max.X + _bbox.Min.X) / 2;
            var y = (_bbox.Max.Y + _bbox.Min.Y) / 2;
            var z = (_bbox.Max.Z + _bbox.Min.Z) / 2;
            return new XYZ(x, y, z);
        }
    }
}
