using System;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit.Geometry;

using RevitOpeningSlopes.Models;
using RevitOpeningSlopes.Models.Enums;

namespace RevitOpeningSlopes.Services.ValueGetter {
    internal class OpeningRightXYZGetter {
        private readonly LinesFromOpening _linesFromOpening;
        private readonly OpeningHeightGetter _heightGetter;
        private readonly NearestElements _nearestElements;
        private readonly OpeningCenterXYZGetter _openingCenterXYZGetter;

        public OpeningRightXYZGetter(
            LinesFromOpening linesFromOpening,
            OpeningHeightGetter heightGetter,
            NearestElements nearestElements,
            OpeningCenterXYZGetter openingCenterXYZGetter) {

            _linesFromOpening = linesFromOpening ?? throw new ArgumentNullException(nameof(linesFromOpening));
            _heightGetter = heightGetter ?? throw new ArgumentNullException(nameof(heightGetter));
            _nearestElements = nearestElements ?? throw new ArgumentNullException(nameof(nearestElements));
            _openingCenterXYZGetter = openingCenterXYZGetter
                ?? throw new ArgumentNullException(nameof(openingCenterXYZGetter));
        }
        public XYZ GetOpeningRightXYZ(FamilyInstance opening) {
            XYZ intersectCoord = null;
            XYZ origin = _openingCenterXYZGetter.GetOpeningCenter(opening);
            Line rightLine = _linesFromOpening.CreateLineFromOpening(origin, opening, 2000, DirectionEnum.Right);
            Element rightElement = _nearestElements.GetElementByRay(rightLine);
            //_linesFromOpening.CreateTestModelLine(rightLine);
            if(rightElement == null) {
                return null;
            }
            //Solid rightSolid = _solidOperations.GetUnitedSolidFromHostElement(rightElement);
            Solid rightSolid = rightElement.GetSolids().FirstOrDefault();
            if(rightSolid != null) {
                SolidCurveIntersectionOptions intersectOptOutside = new SolidCurveIntersectionOptions() {
                    ResultType = SolidCurveIntersectionMode.CurveSegmentsOutside
                };
                SolidCurveIntersectionOptions intersectOptInside = new SolidCurveIntersectionOptions() {
                    ResultType = SolidCurveIntersectionMode.CurveSegmentsInside
                };
                SolidCurveIntersection intersection = rightSolid.IntersectWithCurve(rightLine, intersectOptInside);
                if(intersection.SegmentCount > 0) {
                    intersection = rightSolid.IntersectWithCurve(rightLine, intersectOptOutside);
                } else {
                    throw new ArgumentException("Справа от окна нет элемента, пересекающегося с ним");
                }
                intersectCoord = intersection.GetCurveSegment(0).GetEndPoint(1);
                return intersectCoord;
            } else {
                return null;
            }
        }
    }
}
