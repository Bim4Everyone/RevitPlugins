using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitOpeningSlopes.Models.Enums;

namespace RevitOpeningSlopes.Models {
    internal class OpeningHandler {
        private readonly RevitRepository _revitRepository;
        private readonly FamilyInstance _opening;
        private readonly LinesFromOpening _linesFromOpening;
        private readonly NearestElements _nearestElements;
        private readonly SolidOperations _solidOperations;

        private XYZ _frontOffsetPoint;
        private XYZ _openingDepthPoint;
        private XYZ _centralDepthPoint;
        private XYZ _openingVector;
        private XYZ _openingBboxOrigin;
        private XYZ _rightPoint;
        private XYZ _rightFrontPoint;
        private XYZ _depthPoint;
        private XYZ _rightDepthPoint;
        private XYZ _horizontalCenterPoint;
        private XYZ _horizontalDepthPoint;
        private XYZ _topPoint;
        private XYZ _bottomPoint;
        private XYZ _verticalCenterPoint;

        private double _openingHeight;
        private double _openingWidth;
        private double _openingDepth;
        private double _rotationAngle;

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
        public double RotationAngle { get => _rotationAngle; }
        private void FillingParameters() {

            _openingBboxOrigin = _revitRepository.GetOpeningOriginBoundingBox(_opening);
            _openingVector = _revitRepository.GetOpeningVector(_opening);
            _frontOffsetPoint = GetFrontOffsetPoint(_openingBboxOrigin, _openingVector);
            _openingDepthPoint = GetOpeningDepthPoint(_openingBboxOrigin, _frontOffsetPoint);
            _centralDepthPoint = GetCentralOpeningDepthPoint(
                _openingBboxOrigin, _frontOffsetPoint, _openingDepthPoint);
            _rightPoint = GetRightPoint(_centralDepthPoint, _frontOffsetPoint);
            _rightFrontPoint = GetRightFrontPoint(_rightPoint, _openingVector);
            _depthPoint = GetDepthPoint(_rightFrontPoint);
            _rightDepthPoint = GetRightDepthPoint(_depthPoint, _rightFrontPoint);

            //Line testLine = _linesFromOpening.CreateLineFromOpening(_rightDepthPoint, _opening, 1000,
            //    DirectionEnum.Left);
            //_linesFromOpening.CreateTestModelLine(_forwardOffsetLine);
            //_linesFromOpening.CreateTestModelLine(testLine);


            //Line testLine2 = _linesFromOpening.CreateLineFromOpening(_rightPoint, _opening, 1000,
            //    DirectionEnum.Forward);
            //_linesFromOpening.CreateTestModelLine(testLine2);
            //_linesFromOpening.CreateTestModelLine(_forwardOffsetLine);


            //_horizontalCenterPoint = GetHorizontalCenterPoint(_rightDepthPoint, _openingVector);
            //_verticalCenterPoint = GetVerticalCenterPoint(_openingOrigin, _horizontalCenterPoint);

            _horizontalCenterPoint = GetHorizontalCenterPoint(_rightFrontPoint, _openingVector);
            _horizontalDepthPoint = GetDepthHorizotalPoint(_horizontalCenterPoint, _rightDepthPoint);
            _topPoint = GetTopPoint(_horizontalDepthPoint);
            _bottomPoint = GetBottomPoint(_horizontalDepthPoint);
            _verticalCenterPoint = GetVerticalCenterPoint(_topPoint, _bottomPoint);

            _openingHeight = GetOpeningHeight(_topPoint, _bottomPoint);

            _openingWidth = GetOpeningWidth(_verticalCenterPoint, _rightDepthPoint);
            _openingDepth = GetOpeningDepth(_rightDepthPoint, _rightFrontPoint);
            _rotationAngle = GetRotationAngle(_openingVector);

        }
        private XYZ GetFrontOffsetPoint(XYZ openingBboxOrigin, XYZ openingVector) {
            XYZ frontOffsetPoint = null;
            if(openingBboxOrigin != null && openingVector != null) {
                const double frontLineLength = 1500;
                const double backwardOffset = 500;
                XYZ startPointBbox = new XYZ(openingBboxOrigin.X, openingBboxOrigin.Y, openingBboxOrigin.Z) - openingVector
                        * _revitRepository.ConvertToFeet(backwardOffset);
                frontOffsetPoint = new XYZ(startPointBbox.X, startPointBbox.Y, startPointBbox.Z)
                    + openingVector * _revitRepository.ConvertToFeet(frontLineLength);

            }
            return frontOffsetPoint;
        }

