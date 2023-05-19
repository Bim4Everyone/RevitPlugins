using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.Revit.Geometry;

namespace RevitSetLevelSection.Models {
    internal class IntersectImpl {
        public static readonly double DefaultHeight = 10;
        
        public Application Application { get; set; }
        public Transform LinkedTransform { get; set; }

        public bool IsIntersect(ZoneInfo zoneInfo, Element element) {
            var line = GetShortLineInZ(element);
            var result = zoneInfo.Solid?.IntersectWithCurve(line,
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
        /// Создает трансформированный общий <see cref="Solid"/> элемента.
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
        
        private Line GetShortLineInZ(Element element) {
            return GetShortLineInZ(GetMidPoint(element));
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
        
        /// <summary>
        /// Создает минимальной длины линию относительно точки .
        /// </summary>
        /// <param name="point">Точка вокруг которой создается линия.</param>
        /// <returns>Возвращает новую линию вокруг точки.</returns>
        private Line GetShortLineInZ(XYZ point) {
            var minPoint = new XYZ(0, 0, DefaultHeight);
            var newPoint = new XYZ(point.X, point.Y, 0);
            return Line.CreateBound(newPoint - minPoint, newPoint + minPoint);
        }
    }
}