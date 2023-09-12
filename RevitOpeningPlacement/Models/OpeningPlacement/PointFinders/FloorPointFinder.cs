using Autodesk.Revit.DB;

using RevitClashDetective.Models.Extensions;

using RevitOpeningPlacement.Models.Extensions;
using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.OpeningPlacement.PointFinders {
    /// <summary>
    /// Класс, предоставляющий точку вставки для задания на отверстие по пересечению элемента и плиты
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class FloorPointFinder<T> : IPointFinder where T : Element {
        private readonly Clash<T, CeilingAndFloor> _clash;

        public FloorPointFinder(Clash<T, CeilingAndFloor> clash) {
            _clash = clash;
        }

        public XYZ GetPoint() {
            var solid = _clash.GetIntersection();
            var maxZ = _clash.Element2.IsHorizontal() ? GetMaxZ(_clash.Element2.GetTopFace()) : solid.GetOutline().MaximumPoint.Z;
            var point = solid.ComputeCentroid();
            return new XYZ(point.X, point.Y, maxZ);
        }

        /// <summary>
        /// Возвращает максимальную координату Z для заданной поверхности.
        /// Алгоритм предполагает, что поверхность плоская и горизонтальная
        /// </summary>
        /// <param name="horizontalFace">Горизонтальная плоская поверхность</param>
        /// <returns></returns>
        public double GetMaxZ(Face horizontalFace) {
            return horizontalFace.EdgeLoops.get_Item(0).get_Item(0).AsCurve().GetEndPoint(0).Z;
        }
    }
}