        private XYZ GetOpeningDepthPoint(XYZ openingBboxOrigin, XYZ frontOffsetPoint) {
            XYZ closestPoint = null;
            if(openingBboxOrigin != null && frontOffsetPoint != null) {
                const double halfWidthLength = 2000;
                const double step = 0.032; //~10 мм
                double backWardLength = _revitRepository.ConvertToMillimeters(
                    openingBboxOrigin.DistanceTo(frontOffsetPoint));
                Line rightLine = _linesFromOpening.CreateLineFromOpening(
                    frontOffsetPoint, _opening, halfWidthLength, DirectionEnum.Right);
                ICollection<XYZ> points = _linesFromOpening.SplitCurveToPoints(rightLine, step);
                double closestDist = double.PositiveInfinity;
                Solid openingUnitedSolid = _solidOperations.GetUnitedSolidFromOpening(_opening);
                foreach(XYZ point in points) {
                    Line backWardLine = _linesFromOpening.CreateLineFromOpening(
                        point, _opening, backWardLength, DirectionEnum.Back);
                    SolidCurveIntersectionOptions intersectOptOutside = new SolidCurveIntersectionOptions() {
                        ResultType = SolidCurveIntersectionMode.CurveSegmentsOutside
                    };
                    SolidCurveIntersection intersection = openingUnitedSolid.IntersectWithCurve(backWardLine,
                                intersectOptOutside);
                    if(intersection.SegmentCount > 0) {
                        XYZ intersectCoord = intersection.GetCurveSegment(0).GetEndPoint(1);
                        double currentDist = _revitRepository
                            .ConvertToMillimeters(point.DistanceTo(intersectCoord));
                        if(currentDist < closestDist) {
                            closestDist = currentDist;
                            double tst = _revitRepository.ConvertToMillimeters(closestDist);
                            closestPoint = intersectCoord;
                        }
                    }
                }
            }
            return closestPoint;
        }
        private XYZ GetCentralOpeningDepthPoint(XYZ openingBboxOrigin, XYZ frontOffsetPoint, XYZ openingDepthPoint) {
            XYZ startBackwardLinePoint = null;
            if(openingBboxOrigin != null && frontOffsetPoint != null && openingDepthPoint != null) {
                double backWardLength = _revitRepository.ConvertToMillimeters(
                    openingBboxOrigin.DistanceTo(frontOffsetPoint));
                const double halfWidthLength = 2000;
                Line lineFromDepthPointToLeft = _linesFromOpening.CreateLineFromOpening(
                    openingDepthPoint, _opening, halfWidthLength, DirectionEnum.Left);
                Line backwardLineFromOffsetPoint = _linesFromOpening.CreateLineFromOpening(
                    frontOffsetPoint, _opening, backWardLength, DirectionEnum.Back);
                IList<ClosestPointsPairBetweenTwoCurves> closestPoints = new List<ClosestPointsPairBetweenTwoCurves>();
                backwardLineFromOffsetPoint.ComputeClosestPoints(lineFromDepthPointToLeft, true, false, false,
                            out closestPoints);
                startBackwardLinePoint = closestPoints.FirstOrDefault().XYZPointOnFirstCurve;
            }
            return startBackwardLinePoint;
        }
        ///// <summary>
        ///// Функция находит правую точку от окна
        ///// </summary>
        ///// <param name="forwardLine"></param>
        ///// <returns></returns>
        //private XYZ GetRightPoint(Line forwardLine) {
        //    const double step = 0.032; //~10 мм
        //    const double rightLineLength = 2000;
        //    ICollection<XYZ> points = _linesFromOpening.SplitCurveToPoints(forwardLine, step);
        //    XYZ intersectCoord = null;
        //    foreach(XYZ point in points) {
        //        Line rightLine = _linesFromOpening.CreateLineFromOpening(point, _opening,
        //            rightLineLength,
        //            DirectionEnum.Right);
        //        Element wall = _nearestElements.GetElementByRay(rightLine);
        //        if(wall != null) {
        //            Solid wallSolid = _solidOperations.GetUnitedSolidFromHostElement(wall);
        //            if(wallSolid != null) {
        //                SolidCurveIntersectionOptions intersectOptInside = new SolidCurveIntersectionOptions() {
        //                    ResultType = SolidCurveIntersectionMode.CurveSegmentsInside
        //                };
        //                SolidCurveIntersection intersection = wallSolid.IntersectWithCurve(rightLine,
        //                    intersectOptInside);
        //                if(intersection.SegmentCount > 0) {
        //                    intersectCoord = intersection.GetCurveSegment(0).GetEndPoint(0);
        //                    break;
        //                }
        //            }
        //        }
        //    }
        //    return intersectCoord;
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="startBackwardLinePoint"></param>
        /// <param name="frontOffsetPoint"></param>
        /// <returns></returns>
        private XYZ GetRightPoint(XYZ startBackwardLinePoint, XYZ frontOffsetPoint) {
            XYZ closestPoint = null;
            if(startBackwardLinePoint != null && frontOffsetPoint != null) {
                const double step = 0.032; //~10 мм
                const double rightLineLength = 2000;
                Line forwardLine = Line.CreateBound(startBackwardLinePoint, frontOffsetPoint);
                ICollection<XYZ> points = _linesFromOpening.SplitCurveToPoints(forwardLine, step);
                double closestDist = double.PositiveInfinity;
                foreach(XYZ point in points) {
                    Line rightLine = _linesFromOpening.CreateLineFromOpening(point, _opening,
                        rightLineLength,
                        DirectionEnum.Right);
                    Element wall = _nearestElements.GetElementByRay(rightLine);
                    if(wall != null) {
                        Solid wallSolid = _solidOperations.GetUnitedSolidFromHostElement(wall);
                        if(wallSolid != null) {
                            SolidCurveIntersectionOptions intersectOptInside = new SolidCurveIntersectionOptions() {
                                ResultType = SolidCurveIntersectionMode.CurveSegmentsInside
                            };
                            SolidCurveIntersection intersection = wallSolid.IntersectWithCurve(rightLine,
                                intersectOptInside);
                            if(intersection.SegmentCount > 0) {
                                XYZ intersectCoord = intersection.GetCurveSegment(0).GetEndPoint(0);
                                double currentDist = _revitRepository
                                    .ConvertToMillimeters(point.DistanceTo(intersectCoord));
                                if(currentDist < closestDist) {
                                    closestDist = currentDist;
                                    double tst = _revitRepository.ConvertToMillimeters(closestDist);
                                    closestPoint = intersectCoord;
                                }
                            }
                        }
                    }
                }
            }
            return closestPoint;
        }
        private XYZ GetRightFrontPoint(XYZ rightPoint, XYZ openingVector) {
            XYZ intersectCoord = null;
            if(rightPoint != null && openingVector != null) {
                const double backLineLength = 800;
                const double pointForwardOffset = 200;
                XYZ rightPointWithForwardOffset = rightPoint + openingVector
                    * _revitRepository.ConvertToFeet(pointForwardOffset);
                Line backLine = _linesFromOpening.CreateLineFromOpening(
                    rightPointWithForwardOffset,
                    _opening, backLineLength,
                    DirectionEnum.Back);

                Element wall = _nearestElements.GetElementByRay(backLine);
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
            }
            return intersectCoord;
        }

