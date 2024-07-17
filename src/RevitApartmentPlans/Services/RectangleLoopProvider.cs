using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

namespace RevitApartmentPlans.Services {
    internal class RectangleLoopProvider : IRectangleLoopProvider {
        /// <summary>
        /// Создает прямоугольный замкнутый наружный контур, в который вписаны все заданные замкнутые контуры.<br/>
        /// Линии в этом контуре ориентированы против часовой стрелки.
        /// </summary>
        public CurveLoop CreateRectCounterClockwise(ICollection<CurveLoop> curveLoops) {
            var points = curveLoops
                .SelectMany(loop => loop.Select(curve => curve.GetEndPoint(0)))
                .ToArray();
            Outline outline = new Outline(points[0], points[1]);
            for(var i = 2; i < points.Length; i++) {
                outline.AddPoint(points[i]);
            }
            double z = outline.MinimumPoint.Z;
            double minX = outline.MinimumPoint.X;
            double minY = outline.MinimumPoint.Y;
            double maxX = outline.MaximumPoint.X;
            double maxY = outline.MaximumPoint.Y;

            XYZ leftBottom = new XYZ(minX, minY, z);
            XYZ leftTop = new XYZ(minX, maxY, z);
            XYZ rightTop = new XYZ(maxX, maxY, z);
            XYZ rightBottom = new XYZ(maxX, minY, z);

            var left = Line.CreateBound(leftTop, leftBottom);
            var bottom = Line.CreateBound(leftBottom, rightBottom);
            var right = Line.CreateBound(rightBottom, rightTop);
            var top = Line.CreateBound(rightTop, leftTop);

            return CurveLoop.Create(new Curve[] { left, bottom, right, top });
        }

        public CurveLoop CreateRectCounterClockwise(CurveLoop curveLoop) {
            return CreateRectCounterClockwise(new CurveLoop[] { curveLoop });
        }
    }
}
