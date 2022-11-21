
using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitClashDetective.Models.Extensions;

using RevitOpeningPlacement.Models.Extensions;
using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.OpeningPlacement.ValueGetters;

namespace RevitOpeningPlacement.Models.OpeningPlacement.PointFinders {
    internal class FittingWallPointFinder : IPointFinder {
        private readonly FittingClash<Wall> _clash;
        private readonly IAngleFinder _angleFinder;

        public FittingWallPointFinder(FittingClash<Wall> clash, IAngleFinder angleFinder) {
            _clash = clash;
            _angleFinder = angleFinder;
        }

        public XYZ GetPoint() {
            //получение z-координаты точки вставки
            var solid = _clash.GetIntersection();
            var z = solid.GetOutline().MinimumPoint.Z;

            //получение центра пересечения по BoundingBox-у
            var transformedSolid = _clash.GetRotatedIntersection(_angleFinder);
            var outline = transformedSolid.GetOutline();
            var transformedCenter = outline.MinimumPoint + (outline.MaximumPoint - outline.MinimumPoint) / 2;
            var center = Transform.Identity.GetRotationMatrixAroundZ(_angleFinder.GetAngle().Z).OfPoint(transformedCenter);

            //проекция центра на грань стены
            var sizeInit = new WallOpeningSizeInitializer(transformedSolid);
            var thickness = sizeInit.GetThickness();
            var point = center - _clash.Element2.Orientation * thickness.GetValue().TValue / 2;

            return new XYZ(point.X, point.Y, z);
        }
    }
}