        //private XYZ GetRightDepthPoint(XYZ rightFrontPoint) {
        //    const double depthLineLength = 1000;
        //    Line depthLine = _linesFromOpening.CreateLineFromOpening(
        //        rightFrontPoint,
        //        _opening, depthLineLength,
        //        DirectionEnum.Back);
        //    IEnumerable<Solid> openingSolids = _opening.GetSolids();
        //    XYZ closestPoint = null;
        //    if(openingSolids.Count() > 0) {
        //        SolidCurveIntersectionOptions intersectOptOutside = new SolidCurveIntersectionOptions() {
        //            ResultType = SolidCurveIntersectionMode.CurveSegmentsOutside
        //        };
        //        double closestDist = double.PositiveInfinity;
        //        foreach(Solid solid in openingSolids) {
        //            SolidCurveIntersection intersection = solid.IntersectWithCurve(depthLine, intersectOptOutside);
        //            if(intersection.SegmentCount > 0) {
        //                XYZ intersectCoord = intersection.GetCurveSegment(0).GetEndPoint(1);
        //                double currentDist = _revitRepository
        //                    .ConvertToMillimeters(rightFrontPoint.DistanceTo(intersectCoord));
        //                if(currentDist < closestDist) {
        //                    closestDist = currentDist;
        //                    closestPoint = intersectCoord;
        //                }
        //            }
        //        }
        //    }
        //    return closestPoint;
        //}

