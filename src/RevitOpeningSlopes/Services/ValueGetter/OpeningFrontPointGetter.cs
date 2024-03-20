using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit.Geometry;

using RevitOpeningSlopes.Models;
using RevitOpeningSlopes.Models.Enums;

namespace RevitOpeningSlopes.Services.ValueGetter {
    internal class OpeningFrontPointGetter {
        private readonly OpeningRightXYZGetter _openingRightXYZGetter;
        private readonly SolidOperations _solidOperations;
        private readonly LinesFromOpening _linesFromOpening;
        private readonly NearestElements _nearestElements;

        public OpeningFrontPointGetter(
            OpeningRightXYZGetter openingRightXYZGetter,
            SolidOperations solidOperations,
            LinesFromOpening linesFromOpening,
            NearestElements nearestElements) {

            _openingRightXYZGetter = openingRightXYZGetter
                ?? throw new ArgumentNullException(nameof(openingRightXYZGetter));
            _solidOperations = solidOperations
                ?? throw new ArgumentNullException(nameof(solidOperations));
            _linesFromOpening = linesFromOpening
                ?? throw new ArgumentNullException(nameof(linesFromOpening));
            _nearestElements = nearestElements
                ?? throw new ArgumentNullException(nameof(nearestElements));
        }
        public XYZ GetFrontPoint(FamilyInstance opening) {
            XYZ intersectCoord = null;
            XYZ origin = _openingRightXYZGetter.GetOpeningRightXYZ(opening);
            Line rightLine = _linesFromOpening.CreateLineFromOpening(origin, opening, 500, DirectionEnum.Forward);
            Line leftLine = _linesFromOpening.CreateLineFromOpening(origin, opening, 500, DirectionEnum.Back);
            Line generalLine = _linesFromOpening.MergeLines(rightLine, leftLine);
            //Element window = _nearestElements.GetElementByRay(generalLine, true);
            _linesFromOpening.CreateTestModelLine(generalLine);

            //Solid rightSolid = _solidOperations.GetUnitedSolidFromHostElement(rightElement);

            IEnumerable<Solid> solids = opening.GetSolids();
            if(solids.Count() > 0) {
                SolidCurveIntersectionOptions intersectOptOutside = new SolidCurveIntersectionOptions() {
                    ResultType = SolidCurveIntersectionMode.CurveSegmentsOutside
                };
                SolidCurveIntersectionOptions intersectOptInside = new SolidCurveIntersectionOptions() {
                    ResultType = SolidCurveIntersectionMode.CurveSegmentsInside
                };
                foreach(Solid solid in solids) {
                    SolidCurveIntersection intersection = solid.IntersectWithCurve(generalLine, intersectOptInside);
                    if(intersection.SegmentCount > 0) {
                        intersectCoord = intersection.GetCurveSegment(0).GetEndPoint(1);
                        break;
                    } else {
                        continue;
                    }
                }
                if(intersectCoord == null) {
                    return null;
                } else {
                    return intersectCoord;
                }
                //if(intersection.SegmentCount > 0) {
                //    intersectCoord = intersection.GetCurveSegment(0).GetEndPoint(1);
                //} else {
                //    throw new ArgumentException("Окно не найдено в проеме");
                //}
            } else {
                return null;
            }

        }
    }
}
