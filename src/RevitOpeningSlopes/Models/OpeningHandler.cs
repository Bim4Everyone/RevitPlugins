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
        private Line _forwardOffsetLine;
        private XYZ _openingOrigin;
        private XYZ _rightPoint;
        private XYZ _rightFrontPoint;
        private XYZ _rightDepthPoint;
        private XYZ _horizontalCenterPoint;
        private XYZ _verticalCenterPoint;
        private double _openingHeight;
        private double _openingWidth;
        private double _openingDepth;

        public OpeningHandler(RevitRepository revitRepository, FamilyInstance opening) {
            _revitRepository = revitRepository
                ?? throw new ArgumentNullException(nameof(revitRepository));
            _linesFromOpening = new LinesFromOpening(revitRepository);
            _nearestElements = new NearestElements(revitRepository);
            _opening = opening
                ?? throw new ArgumentNullException(nameof(opening));
            _solidOperations = new SolidOperations(revitRepository);
            FillingParameters();
        }

        public double OpeningHeight { get => _openingHeight; }
        public double OpeningWidth { get => _openingWidth; }
        public double OpeningDepth { get => _openingDepth; }
        public XYZ OpeningCenterPoint { get => _verticalCenterPoint; }
        private void FillingParameters() {
            _openingOrigin = _revitRepository.GetOpeningLocation(_opening);
            _forwardOffsetLine = _linesFromOpening.CreateLineFromOffsetPoint(_opening);
            _rightPoint = GetRightPoint(_forwardOffsetLine);
            _rightFrontPoint = GetRightFrontPoint(_rightPoint);
            _rightDepthPoint = GetRightDepthPoint(_rightFrontPoint);
            _horizontalCenterPoint = GetHorizontalCenterPoint(_forwardOffsetLine, _rightDepthPoint);
            _verticalCenterPoint = GetVerticalCenterPoint(_openingOrigin, _horizontalCenterPoint);
            _openingHeight = GetOpeningHeight(_openingOrigin, _verticalCenterPoint);
            _openingWidth = GetOpeningWidth(_verticalCenterPoint, _rightDepthPoint);
            _openingDepth = GetOpeningDepth(_rightDepthPoint, _rightFrontPoint);
        }
        private XYZ GetRightPoint(Line forwardLine) {
            const double step = 0.032; //~10 мм
            const double rightLineLength = 2000;
            ICollection<XYZ> points = _linesFromOpening.SplitCurveToPoints(forwardLine, step);
            double closestDist = double.PositiveInfinity;
            XYZ closestPoint = null;
            foreach(XYZ point in points) {
                Line rightLine = _linesFromOpening.CreateLineFromOpening(point, _opening,
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
        private XYZ GetRightFrontPoint(XYZ rightPoint) {
            XYZ openingVector = _revitRepository.GetOpeningVector(_opening);
            const double backLineLength = 800;
            const double pointForwardOffset = 400;
            XYZ rightPointWithForwardOffset = rightPoint + openingVector
                * _revitRepository.ConvertToFeet(pointForwardOffset);
            Line backLine = _linesFromOpening.CreateLineFromOpening(
                rightPointWithForwardOffset,
                _opening, backLineLength,
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

        private XYZ GetRightDepthPoint(XYZ rightFrontPoint) {
            const double depthLineLength = 1000;
            Line depthLine = _linesFromOpening.CreateLineFromOpening(
                rightFrontPoint,
                _opening, depthLineLength,
                DirectionEnum.Back);
            IEnumerable<Solid> openingSolids = _opening.GetSolids();
            XYZ closestPoint = null;
            if(openingSolids.Count() > 0) {
                SolidCurveIntersectionOptions intersectOptOutside = new SolidCurveIntersectionOptions() {
                    ResultType = SolidCurveIntersectionMode.CurveSegmentsOutside
                };
                double closestDist = double.PositiveInfinity;
                foreach(Solid solid in openingSolids) {
                    SolidCurveIntersection intersection = solid.IntersectWithCurve(depthLine, intersectOptOutside);
                    if(intersection.SegmentCount > 0) {
                        XYZ intersectCoord = intersection.GetCurveSegment(0).GetEndPoint(1);
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

        private XYZ GetHorizontalCenterPoint(Line forwardOffsetLine, XYZ rightDepthPoint) {
            XYZ origin = forwardOffsetLine.GetEndPoint(0);
            XYZ directionForwardLine = forwardOffsetLine.Direction;
            double t;

            if(Math.Abs(directionForwardLine.X) > double.Epsilon)
                t = (rightDepthPoint.X - origin.X) / directionForwardLine.X;
            else
                t = (rightDepthPoint.Y - origin.Y) / directionForwardLine.Y; // Используем y-компоненту

            // Вычисление координат точки пересечения
            double intersectionX = origin.X + t * directionForwardLine.X;
            double intersectionY = origin.Y + t * directionForwardLine.Y;
            double intersectionZ = origin.Z + t * directionForwardLine.Z;

            XYZ intersectCoord = new XYZ(intersectionX, intersectionY, intersectionZ);

            return intersectCoord;
        }

        private XYZ GetVerticalCenterPoint(XYZ openingOrigin, XYZ horizontalCenterPoint) {
            XYZ intersectCoord = null;
            XYZ verticalCenter = null;
            Line upwardLine = _linesFromOpening.CreateLineFromOpening(horizontalCenterPoint,
                _opening, 4000, DirectionEnum.Top);
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

        private double GetOpeningHeight(XYZ openingOrigin, XYZ verticalCenterPoint) {
            double openingHeight = (verticalCenterPoint.Z - openingOrigin.Z) * 2;
            return openingHeight;
        }

        private double GetOpeningWidth(XYZ verticalCenterPoint, XYZ rightDepthPoint) {
            double openingWidth = Math.Sqrt(Math.Pow(verticalCenterPoint.X - rightDepthPoint.X, 2)
                + Math.Pow(verticalCenterPoint.Y - rightDepthPoint.Y, 2)) * 2;
            return openingWidth;
        }

        private double GetOpeningDepth(XYZ rightDepthPoint, XYZ rightFrontPoint) {
            return rightDepthPoint.DistanceTo(rightFrontPoint);
        }
    }
}