        private XYZ GetDepthPoint(XYZ rightFrontPoint) {
            XYZ closestPoint = null;
            if(rightFrontPoint != null) {
                const double step = 0.032; //~10 мм
                const double alongsideLineLength = 300;
                const double depthLineLength = 600;
                Line alongsideLine = _linesFromOpening.CreateLineFromOpening(
                    rightFrontPoint, _opening, alongsideLineLength, DirectionEnum.Left);
                ICollection<XYZ> points = _linesFromOpening.SplitCurveToPoints(alongsideLine, step);

                Solid openingSolid = _solidOperations.GetUnitedSolidFromOpening(_opening);
                SolidCurveIntersectionOptions intersectOptOutside = new SolidCurveIntersectionOptions() {
                    ResultType = SolidCurveIntersectionMode.CurveSegmentsOutside
                };
                double closestDist = double.PositiveInfinity;
                foreach(XYZ point in points) {
                    Line backwardLine = _linesFromOpening.CreateLineFromOpening(
                point, _opening, depthLineLength, DirectionEnum.Back);
                    SolidCurveIntersection intersection = openingSolid.IntersectWithCurve(
                            backwardLine, intersectOptOutside);
                    if(intersection.SegmentCount > 0) {
                        XYZ intersectCoord = intersection.GetCurveSegment(0).GetEndPoint(1);
                        double currentDist = _revitRepository
                            .ConvertToMillimeters(point.DistanceTo(intersectCoord));
                        if(currentDist < closestDist) {
                            closestDist = currentDist;
                            double tst = _revitRepository.ConvertToMillimeters(closestDist);
                            closestPoint = intersectCoord;
                        }
                    }
                }
            }
            return closestPoint;
        }

        private XYZ GetRightDepthPoint(XYZ depthPoint, XYZ rightFrontPoint) {
            XYZ rightDepthPoint = null;

            if(depthPoint != null && rightFrontPoint != null) {
                const double alongsideLineLength = 300;
                const double depthLineLength = 600;
                Line depthLine = _linesFromOpening.CreateLineFromOpening(
                    rightFrontPoint, _opening, depthLineLength, DirectionEnum.Back);
                Line lineFromClosestPoint = _linesFromOpening.CreateLineFromOpening(
                    depthPoint, _opening, alongsideLineLength, DirectionEnum.Right);

                IList<ClosestPointsPairBetweenTwoCurves> closestPoints = new List<ClosestPointsPairBetweenTwoCurves>();
                depthLine.ComputeClosestPoints(lineFromClosestPoint, true, false, false,
                            out closestPoints);
                rightDepthPoint = closestPoints.FirstOrDefault().XYZPointOnFirstCurve;
            }
            return rightDepthPoint;
        }

