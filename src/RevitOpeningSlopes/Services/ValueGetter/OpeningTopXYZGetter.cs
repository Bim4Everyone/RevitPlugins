using System;

using Autodesk.Revit.DB;

using RevitOpeningSlopes.Models;
using RevitOpeningSlopes.Models.Enums;

namespace RevitOpeningSlopes.Services.ValueGetter {
    internal class OpeningTopXYZGetter {
        private readonly RevitRepository _revitRepository;
        private readonly LinesFromOpening _linesFromOpening;
        private readonly NearestElements _nearestElements;
        private readonly SolidOperations _solidOperations;


        public OpeningTopXYZGetter(
            RevitRepository revitRepository,
            LinesFromOpening linesFromOpening,
            NearestElements nearestElements,
            SolidOperations solidOperations) {

            _revitRepository = revitRepository
                ?? throw new ArgumentNullException(nameof(revitRepository));
            _linesFromOpening = linesFromOpening
                ?? throw new ArgumentNullException(nameof(linesFromOpening));
            _nearestElements = nearestElements
                ?? throw new ArgumentNullException(nameof(nearestElements));
            _solidOperations = solidOperations
                ?? throw new ArgumentNullException(nameof(solidOperations));

        }
        public XYZ GetOpeningTopXYZ(FamilyInstance opening) {
            XYZ intersectCoord = null;
            XYZ origin = _revitRepository.GetOpeningLocation(opening);
            Line upwardLine = _linesFromOpening.CreateLineFromOpening(origin, opening, 4000, DirectionEnum.Top);
            Element topElement = _nearestElements.GetElementByRay(upwardLine);
            //_linesFromOpening.CreateTestModelLine(upwardLine);
            if(topElement == null) {
                return null;
            }
            Solid topSolid = _solidOperations.GetUnitedSolidFromHostElement(topElement);
            //DirectShape ds = DirectShape.CreateElement(_revitRepository.Document,
            //    new ElementId(BuiltInCategory.OST_GenericModel));
            //ds.SetShape(new GeometryObject[] { topSolid });
            if(topSolid != null) {

                SolidCurveIntersectionOptions intersectOptOutside = new SolidCurveIntersectionOptions() {
                    ResultType = SolidCurveIntersectionMode.CurveSegmentsOutside
                };
                SolidCurveIntersectionOptions intersectOptInside = new SolidCurveIntersectionOptions() {
                    ResultType = SolidCurveIntersectionMode.CurveSegmentsInside
                };
                SolidCurveIntersection intersection = topSolid.IntersectWithCurve(upwardLine, intersectOptInside);
                if(intersection.SegmentCount > 0) {
                    intersection = topSolid.IntersectWithCurve(upwardLine, intersectOptOutside);
                } else {
                    throw new ArgumentException("Над окном нет элемента, пересекающегося с окном");
                }
                intersectCoord = intersection.GetCurveSegment(0).GetEndPoint(1);
                //Line testL = Line.CreateBound(intersectCoord, new XYZ(0, 0, 0));
                //_linesFromOpening.CreateTestModelLine(intersection.GetCurveSegment(0) as Line);
                return intersectCoord;
            } else {
                return null;
            }

        }
    }
}
