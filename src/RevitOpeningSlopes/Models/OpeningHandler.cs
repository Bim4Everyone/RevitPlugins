using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit.Geometry;

using RevitOpeningSlopes.Models.Enums;

namespace RevitOpeningSlopes.Models {
    internal class OpeningHandler {
        private readonly RevitRepository _revitRepository;
        private readonly FamilyInstance _opening;
        private readonly LinesFromOpening _linesFromOpening;
        private readonly NearestElements _nearestElements;
        private readonly SolidOperations _solidOperations;

        public OpeningHandler(RevitRepository revitRepository, FamilyInstance opening) {
            _revitRepository = revitRepository
                ?? throw new ArgumentNullException(nameof(revitRepository));
            _linesFromOpening = new LinesFromOpening(revitRepository);
            _nearestElements = new NearestElements(revitRepository);
            _opening = opening
                ?? throw new ArgumentNullException(nameof(opening));
            _solidOperations = new SolidOperations(revitRepository);
        }
        private XYZ RightPoint { get => GetRightPoint(_opening); }
        private XYZ RightFrontPoint { get => GetRightFrontPoint(_opening); }
        private XYZ RightDepthPoint { get => GetRightDepthPoint(_opening); }
        private Line ForwardOffsetLine { get => _linesFromOpening.CreateLineFromOffsetPoint(_opening); }
        private XYZ HorizontalCenterPoint { get => GetHorizontalCenterPoint(); }
        private XYZ VerticalCenterPoint { get => GetVerticalCenterPoint(_opening); }
        private XYZ OpeningOrigin { get => _revitRepository.GetOpeningLocation(_opening); }
        public double OpeningHeight { get => GetOpeningHeight(); }
        public double OpeningWidth { get => GetOpeningWidth(); }
        public double OpeningDepth { get => GetOpeningDepth(); }
        public XYZ OpeningCenterPoint { get => VerticalCenterPoint; }