        //private XYZ GetHorizontalCenterPoint(Line forwardOffsetLine, XYZ rightDepthPoint) {
        //    XYZ origin = forwardOffsetLine.GetEndPoint(0);
        //    XYZ directionForwardLine = forwardOffsetLine.Direction;
        //    double t;

        //    if(Math.Abs(directionForwardLine.X) > double.Epsilon)
        //        t = (rightDepthPoint.X - origin.X) / directionForwardLine.X;
        //    else
        //        t = (rightDepthPoint.Y - origin.Y) / directionForwardLine.Y; // Используем y-компоненту

        //    // Вычисление координат точки пересечения
        //    double intersectionX = origin.X + t * directionForwardLine.X;
        //    double intersectionY = origin.Y + t * directionForwardLine.Y;
        //    double intersectionZ = origin.Z + t * directionForwardLine.Z;

        //    XYZ intersectCoord = new XYZ(intersectionX, intersectionY, intersectionZ);

        //    return intersectCoord;
        //}
        //private XYZ GetHorizontalCenterPoint(XYZ rightDepthPoint, XYZ openingVector) {
        //    const double leftLineLength = 4000;
        //    const double offset = 10;
        //    XYZ normalVector = XYZ.BasisZ.CrossProduct(openingVector).Normalize();
        //    XYZ startPoint = new XYZ(rightDepthPoint.X, rightDepthPoint.Y, rightDepthPoint.Z) - normalVector
        //            * _revitRepository.ConvertToFeet(offset);
        //    Line leftLine = _linesFromOpening.CreateLineFromOpening(startPoint, _opening,
        //            leftLineLength,
        //            DirectionEnum.Left);
        //    Element wall = _nearestElements.GetElementByRay(leftLine);
        //    XYZ centerPoint = null;
        //    if(wall != null) {
        //        Solid wallSolid = _solidOperations.GetUnitedSolidFromHostElement(wall);
        //        if(wallSolid != null) {
        //            SolidCurveIntersectionOptions intersectOptOutside = new SolidCurveIntersectionOptions() {
        //                ResultType = SolidCurveIntersectionMode.CurveSegmentsOutside
        //            };
        //            SolidCurveIntersection intersection = wallSolid.IntersectWithCurve(leftLine, intersectOptOutside);
        //            if(intersection.SegmentCount > 0) {
        //                XYZ intersectCoord = intersection.GetCurveSegment(0).GetEndPoint(1);
        //                centerPoint = new XYZ((intersectCoord.X + rightDepthPoint.X) / 2,
        //                    (intersectCoord.Y + rightDepthPoint.Y) / 2,
        //                    intersectCoord.Z);
        //            }
        //        }
        //    }
        //    return centerPoint;
        //}
        private XYZ GetHorizontalCenterPoint(XYZ rightFrontpoint, XYZ openingVector) {
            XYZ centerPoint = null;

            if(rightFrontpoint != null && openingVector != null) {
                const double leftLineLength = 4000;
                const double offset = 10;
                XYZ normalVector = XYZ.BasisZ.CrossProduct(openingVector).Normalize();
                XYZ startPoint = new XYZ(rightFrontpoint.X, rightFrontpoint.Y, rightFrontpoint.Z) - normalVector
                        * _revitRepository.ConvertToFeet(offset);
                Line leftLine = _linesFromOpening.CreateLineFromOpening(startPoint, _opening,
                        leftLineLength,
                        DirectionEnum.Left);
                Element wall = _nearestElements.GetElementByRay(leftLine);
                if(wall != null) {
                    Solid wallSolid = _solidOperations.GetUnitedSolidFromHostElement(wall);
                    if(wallSolid != null) {
                        SolidCurveIntersectionOptions intersectOptOutside = new SolidCurveIntersectionOptions() {
                            ResultType = SolidCurveIntersectionMode.CurveSegmentsOutside
                        };
                        SolidCurveIntersection intersection = wallSolid.IntersectWithCurve(leftLine, intersectOptOutside);
                        if(intersection.SegmentCount > 0) {
                            XYZ intersectCoord = intersection.GetCurveSegment(0).GetEndPoint(1);
                            centerPoint = new XYZ((intersectCoord.X + rightFrontpoint.X) / 2,
                                (intersectCoord.Y + rightFrontpoint.Y) / 2,
                                intersectCoord.Z);
                        }
                    }
                }
            }
            return centerPoint;
        }

