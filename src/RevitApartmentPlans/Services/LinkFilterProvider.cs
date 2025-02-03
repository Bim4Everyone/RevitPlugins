using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;

namespace RevitApartmentPlans.Services {
    internal class LinkFilterProvider {
        public FilteredElementCollector GetFilter(RevitLinkInstance link, ViewPlan activePlan) {
            if(link is null) {
                throw new ArgumentNullException(nameof(link));
            }
            if(activePlan is null) {
                throw new ArgumentNullException(nameof(activePlan));
            }

#if REVIT_2023_OR_LESS
            var solid = GetCropSolid(activePlan);
            var transform = link.GetTransform();
            var transformedSolid = SolidUtils.CreateTransformed(solid, transform.Inverse);
            return new FilteredElementCollector(link.GetLinkDocument())
                .WherePasses(new ElementIntersectsSolidFilter(transformedSolid));
#else
            return new FilteredElementCollector(activePlan.Document, activePlan.Id, link.Id);
#endif
        }

        /// <summary>
        /// Строит солид путем выдавливания формы области подрезки от секущей плоскости вверх на 1 фут
        /// </summary>
        /// <param name="viewPlan">План для создания солида</param>
        /// <returns>Солид области подрезки</returns>
        private Solid GetCropSolid(ViewPlan viewPlan) {
            var cutZ = GetCutPlaneZ(viewPlan);
            var loops = GetShape(viewPlan, cutZ);
            return GeometryCreationUtilities.CreateExtrusionGeometry(loops, XYZ.BasisZ, 1);
        }

        /// <summary>
        /// Возвращает форму области подрезки плана, если она включена, или прямоугольник по боксу
        /// </summary>
        private IList<CurveLoop> GetShape(ViewPlan viewPlan, double z) {
            var shape = viewPlan.GetCropRegionShapeManager().GetCropShape().Select(loop => CopyToZ(loop, z)).ToArray();
            if(shape is null) {
                var box = viewPlan.GetBoundingBox();

                var bottomLeft = new XYZ(box.Min.X, box.Min.Y, z);
                var topLeft = new XYZ(box.Min.X, box.Max.Y, z);
                var topRight = new XYZ(box.Max.X, box.Max.Y, z);
                var bottomRight = new XYZ(box.Max.X, box.Min.Y, z);

                shape = new CurveLoop[]{ CurveLoop.Create(new Curve[] {
                    Line.CreateBound(bottomLeft, topLeft),
                    Line.CreateBound(topLeft, topRight),
                    Line.CreateBound(topRight, bottomRight),
                    Line.CreateBound(bottomRight, bottomLeft)
                })};
            }
            return shape;
        }

        private CurveLoop CopyToZ(CurveLoop loop, double z) {
            double loopZ = loop.First().GetEndPoint(0).Z;
            var transform = Transform.CreateTranslation(new XYZ(0, 0, z - loopZ));
            return CurveLoop.CreateViaTransform(loop, transform);
        }

        private double GetCutPlaneZ(ViewPlan viewPlan) {
            var viewRange = viewPlan.GetViewRange();
            var doc = viewPlan.Document;
            // ссылки на другие уровни (сверху/внизу) нельзя получить, если там установлено уровень выше/неограниченный.
            // поэтому берем уровень секущей плоскости
            var cutLevel = doc.GetElement(viewRange.GetLevelId(PlanViewPlane.CutPlane)) as Level;

            return viewRange.GetOffset(PlanViewPlane.BottomClipPlane) + cutLevel.Elevation;
        }
    }
}
