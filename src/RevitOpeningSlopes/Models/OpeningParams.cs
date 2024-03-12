using System;

using Autodesk.Revit.DB;

using RevitOpeningSlopes.Models.Enums;

namespace RevitOpeningSlopes.Models {
    internal class OpeningParams {
        private readonly RevitRepository _revitRepository;
        private readonly LinesFromOpening _linesFromOpening;
        private readonly SolidOperations _solidOperations;
        private readonly NearestElements _nearestElements;
        public OpeningParams(RevitRepository revitRepository) {
            _revitRepository = revitRepository;
            _linesFromOpening = new LinesFromOpening(revitRepository);
            _solidOperations = new SolidOperations(revitRepository);
            _nearestElements = new NearestElements(revitRepository);
        }
        public XYZ GetOpeningCenter(FamilyInstance opening) {
            XYZ origin = _revitRepository.GetOpeningLocation(opening);
            double height = GetOpeningHeight(opening);
            return new XYZ(origin.X, origin.Y, origin.Z + height / 2);
        }
        public double GetOpeningHeight(FamilyInstance opening) {
            XYZ intersectCoord = null;
            XYZ origin = _revitRepository.GetOpeningLocation(opening);
            Line upwardLine = _linesFromOpening.CreateLineFromOpening(opening, DirectionEnum.Top);
            Element topElement = _nearestElements.GetElementByRay(upwardLine);
            _linesFromOpening.CreateTestModelLine(upwardLine);
            if(topElement == null) {
                return 100;
            }
            Solid topSolid = _solidOperations.GetUnitedSolidFromHostElement(topElement);
            if(topSolid != null) {

                SolidCurveIntersectionOptions intersectOptOutside = new SolidCurveIntersectionOptions() {
                    ResultType = SolidCurveIntersectionMode.CurveSegmentsOutside
                };
                SolidCurveIntersectionOptions intersectOptInside = new SolidCurveIntersectionOptions() {
                    ResultType = SolidCurveIntersectionMode.CurveSegmentsInside
                };
                SolidCurveIntersection intersection = topSolid.IntersectWithCurve(upwardLine, intersectOptInside);
                if(intersection != null) {
                    intersection = topSolid.IntersectWithCurve(upwardLine, intersectOptOutside);
                } else {
                    throw new ArgumentException("Над окном нет элемента, пересекающегося с окном");
                }
                intersectCoord = intersection.GetCurveSegment(0).GetEndPoint(1);
                return origin.DistanceTo(intersectCoord);
            } else {
                return 100;
            }

        }
        //public double GetOpeningHeight(FamilyInstance opening) {
        //    //BoundingBoxXYZ bbox = opening.GetBoundingBox();
        //    //XYZ origin = _revitRepository.GetOpeningLocation(opening);
        //    //XYZ topPoint = new XYZ(origin.X, origin.Y, bbox.Max.Z);
        //    //double g = _revitRepository.ConvertToMillimeters(origin.DistanceTo(topPoint));
        //    //return origin.DistanceTo(topPoint);

        //}

    }
}