        private XYZ GetDepthHorizotalPoint(XYZ horizontalPoint, XYZ depthRightPoint) {
            XYZ depthHorizontalPoint = null;

            if(horizontalPoint != null && depthRightPoint != null) {
                const double depthLength = 800;
                const double halfWidthLength = 2000;
                Line lineFromHorizontalPoint = _linesFromOpening.CreateLineFromOpening(
                    horizontalPoint, _opening, depthLength, DirectionEnum.Back);
                Line lineFromRightPoint = _linesFromOpening.CreateLineFromOpening(
                    depthRightPoint, _opening, halfWidthLength, DirectionEnum.Left);
                IList<ClosestPointsPairBetweenTwoCurves> closestPoints = new List<ClosestPointsPairBetweenTwoCurves>();
                lineFromHorizontalPoint.ComputeClosestPoints(lineFromRightPoint, true, false, false,
                            out closestPoints);
                depthHorizontalPoint = closestPoints.FirstOrDefault().XYZPointOnFirstCurve;
            }
            return depthHorizontalPoint;
        }

        //private XYZ GetVerticalCenterPoint(XYZ openingOrigin, XYZ horizontalCenterPoint) {
        //    XYZ verticalCenter = null;
        //    const double topLength = 6000;
        //    Line upwardLine = _linesFromOpening.CreateLineFromOpening(horizontalCenterPoint,
        //        _opening, topLength, DirectionEnum.Top);
        //    Element topElement = _nearestElements.GetElementByRay(upwardLine);
        //    if(topElement == null) {
        //        return null;
        //    }
        //    Solid topSolid = _solidOperations.GetUnitedSolidFromHostElement(topElement);

        //    if(topSolid != null) {

        //        SolidCurveIntersectionOptions intersectOptOutside = new SolidCurveIntersectionOptions() {
        //            ResultType = SolidCurveIntersectionMode.CurveSegmentsOutside
        //        };
        //        SolidCurveIntersection intersection = topSolid.IntersectWithCurve(upwardLine, intersectOptOutside);
        //        if(intersection.SegmentCount > 0) {
        //            XYZ intersectCoord = intersection.GetCurveSegment(0).GetEndPoint(1);
        //            double centerZ = (intersectCoord.Z + openingOrigin.Z) / 2;
        //            verticalCenter = new XYZ(intersectCoord.X, intersectCoord.Y, centerZ);
        //        } else {
        //            throw new ArgumentException("Над окном нет элемента, пересекающегося с окном");
        //        }
        //    }
        //    return verticalCenter;
        //}

        private XYZ GetTopPoint(XYZ horizontalDepthPoint) {
            XYZ intersectCoord = null;

            if(horizontalDepthPoint != null) {
                const double lineLength = 6000;
                Line upwardLine = _linesFromOpening.CreateLineFromOpening(horizontalDepthPoint,
                    _opening, lineLength, DirectionEnum.Top);

                Element topElement = _nearestElements.GetElementByRay(upwardLine);
                if(topElement == null) {
                    return null;
                }
                Solid topSolid = _solidOperations.GetUnitedSolidFromHostElement(topElement);
                if(topSolid != null) {

                    SolidCurveIntersectionOptions intersectOptOutside = new SolidCurveIntersectionOptions() {
                        ResultType = SolidCurveIntersectionMode.CurveSegmentsOutside
                    };
                    SolidCurveIntersection topIntersection = topSolid.IntersectWithCurve(
                        upwardLine, intersectOptOutside);

                    if(topIntersection.SegmentCount > 0) {
                        intersectCoord = topIntersection.GetCurveSegment(0).GetEndPoint(1);
                    }
                }
            }
            return intersectCoord;
        }