        private XYZ GetRightPoint(FamilyInstance opening) {
            Line lineFromOffsetPoint = ForwardOffsetLine;
            const double step = 0.032; //~10 мм
            const double rightLineLength = 2000;
            ICollection<XYZ> points = _linesFromOpening.SplitCurveToPoints(lineFromOffsetPoint, step);
            double closestDist = double.PositiveInfinity;
            XYZ closestPoint = null;
            foreach(XYZ point in points) {
                Line rightLine = _linesFromOpening.CreateLineFromOpening(point, opening,
                    rightLineLength,
                    DirectionEnum.Right);
                Element wall = _nearestElements.GetElementByRay(rightLine);
                if(wall != null) {
                    XYZ intersectCoord = null;
                    Solid wallSolid = _solidOperations.GetUnitedSolidFromHostElement(wall);
                    if(wallSolid != null) {
                        SolidCurveIntersectionOptions intersectOptOutside = new SolidCurveIntersectionOptions() {
                            ResultType = SolidCurveIntersectionMode.CurveSegmentsOutside
                        };
                        SolidCurveIntersection intersection = wallSolid.IntersectWithCurve(rightLine,
                            intersectOptOutside);
                        if(intersection.SegmentCount > 0) {
                            intersectCoord = intersection.GetCurveSegment(0).GetEndPoint(1);
                            double currentDist = intersectCoord.DistanceTo(point);
                            if(currentDist < closestDist) {
                                closestDist = currentDist;
                                closestPoint = intersectCoord;
                            }
                        } else {
                            continue;
                        }
                    }
                }
            }
            return closestPoint;
        }
        private XYZ GetRightFrontPoint(FamilyInstance opening) {
            XYZ rightPoint = RightPoint;
            XYZ openingVector = _revitRepository.GetOpeningVector(opening);
            const double backLineLength = 800;
            const double pointForwardOffset = 400;
            XYZ rightPointWithForwardOffset = rightPoint + openingVector
                * _revitRepository.ConvertToFeet(pointForwardOffset);
            Line backLine = _linesFromOpening.CreateLineFromOpening(
                rightPointWithForwardOffset,
                opening, backLineLength,
                DirectionEnum.Back);
            Element wall = _nearestElements.GetElementByRay(backLine);
            XYZ intersectCoord = null;
            if(wall != null) {
                Solid wallSolid = _solidOperations.GetUnitedSolidFromHostElement(wall);
                if(wallSolid != null) {
                    SolidCurveIntersectionOptions intersectOptOutside = new SolidCurveIntersectionOptions() {
                        ResultType = SolidCurveIntersectionMode.CurveSegmentsOutside
                    };
                    SolidCurveIntersection intersection = wallSolid.IntersectWithCurve(backLine, intersectOptOutside);
                    if(intersection.SegmentCount > 0) {
                        intersectCoord = intersection.GetCurveSegment(0).GetEndPoint(1);
                    }
                }
            }
            return intersectCoord;
        }
        private XYZ GetRightDepthPoint(FamilyInstance opening) {
            XYZ rightFrontPoint = RightFrontPoint;
            const double depthLineLength = 1000;
            Line depthLine = _linesFromOpening.CreateLineFromOpening(
                rightFrontPoint,
                opening, depthLineLength,
                DirectionEnum.Back);
            IEnumerable<Solid> openingSolids = opening.GetSolids();
            XYZ intersectCoord = null;
            XYZ closestPoint = null;
            if(openingSolids.Count() > 0) {
                SolidCurveIntersectionOptions intersectOptOutside = new SolidCurveIntersectionOptions() {
                    ResultType = SolidCurveIntersectionMode.CurveSegmentsOutside
                };
                double closestDist = double.PositiveInfinity;
                foreach(Solid solid in openingSolids) {
                    SolidCurveIntersection intersection = solid.IntersectWithCurve(depthLine, intersectOptOutside);
                    if(intersection.SegmentCount > 0) {
                        intersectCoord = intersection.GetCurveSegment(0).GetEndPoint(1);
                        double currentDist = _revitRepository
                            .ConvertToMillimeters(rightFrontPoint.DistanceTo(intersectCoord));
                        if(currentDist < closestDist) {
                            closestDist = currentDist;
                            closestPoint = intersectCoord;
                        }
                    }
                }
            }
            return closestPoint;
        }
        private XYZ GetHorizontalCenterPoint() {
            Line forwardOffsetLine = ForwardOffsetLine;
            XYZ origin = forwardOffsetLine.GetEndPoint(0);
            XYZ depthPoint = RightDepthPoint;
            XYZ directionForwardLine = forwardOffsetLine.Direction;
            double t;

            if(Math.Abs(directionForwardLine.X) > double.Epsilon)
                t = (depthPoint.X - origin.X) / directionForwardLine.X;
            else
                t = (depthPoint.Y - origin.Y) / directionForwardLine.Y; // Используем y-компоненту

            // Вычисление координат точки пересечения
            double intersectionX = origin.X + t * directionForwardLine.X;
            double intersectionY = origin.Y + t * directionForwardLine.Y;
            double intersectionZ = origin.Z + t * directionForwardLine.Z;

            XYZ intersectCoord = new XYZ(intersectionX, intersectionY, intersectionZ);

            return intersectCoord;
        }
        private XYZ GetVerticalCenterPoint(FamilyInstance opening) {
            XYZ intersectCoord = null;
            XYZ verticalCenter = null;
            XYZ origin = HorizontalCenterPoint;
            XYZ openingOrigin = OpeningOrigin;
            Line upwardLine = _linesFromOpening.CreateLineFromOpening(origin, opening, 4000, DirectionEnum.Top);
            Element topElement = _nearestElements.GetElementByRay(upwardLine);
            if(topElement == null) {
                return null;
            }
            Solid topSolid = _solidOperations.GetUnitedSolidFromHostElement(topElement);

            if(topSolid != null) {

                SolidCurveIntersectionOptions intersectOptOutside = new SolidCurveIntersectionOptions() {
                    ResultType = SolidCurveIntersectionMode.CurveSegmentsOutside
                };

                SolidCurveIntersection intersection = topSolid.IntersectWithCurve(upwardLine, intersectOptOutside);
                if(intersection.SegmentCount > 0) {
                    intersectCoord = intersection.GetCurveSegment(0).GetEndPoint(1);
                    double centerZ = (intersectCoord.Z + openingOrigin.Z) / 2;
                    verticalCenter = new XYZ(intersectCoord.X, intersectCoord.Y, centerZ);
                } else {
                    throw new ArgumentException("Над окном нет элемента, пересекающегося с окном");
                }
            }
            return verticalCenter;
        }
        private double GetOpeningHeight() {
            XYZ verticalCenterPoint = VerticalCenterPoint;
            XYZ openingOrigin = OpeningOrigin;
            double openingHeight = (verticalCenterPoint.Z - openingOrigin.Z) * 2;
            return openingHeight;
        }
        private double GetOpeningWidth() {
            XYZ verticalCenterPoint = VerticalCenterPoint;
            XYZ rightPoint = RightDepthPoint;
            double openingWidth = Math.Sqrt(Math.Pow(verticalCenterPoint.X - rightPoint.X, 2)
                + Math.Pow(verticalCenterPoint.Y - rightPoint.Y, 2)) * 2;
            return openingWidth;
        }
        private double GetOpeningDepth() {
            XYZ rightDepthPoint = RightDepthPoint;
            XYZ rightFrontPoint = RightFrontPoint;
            return rightDepthPoint.DistanceTo(rightFrontPoint);
        }
    }
}
