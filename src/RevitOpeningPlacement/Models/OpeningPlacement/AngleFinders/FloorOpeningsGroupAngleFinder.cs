using System;
using System.Linq;

using Autodesk.Revit.DB;

using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.OpeningUnion;

namespace RevitOpeningPlacement.Models.OpeningPlacement.AngleFinders {
    /// <summary>
    /// Класс, предоставляющий угол поворота для экземпляра семейства задания на отверстие, которое является объединением нескольких заданий
    /// </summary>
    internal class FloorOpeningsGroupAngleFinder : IAngleFinder {
        private readonly OpeningsGroup _openingsGroup;

        /// <summary>
        /// Конструктор класса, предоставляющего угол поворота для экземпляра семейства задания на отверстие, которое является объединением нескольких заданий
        /// </summary>
        /// <param name="openingsGroup">Группа заданий на отверстия для объединения</param>
        /// <exception cref="ArgumentNullException"></exception>
        public FloorOpeningsGroupAngleFinder(OpeningsGroup openingsGroup) {
            _openingsGroup = openingsGroup ?? throw new ArgumentNullException(nameof(openingsGroup));
        }


        public Rotates GetAngle() {
            var transform = _openingsGroup.Elements.First().GetFamilyInstance().GetTotalTransform();
            var angle = XYZ.BasisY.AngleTo(transform.BasisY);
            return (transform.BasisY.X <= 0 && transform.BasisY.Y <= 0) || (transform.BasisY.X <= 0 && transform.BasisY.Y >= 0) ? new Rotates(0, 0, angle) : new Rotates(0, 0, -angle);
        }
    }
}
