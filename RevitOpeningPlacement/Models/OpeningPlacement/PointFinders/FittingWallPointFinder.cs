
using Autodesk.Revit.DB;

using RevitClashDetective.Models.Extensions;
using RevitClashDetective.Models.Value;

using RevitOpeningPlacement.Models.Extensions;
using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.OpeningPlacement.ValueGetters;

namespace RevitOpeningPlacement.Models.OpeningPlacement.PointFinders {
    internal class FittingWallPointFinder : RoundValueGetter, IPointFinder {
        private readonly FittingClash<Wall> _clash;
        private readonly IAngleFinder _angleFinder;
        private readonly IValueGetter<DoubleParamValue> _sizeGetter;
        /// <summary>
        /// Округление высотной отметки отверстия в мм
        /// </summary>
        private const int _heightRound = 10;

        public FittingWallPointFinder(FittingClash<Wall> clash, IAngleFinder angleFinder, IValueGetter<DoubleParamValue> sizeGetter = null) {
            _clash = clash;
            _angleFinder = angleFinder;
            _sizeGetter = sizeGetter;
        }

        public XYZ GetPoint() {
            //получение центра пересечения по BoundingBox-у
            var transformedSolid = _clash.GetRotatedIntersection(_angleFinder);
            var outline = transformedSolid.GetOutline();
            var transformedCenter = outline.MinimumPoint + (outline.MaximumPoint - outline.MinimumPoint) / 2;
            var center = Transform.Identity.GetRotationMatrixAroundZ(_angleFinder.GetAngle().Z).OfPoint(transformedCenter);

            //получение z-координаты точки вставки
            var solid = _clash.GetIntersection();
            var z = solid.GetOutline().MinimumPoint.Z;

            if(_sizeGetter != null) {
                z = center.Z - _sizeGetter.GetValue().TValue / 2;
            }
            z = RoundFeetToMillimeters(z, _heightRound);

            //проекция центра на грань стены
            var sizeInit = new WallOpeningSizeInitializer(transformedSolid);
            var thickness = sizeInit.GetThickness();
            var point = center - _clash.Element2Transform.OfVector(_clash.Element2.Orientation) * thickness.GetValue().TValue / 2;

            return new XYZ(point.X, point.Y, z);
        }
    }
}