        private XYZ GetBottomPoint(XYZ horizontalDepthPoint) {
            XYZ intersectCoord = null;

            if(horizontalDepthPoint != null) {
                const double lineLength = 6000;
                Line bottomLine = _linesFromOpening.CreateLineFromOpening(horizontalDepthPoint,
                    _opening, lineLength, DirectionEnum.Down);

                Element bottomElement = _nearestElements.GetElementByRay(bottomLine);
                if(bottomElement == null) {
                    return null;
                }
                Solid topSolid = _solidOperations.GetUnitedSolidFromHostElement(bottomElement);
                if(topSolid != null) {

                    SolidCurveIntersectionOptions intersectOptOutside = new SolidCurveIntersectionOptions() {
                        ResultType = SolidCurveIntersectionMode.CurveSegmentsOutside
                    };
                    SolidCurveIntersection topIntersection = topSolid.IntersectWithCurve(
                        bottomLine, intersectOptOutside);

                    if(topIntersection.SegmentCount > 0) {
                        intersectCoord = topIntersection.GetCurveSegment(0).GetEndPoint(1);
                    }
                }
            }
            return intersectCoord;
        }

        private XYZ GetVerticalCenterPoint(XYZ topPoint, XYZ bottomPoint) {
            XYZ verticalCenter = null;
            if(topPoint != null && bottomPoint != null) {
                double x = (topPoint.X + bottomPoint.X) / 2;
                double y = (topPoint.Y + bottomPoint.Y) / 2;
                double z = (topPoint.Z + bottomPoint.Z) / 2;
                verticalCenter = new XYZ(x, y, z);
            }
            return verticalCenter;
        }

        private double GetOpeningHeight(XYZ topPoint, XYZ bottomPoint) {
            double openingHeight = 0;
            if(topPoint != null && bottomPoint != null) {
                openingHeight = bottomPoint.DistanceTo(topPoint);
            }
            return openingHeight;
        }

        private double GetOpeningWidth(XYZ verticalCenterPoint, XYZ rightDepthPoint) {
            double openingWidth = 0;
            if(verticalCenterPoint != null && rightDepthPoint != null) {
                openingWidth = Math.Sqrt(Math.Pow(verticalCenterPoint.X - rightDepthPoint.X, 2)
                    + Math.Pow(verticalCenterPoint.Y - rightDepthPoint.Y, 2)) * 2;
            }
            return openingWidth;
        }

        private double GetOpeningDepth(XYZ rightDepthPoint, XYZ rightFrontPoint) {
            double openingDepth = 0;
            if(rightDepthPoint != null && rightFrontPoint != null) {
                openingDepth = rightDepthPoint.DistanceTo(rightFrontPoint);
            }
            return openingDepth;
        }

        private double GetRotationAngle(XYZ openingVector) {
            double radians = 0;
            if(openingVector != null) {
                XYZ originVector = new XYZ(0, -1, 0);
                double scalarMultiply = ScalarMultiply(originVector, openingVector);
                double originMagnitude = Magnitude(originVector.X, originVector.Y);
                double openingMagnitude = Magnitude(openingVector.X, openingVector.Y);
                double cosTheta = scalarMultiply / (originMagnitude * openingMagnitude);
                radians = Math.Acos(cosTheta);
                if(openingVector.X < 0) {
                    return -radians;
                } else {
                    return radians;
                }
            }
            return radians;
        }

        //private double GetRotationAngle(XYZ openingVector) {
        //    double radiansAngle = (_opening.Location as LocationPoint).Rotation;
        //    if(openingVector.X < 0) {
        //        return -radiansAngle;
        //    } else {
        //        return radiansAngle;
        //    }
        //}

        private double ScalarMultiply(XYZ originVector, XYZ openingVector) {
            if(originVector != null && openingVector != null) {
                return openingVector.X * originVector.X + openingVector.Y * originVector.Y;
            } else {
                return 0;
            }
        }
        private double Magnitude(double x, double y) {
            return Math.Sqrt(x * x + y * y);
        }
    }
}
