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

        private Outline _outlineWithOffset;
        private BoundingBoxXYZ _openingBoundingBox;
        private Solid _nearestElementsSolid;
        private Solid _openingSolid;
        private XYZ _frontOffsetPoint;
        private XYZ _centralBackwardOffsetPoint;
        private XYZ _openingDepthPoint;
        private XYZ _centralOpeningDepthPoint;
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

        private double _openingHeight = 0;
        private double _openingWidth = 0;
        private double _openingDepth = 0;
        private double _rotationAngle = 0;

        public OpeningHandler(RevitRepository revitRepository, FamilyInstance opening) {
            _revitRepository = revitRepository
                ?? throw new ArgumentNullException(nameof(revitRepository));
            _linesFromOpening = new LinesFromOpening(revitRepository);
            _nearestElements = new NearestElements(revitRepository);
            _opening = opening
                ?? throw new ArgumentNullException(nameof(opening));
            _solidOperations = new SolidOperations(revitRepository);
            CheckVectorDirection();
        }

        private XYZ GetOpeningVector() {
            if(_openingVector != null) {
                return _openingVector;
            } else {
                _openingVector = _revitRepository.GetOpeningVector(_opening);
                return _openingVector;
            }

        }

        public XYZ GetOpeningBoundingBoxOrigin() {
            if(_openingBboxOrigin != null) {
                return _openingBboxOrigin;
            } else {
                BoundingBoxXYZ openingBoundingBox = GetOpeningGeometryBbox();
                XYZ minPoint = openingBoundingBox.Min;
                XYZ maxPoint = openingBoundingBox.Max;

                double x = (minPoint.X + maxPoint.X) / 2;
                double y = (minPoint.Y + maxPoint.Y) / 2;
                double z = (minPoint.Z + maxPoint.Z) / 2;
                _openingBboxOrigin = new XYZ(x, y, z);

                return _openingBboxOrigin;
            }
        }
        private BoundingBoxXYZ GetOpeningGeometryBbox() {
            if(_openingBoundingBox != null) {
                return _openingBoundingBox;
            } else {
                Solid openingSolid = GetOpeningSolid();
                if(openingSolid != null) {

                    BoundingBoxXYZ bbox = openingSolid.GetBoundingBox();
                    _openingBoundingBox = new BoundingBoxXYZ() {
                        Min = bbox.Transform.OfPoint(bbox.Min),
                        Max = bbox.Transform.OfPoint(bbox.Max)
                    };
                }
                return _openingBoundingBox;
            }
        }

        private Solid GetOpeningSolid() {
            if(_openingSolid != null) {
                return _openingSolid;
            } else {
                _openingSolid = _solidOperations.GetUnitedSolidFromOpening(_opening);
                return _openingSolid;
            }
        }

        private XYZ GetCentralBackwardOffsetPoint() {
            if(_centralBackwardOffsetPoint != null) {
                return _centralBackwardOffsetPoint;
            } else {
                XYZ openingBboxOrigin = GetOpeningBoundingBoxOrigin();
                XYZ openingVector = GetOpeningVector();
                if(openingBboxOrigin != null && openingVector != null) {
                    const double backwardOffset = 500;
                    _centralBackwardOffsetPoint = new XYZ(
                        openingBboxOrigin.X, openingBboxOrigin.Y, openingBboxOrigin.Z)
                            - openingVector * _revitRepository.ConvertToFeet(backwardOffset);
                }
                return _centralBackwardOffsetPoint;
            }
        }

        private XYZ GetFrontOffsetPoint(double frontLineLength = 1500) {
            if(_frontOffsetPoint != null) {
                return _frontOffsetPoint;
            } else {
                XYZ centralBackwardOffsetPoint = GetCentralBackwardOffsetPoint();
                XYZ openingVector = GetOpeningVector();
                if(centralBackwardOffsetPoint != null && openingVector != null) {
                    _frontOffsetPoint = new XYZ(
                    centralBackwardOffsetPoint.X, centralBackwardOffsetPoint.Y, centralBackwardOffsetPoint.Z)
                    + openingVector * _revitRepository.ConvertToFeet(frontLineLength);
                }
                return _frontOffsetPoint;
            }
        }

        private Outline GetOutlineWithOffset() {
            if(_outlineWithOffset != null) {
                return _outlineWithOffset;
            } else {
                BoundingBoxXYZ openingBoundingBox = GetOpeningGeometryBbox();
                if(openingBoundingBox != null) {
                    double offsetLength = _revitRepository.ConvertToFeet(300);
                    XYZ minPoint = openingBoundingBox.Min - new XYZ(1, 1, 1) * offsetLength;
                    XYZ maxPoint = openingBoundingBox.Max + new XYZ(1, 1, 1) * offsetLength;
                    _outlineWithOffset = new Outline(minPoint, maxPoint);
                }
                return _outlineWithOffset;
            }
        }

        private void CheckVectorDirection() {
            const double lineLength = 100;
            XYZ originPoint = GetOpeningBoundingBoxOrigin();
            XYZ openingVector = GetOpeningVector();

            Line forwardLine = _linesFromOpening.CreateLineFromOpening(
                originPoint, openingVector, lineLength, DirectionEnum.Forward);

            Line backwardLine = _linesFromOpening.CreateLineFromOpening(
                originPoint, openingVector, lineLength, DirectionEnum.Back);

            IList<Element> forwardElements = _nearestElements.GetElementsByRay(forwardLine);
            IList<Element> backwardElements = _nearestElements.GetElementsByRay(backwardLine);

            if(forwardElements.Count > backwardElements.Count) {
                _openingVector = _openingVector.Negate();
            }

        }

        private Solid GetUnitedSolidFromBoundingBox() {
            if(_nearestElementsSolid != null) {
                return _nearestElementsSolid;
            } else {
                Outline outlineWithOffset = GetOutlineWithOffset();
                if(outlineWithOffset != null) {

                    ElementFilter categoryFilter = new ElementMulticategoryFilter(
                    new BuiltInCategory[] {
                    BuiltInCategory.OST_Walls,
                    BuiltInCategory.OST_Columns,
                    BuiltInCategory.OST_StructuralColumns,
                    BuiltInCategory.OST_StructuralFraming,
                    BuiltInCategory.OST_Floors});

                    BoundingBoxIntersectsFilter bboxIntersectFilter =
                    new BoundingBoxIntersectsFilter(outlineWithOffset);

                    IEnumerable<Element> nearestElements = new FilteredElementCollector(_revitRepository.Document)
                        .WhereElementIsNotElementType()
                        .WherePasses(categoryFilter)
                        .WherePasses(bboxIntersectFilter)
                        .ToElements();

                    IList<Solid> nearestSolids = nearestElements
                        .Select(el => _solidOperations.GetUnitedSolid(el.GetSolids()))
                        .ToList();

                    _nearestElementsSolid = _solidOperations.GetUnitedSolid(nearestSolids);
                    //_solidOperations.CreateDirectShape(_nearestElementsSolid);
                }
            }
            return _nearestElementsSolid;
        }

        private XYZ GetOpeningDepthPoint() {
            if(_openingDepthPoint != null) {
                return _openingDepthPoint;
            } else {
                XYZ centralBackwardOffsetPoint = GetCentralBackwardOffsetPoint();
                XYZ frontOffsetPoint = GetFrontOffsetPoint();
                if(centralBackwardOffsetPoint != null && frontOffsetPoint != null) {
                    const double halfWidthLength = 2000;
                    const double step = 0.032; //~10 мм
                    double closestDist = double.PositiveInfinity;
                    double backWardLength = _revitRepository.ConvertToMillimeters(
                        centralBackwardOffsetPoint.DistanceTo(frontOffsetPoint));
                    XYZ openingVector = GetOpeningVector();

                    Line rightLine = _linesFromOpening.CreateLineFromOpening(
                        frontOffsetPoint, openingVector, halfWidthLength, DirectionEnum.Right);
                    ICollection<XYZ> points = _linesFromOpening.SplitCurveToPoints(rightLine, step);
                    Solid openingSolid = GetOpeningSolid();

                    foreach(XYZ point in points) {
                        Line backWardLine = _linesFromOpening.CreateLineFromOpening(
                            point, openingVector, backWardLength, DirectionEnum.Back);
                        SolidCurveIntersectionOptions intersectOptOutside = new SolidCurveIntersectionOptions() {
                            ResultType = SolidCurveIntersectionMode.CurveSegmentsOutside
                        };
                        SolidCurveIntersection intersection = openingSolid.IntersectWithCurve(backWardLine,
                                    intersectOptOutside);
                        if(intersection.SegmentCount > 0) {
                            XYZ intersectCoord = intersection.GetCurveSegment(0).GetEndPoint(1);
                            double currentDist = point.DistanceTo(intersectCoord);
                            if(currentDist < closestDist) {
                                closestDist = currentDist;
                                double tst = _revitRepository.ConvertToMillimeters(closestDist);
                                _openingDepthPoint = intersectCoord;
                            }
                        }
                    }
                }
                return _openingDepthPoint;
            }
        }

        private XYZ GetCentralOpeningDepthPoint() {
            if(_centralOpeningDepthPoint != null) {
                return _centralOpeningDepthPoint;
            } else {
                XYZ centralBackwardOffsetPoint = GetCentralBackwardOffsetPoint();
                XYZ frontOffsetPoint = GetFrontOffsetPoint();
                XYZ openingDepthPoint = GetOpeningDepthPoint();
                XYZ openingVector = GetOpeningVector();

                if(centralBackwardOffsetPoint != null && frontOffsetPoint != null && openingDepthPoint != null) {
                    double backWardLength = _revitRepository.ConvertToMillimeters(
                        centralBackwardOffsetPoint.DistanceTo(frontOffsetPoint));
                    const double halfWidthLength = 2000;

                    Line lineFromDepthPointToLeft = _linesFromOpening.CreateLineFromOpening(
                        openingDepthPoint, openingVector, halfWidthLength, DirectionEnum.Left);
                    Line backwardLineFromOffsetPoint = _linesFromOpening.CreateLineFromOpening(
                        frontOffsetPoint, openingVector, backWardLength, DirectionEnum.Back);

                    IList<ClosestPointsPairBetweenTwoCurves> closestPoints =
                        new List<ClosestPointsPairBetweenTwoCurves>();

                    backwardLineFromOffsetPoint.ComputeClosestPoints(lineFromDepthPointToLeft, true, false, false,
                                out closestPoints);
                    _centralOpeningDepthPoint = closestPoints.FirstOrDefault().XYZPointOnFirstCurve;
                }
                return _centralOpeningDepthPoint;
            }
        }

        private XYZ GetRightPoint() {
            if(_rightPoint != null) {
                return _rightPoint;
            } else {
                XYZ centralBackwardDepthPoint = GetCentralOpeningDepthPoint();
                XYZ frontOffsetPoint = GetFrontOffsetPoint();
                Solid nearestElementsSolid = GetUnitedSolidFromBoundingBox();
                XYZ openingVector = GetOpeningVector();

                if(centralBackwardDepthPoint != null && frontOffsetPoint != null && nearestElementsSolid != null) {
                    const double step = 0.032; //~10 мм
                    const double rightLineLength = 2000;
                    double closestDist = double.PositiveInfinity;

                    Line forwardLine = Line.CreateBound(frontOffsetPoint, centralBackwardDepthPoint);
                    ICollection<XYZ> points = _linesFromOpening.SplitCurveToPoints(forwardLine, step);
                    foreach(XYZ point in points) {
                        Line rightLine = _linesFromOpening.CreateLineFromOpening(point, openingVector,
                            rightLineLength,
                            DirectionEnum.Right);
                        SolidCurveIntersectionOptions intersectOptInside = new SolidCurveIntersectionOptions() {
                            ResultType = SolidCurveIntersectionMode.CurveSegmentsInside
                        };
                        SolidCurveIntersection intersection = nearestElementsSolid.IntersectWithCurve(rightLine,
                            intersectOptInside);
                        if(intersection.SegmentCount > 0) {
                            XYZ intersectCoord = intersection.GetCurveSegment(0).GetEndPoint(0);
                            double currentDist = _revitRepository
                                .ConvertToMillimeters(point.DistanceTo(intersectCoord));

                            if(currentDist < closestDist) {
                                closestDist = currentDist;
                                _rightPoint = intersectCoord;
                            }
                        }
                    }
                }
                if(_rightPoint == null) {
                    throw new ArgumentException("Окно не углублено внутрь фасада");
                }
                return _rightPoint;
            }
        }

        private XYZ GetRightFrontPoint() {
            if(_rightFrontPoint != null) {
                return _rightFrontPoint;
            } else {
                XYZ rightPoint = GetRightPoint();
                XYZ openingVector = GetOpeningVector();
                Solid nearestElementsSolid = GetUnitedSolidFromBoundingBox();
                if(rightPoint != null && openingVector != null & nearestElementsSolid != null) {
                    const double backLineLength = 1000;
                    const double pointForwardOffset = 800;
                    XYZ rightPointWithForwardOffset = rightPoint + openingVector
                        * _revitRepository.ConvertToFeet(pointForwardOffset);
                    Line backLine = _linesFromOpening.CreateLineFromOpening(
                        rightPointWithForwardOffset,
                        openingVector, backLineLength,
                        DirectionEnum.Back);

                    SolidCurveIntersectionOptions intersectOptInside = new SolidCurveIntersectionOptions() {
                        ResultType = SolidCurveIntersectionMode.CurveSegmentsInside
                    };
                    SolidCurveIntersection intersection = nearestElementsSolid
                    .IntersectWithCurve(backLine, intersectOptInside);
                    if(intersection.SegmentCount > 0) {
                        _rightFrontPoint = intersection.GetCurveSegment(0).GetEndPoint(0);
                    }
                }
                return _rightFrontPoint;
            }
        }

        private XYZ GetDepthPoint() {
            if(_depthPoint != null) {
                return _depthPoint;
            } else {
                XYZ rightFrontPoint = GetRightFrontPoint();
                Solid openingSolid = GetOpeningSolid();
                XYZ openingVector = GetOpeningVector();

                if(rightFrontPoint != null & openingSolid != null) {
                    const double step = 0.032; //~10 мм
                    const double alongsideLineLength = 300;
                    const double depthLineLength = 600;
                    double closestDist = double.PositiveInfinity;

                    Line alongsideLine = _linesFromOpening.CreateLineFromOpening(
                        rightFrontPoint, openingVector, alongsideLineLength, DirectionEnum.Left);
                    ICollection<XYZ> points = _linesFromOpening.SplitCurveToPoints(alongsideLine, step);
                    SolidCurveIntersectionOptions intersectOptOutside = new SolidCurveIntersectionOptions() {
                        ResultType = SolidCurveIntersectionMode.CurveSegmentsOutside
                    };

                    foreach(XYZ point in points) {
                        Line backwardLine = _linesFromOpening.CreateLineFromOpening(
                    point, openingVector, depthLineLength, DirectionEnum.Back);
                        SolidCurveIntersection intersection = openingSolid.IntersectWithCurve(
                                backwardLine, intersectOptOutside);
                        if(intersection.SegmentCount > 0) {
                            XYZ intersectCoord = intersection.GetCurveSegment(0).GetEndPoint(1);
                            double currentDist = _revitRepository
                                .ConvertToMillimeters(point.DistanceTo(intersectCoord));
                            if(currentDist < closestDist) {
                                closestDist = currentDist;
                                _depthPoint = intersectCoord;
                            }
                        }
                    }
                }
                return _depthPoint;
            }
        }

        private XYZ GetRightDepthPoint() {
            if(_rightDepthPoint != null) {
                return _rightDepthPoint;
            } else {
                XYZ depthPoint = GetDepthPoint();
                XYZ rightFrontPoint = GetRightFrontPoint();
                XYZ openingVector = GetOpeningVector();

                if(depthPoint != null && rightFrontPoint != null) {
                    const double alongsideLineLength = 300;
                    const double depthLineLength = 600;

                    Line depthLine = _linesFromOpening.CreateLineFromOpening(
                        rightFrontPoint, openingVector, depthLineLength, DirectionEnum.Back);
                    Line lineFromClosestPoint = _linesFromOpening.CreateLineFromOpening(
                        depthPoint, openingVector, alongsideLineLength, DirectionEnum.Right);

                    IList<ClosestPointsPairBetweenTwoCurves> closestPoints =
                        new List<ClosestPointsPairBetweenTwoCurves>();
                    depthLine.ComputeClosestPoints(lineFromClosestPoint, true, false, false,
                                out closestPoints);
                    _rightDepthPoint = closestPoints.FirstOrDefault().XYZPointOnFirstCurve;
                }
                return _rightDepthPoint;
            }
        }

        private XYZ GetHorizontalCenterPoint() {
            if(_horizontalCenterPoint != null) {
                return _horizontalCenterPoint;
            } else {
                XYZ rightFrontpoint = GetRightFrontPoint();
                XYZ openingVector = GetOpeningVector();
                Solid nearestElementsSolid = GetUnitedSolidFromBoundingBox();

                if(rightFrontpoint != null && openingVector != null && nearestElementsSolid != null) {
                    const double leftLineLength = 4000;
                    const double offset = 10;

                    XYZ normalVector = XYZ.BasisZ.CrossProduct(openingVector).Normalize();
                    XYZ startPoint = new XYZ(rightFrontpoint.X, rightFrontpoint.Y, rightFrontpoint.Z) - normalVector
                            * _revitRepository.ConvertToFeet(offset);
                    Line leftLine = _linesFromOpening.CreateLineFromOpening(startPoint, openingVector,
                            leftLineLength,
                            DirectionEnum.Left);

                    SolidCurveIntersectionOptions intersectOptOutside = new SolidCurveIntersectionOptions() {
                        ResultType = SolidCurveIntersectionMode.CurveSegmentsOutside
                    };
                    SolidCurveIntersection intersection = nearestElementsSolid
                    .IntersectWithCurve(leftLine, intersectOptOutside);

                    if(intersection.SegmentCount > 0) {
                        XYZ intersectCoord = intersection.GetCurveSegment(0).GetEndPoint(1);
                        _horizontalCenterPoint = new XYZ((intersectCoord.X + rightFrontpoint.X) / 2,
                            (intersectCoord.Y + rightFrontpoint.Y) / 2,
                            intersectCoord.Z);
                    }
                }
                return _horizontalCenterPoint;
            }
        }

        private XYZ GetHorizontalDepthPoint() {
            if(_horizontalDepthPoint != null) {
                return _horizontalDepthPoint;
            } else {
                XYZ horizontalCenterPoint = GetHorizontalCenterPoint();
                XYZ rightDepthPoint = GetRightDepthPoint();
                XYZ openingVector = GetOpeningVector();

                if(horizontalCenterPoint != null && rightDepthPoint != null) {
                    const double depthLength = 800;
                    const double halfWidthLength = 2000;

                    Line lineFromHorizontalPoint = _linesFromOpening.CreateLineFromOpening(
                        horizontalCenterPoint, openingVector, depthLength, DirectionEnum.Back);
                    Line lineFromRightPoint = _linesFromOpening.CreateLineFromOpening(
                        rightDepthPoint, openingVector, halfWidthLength, DirectionEnum.Left);

                    IList<ClosestPointsPairBetweenTwoCurves> closestPoints =
                        new List<ClosestPointsPairBetweenTwoCurves>();
                    lineFromHorizontalPoint.ComputeClosestPoints(lineFromRightPoint, true, false, false,
                                out closestPoints);
                    _horizontalDepthPoint = closestPoints.FirstOrDefault().XYZPointOnFirstCurve;
                }
                return _horizontalDepthPoint;
            }
        }

        private XYZ GetTopPoint() {
            if(_topPoint != null) {
                return _topPoint;
            } else {
                XYZ horizontalDepthPoint = GetHorizontalDepthPoint();
                Solid nearestElementsSolid = GetUnitedSolidFromBoundingBox();
                XYZ openingVector = GetOpeningVector();

                if(horizontalDepthPoint != null && nearestElementsSolid != null) {
                    const double lineLength = 6000;

                    Line upwardLine = _linesFromOpening.CreateLineFromOpening(horizontalDepthPoint,
                        openingVector, lineLength, DirectionEnum.Top);

                    SolidCurveIntersectionOptions intersectOptOutside = new SolidCurveIntersectionOptions() {
                        ResultType = SolidCurveIntersectionMode.CurveSegmentsOutside
                    };
                    SolidCurveIntersection topIntersection = nearestElementsSolid.IntersectWithCurve(
                        upwardLine, intersectOptOutside);

                    if(topIntersection.SegmentCount > 0) {
                        _topPoint = topIntersection.GetCurveSegment(0).GetEndPoint(1);
                    }
                }
                return _topPoint;
            }
        }

        private XYZ GetBottomPoint() {
            if(_bottomPoint != null) {
                return _bottomPoint;
            } else {
                XYZ horizontalDepthPoint = GetHorizontalDepthPoint();
                Solid nearestElementsSolid = GetUnitedSolidFromBoundingBox();
                XYZ openingVector = GetOpeningVector();

                if(horizontalDepthPoint != null && nearestElementsSolid != null) {
                    const double lineLength = 6000;
                    Line bottomLine = _linesFromOpening.CreateLineFromOpening(horizontalDepthPoint,
                        openingVector, lineLength, DirectionEnum.Down);

                    SolidCurveIntersectionOptions intersectOptOutside = new SolidCurveIntersectionOptions() {
                        ResultType = SolidCurveIntersectionMode.CurveSegmentsOutside
                    };
                    SolidCurveIntersection bottomIntersection = nearestElementsSolid.IntersectWithCurve(
                        bottomLine, intersectOptOutside);

                    if(bottomIntersection.SegmentCount > 0) {
                        _bottomPoint = bottomIntersection.GetCurveSegment(0).GetEndPoint(1);
                    }
                }
                return _bottomPoint;
            }
        }

        public XYZ GetVerticalCenterPoint() {
            if(_verticalCenterPoint != null) {
                return _verticalCenterPoint;
            } else {
                XYZ topPoint = GetTopPoint();
                XYZ bottomPoint = GetBottomPoint();

                if(topPoint != null && bottomPoint != null) {
                    double x = (topPoint.X + bottomPoint.X) / 2;
                    double y = (topPoint.Y + bottomPoint.Y) / 2;
                    double z = (topPoint.Z + bottomPoint.Z) / 2;
                    _verticalCenterPoint = new XYZ(x, y, z);
                }
                return _verticalCenterPoint;
            }
        }

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

        public double GetOpeningHeight() {
            if(_openingHeight > 0) {
                return _openingHeight;
            } else {
                XYZ topPoint = GetTopPoint();
                XYZ bottomPoint = GetBottomPoint();

                if(topPoint != null && bottomPoint != null) {
                    _openingHeight = bottomPoint.DistanceTo(topPoint);
                } else {
                    throw new ArgumentException("Не удалось рассчитать высоту проема");
                }
                return _openingHeight;
            }
        }

        public double GetOpeningWidth() {
            if(_openingWidth > 0) {
                return _openingWidth;
            } else {
                XYZ verticalCenterPoint = GetVerticalCenterPoint();
                XYZ rightDepthPoint = GetRightDepthPoint();

                if(verticalCenterPoint != null && rightDepthPoint != null) {
                    _openingWidth = Math.Sqrt(Math.Pow(verticalCenterPoint.X - rightDepthPoint.X, 2)
                        + Math.Pow(verticalCenterPoint.Y - rightDepthPoint.Y, 2)) * 2;
                } else {
                    throw new ArgumentException("не удалось рассчитать ширину проема");
                }
                return _openingWidth;

            }
        }

        public double GetOpeningDepth() {
            if(_openingDepth > 0) {
                return _openingDepth;
            } else {
                XYZ rightDepthPoint = GetRightDepthPoint();
                XYZ rightFrontPoint = GetRightFrontPoint();

                if(rightDepthPoint != null && rightFrontPoint != null) {
                    _openingDepth = rightDepthPoint.DistanceTo(rightFrontPoint);
                } else {
                    throw new ArgumentException("Не удалось рассчитать глубину проема");
                }
                return _openingDepth;
            }
        }

        public double GetRotationAngle() {
            if(_rotationAngle > 0) {
                return _rotationAngle;
            } else {
                XYZ openingVector = GetOpeningVector();

                if(openingVector != null) {
                    XYZ originVector = new XYZ(0, -1, 0);
                    double scalarMultiply = ScalarMultiply(originVector, openingVector);
                    double originMagnitude = Magnitude(originVector.X, originVector.Y);
                    double openingMagnitude = Magnitude(openingVector.X, openingVector.Y);
                    double cosTheta = scalarMultiply / (originMagnitude * openingMagnitude);

                    _rotationAngle = Math.Acos(cosTheta);
                    if(openingVector.X < 0) {
                        return -_rotationAngle;
                    } else {
                        return _rotationAngle;
                    }
                }
                return _rotationAngle;
            }
        }

    }
}
