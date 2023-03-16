using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.Revit.Geometry;

namespace RevitSetLevelSection.Models {
    internal class IntersectImpl {
        /// <summary>
        /// Нужен для проецирования <see cref="Curve"/> на ось Z.
        /// </summary>
        private readonly Transform _transform = Transform.Identity;

        public IntersectImpl() {
            _transform.BasisZ = XYZ.Zero;
        }
        
        public Application Application { get; set; }
        public Transform LinkedTransform { get; set; }

        public bool IsIntersect(Area area, Element element) {
            var line = GetShortLine(element).CreateTransformed(_transform);
            var result = CreateSolid(area)?.IntersectWithCurve(line,
                new SolidCurveIntersectionOptions() {ResultType = SolidCurveIntersectionMode.CurveSegmentsInside});

            if(result?.ResultType == SolidCurveIntersectionMode.CurveSegmentsInside) {
                return result.Any(item => item.Length > 0);
            }

            return false;
        }

        public bool IsIntersect(FamilyInstance massElement, Element element) {
            var line = GetShortLine(element);
            var result = CreateSolid(massElement)?.IntersectWithCurve(line,
                new SolidCurveIntersectionOptions() {ResultType = SolidCurveIntersectionMode.CurveSegmentsInside});

            if(result?.ResultType == SolidCurveIntersectionMode.CurveSegmentsInside) {
                return result.Any(item => item.Length > 0);
            }

            return false;
        }

        /// <summary>
        /// Возвращает транспонированный <see cref="Solid"/> по границам зоны расположенный на Z=0.
        /// </summary>
        /// <param name="area">Зона.</param>
        /// <returns>Возвращает транспонированный <see cref="Solid"/> по границам зоны расположенный на Z=0.</returns>
        private Solid CreateSolid(Area area) {
            // Зоны являются замкнутыми и с простым одним контуром
            var boundarySegments = area.GetBoundarySegments(SpatialElementExtensions.DefaultBoundaryOptions)
                .First();

            var curves = boundarySegments
                .Select(item => item.GetCurve())
                .Select(item => item.CreateTransformed(_transform))
                .Select(item => item.CreateTransformed(LinkedTransform))
                .ToList();

            var curveLoops = new[] {CurveLoop.Create(curves)};
            return GeometryCreationUtilities.CreateExtrusionGeometry(curveLoops,
                XYZ.BasisZ,
                2 * Application.ShortCurveTolerance);
        }

        /// <summary>
        /// Создает транспонированный общий <see cref="Solid"/> элемента.
        /// </summary>
        /// <param name="element"></param>
        /// <returns>Возвращает самый большой <see cref="Solid"/> элемента после объединения.</returns>
        private Solid CreateSolid(Element element) {
            Solid solid = SolidExtensions.CreateUnitedSolids(element.GetSolids().ToList())
                .OrderBy(item => item.Volume)
                .Where(item => item.Volume > 0)
                .LastOrDefault();

            return solid == null ? null : SolidUtils.CreateTransformed(solid, LinkedTransform);
        }

        private Line GetShortLine(Element element) {
            return GetShortLine(GetMidPoint(element));
        }

        /// <summary>
        /// Возвращает центральную точку по <see cref="BoundingBoxXYZ"/>.
        /// </summary>
        /// <param name="element">Элемент.</param>
        /// <returns>Возвращает центральную точку, если у элемента есть <see cref="BoundingBoxXYZ"/>, иначе <see cref="Autodesk.Revit.DB.XYZ.Zero"/>.</returns>
        private XYZ GetMidPoint(Element element) {
            return element.GetBoundingBox()
                ?.GetMidPoint() ?? XYZ.Zero;
        }

        /// <summary>
        /// Создает минимальной длины линию относительно точки.
        /// </summary>
        /// <param name="point">Точка вокруг которой создается линия.</param>
        /// <returns>Возвращает новую линию вокруг точки.</returns>
        private Line GetShortLine(XYZ point) {
            var minPoint = new XYZ(Application.ShortCurveTolerance, 
                Application.ShortCurveTolerance, 
                Application.ShortCurveTolerance);
            return Line.CreateBound(point - minPoint, point + minPoint);
        }
    }
}