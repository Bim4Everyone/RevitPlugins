using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitOpeningSlopes.Models {
    internal class SlopesDataGetter {
        private readonly RevitRepository _revitRepository;


        public SlopesDataGetter(RevitRepository revitRepository) {

            _revitRepository = revitRepository;
        }
        //public Line CreateLineFromOffsetPoint(FamilyInstance opening) {
        //    XYZ openingOrigin = _revitRepository.GetOpeningLocation(opening);
        //    XYZ openingVector = _revitRepository.GetOpeningVector(opening);
        //    double offset = _revitRepository.ConvertToFeet(300);
        //    const double frontLineLength = 900;
        //    XYZ frontOffsetPoint = new XYZ(openingOrigin.X, openingOrigin.Y, openingOrigin.Z + offset)
        //        + openingVector * _revitRepository.ConvertToFeet(frontLineLength);
        //    Line lineFromOffsetPoint = _linesFromOpening.CreateLineFromOpening(
        //        frontOffsetPoint, opening, frontLineLength, DirectionEnum.Back);
        //    return lineFromOffsetPoint;
        //}
        //public XYZ GetRightPoint(FamilyInstance opening) {
        //    Line lineFromOffsetPoint = CreateLineFromOffsetPoint(opening);
        //    const double step = 0.032; //~10 мм
        //    const double rightLineLength = 2000;
        //    ICollection<XYZ> points = _linesFromOpening.SplitCurveToPoints(lineFromOffsetPoint, step);
        //    double closestDist = double.PositiveInfinity;
        //    XYZ closestPoint = null;
        //    foreach(XYZ point in points) {
        //        Line rightLine = _linesFromOpening.CreateLineFromOpening(point, opening,
        //            rightLineLength,
        //            DirectionEnum.Right);
        //        Element wall = _nearestElements.GetElementByRay(rightLine);
        //        if(wall != null) {
        //            XYZ intersectCoord = null;
        //            Solid wallSolid = _solidOperations.GetUnitedSolidFromHostElement(wall);
        //            if(wallSolid != null) {
        //                SolidCurveIntersectionOptions intersectOptOutside = new SolidCurveIntersectionOptions() {
        //                    ResultType = SolidCurveIntersectionMode.CurveSegmentsOutside
        //                };
        //                SolidCurveIntersection intersection = wallSolid.IntersectWithCurve(rightLine, intersectOptOutside);
        //                if(intersection.SegmentCount > 0) {
        //                    intersectCoord = intersection.GetCurveSegment(0).GetEndPoint(1);
        //                    double currentDist = intersectCoord.DistanceTo(point);
        //                    if(currentDist < closestDist) {
        //                        closestDist = currentDist;
        //                        closestPoint = intersectCoord;
        //                    }
        //                } else {
        //                    continue;
        //                }
        //            }
        //        }

        //    }
        //    //Line testLine = _linesFromOpening.CreateLineFromOpening(closestPoint, opening, 600, DirectionEnum.Forward);
        //    //_linesFromOpening.CreateTestModelLine(testLine);
        //    return closestPoint;
        //}

        //public XYZ GetRightFrontPoint(FamilyInstance opening) {
        //    XYZ rightPoint = GetRightPoint(opening);
        //    XYZ openingVector = _revitRepository.GetOpeningVector(opening);
        //    const double backLineLength = 800;
        //    const double pointForwardOffset = 400;
        //    XYZ rightPointWithForwardOffset = rightPoint + openingVector
        //        * _revitRepository.ConvertToFeet(pointForwardOffset);
        //    Line backLine = _linesFromOpening.CreateLineFromOpening(
        //        rightPointWithForwardOffset,
        //        opening, backLineLength,
        //        DirectionEnum.Back);
        //    Element wall = _nearestElements.GetElementByRay(backLine);
        //    XYZ intersectCoord = null;
        //    if(wall != null) {
        //        Solid wallSolid = _solidOperations.GetUnitedSolidFromHostElement(wall);
        //        if(wallSolid != null) {
        //            SolidCurveIntersectionOptions intersectOptOutside = new SolidCurveIntersectionOptions() {
        //                ResultType = SolidCurveIntersectionMode.CurveSegmentsOutside
        //            };
        //            SolidCurveIntersection intersection = wallSolid.IntersectWithCurve(backLine, intersectOptOutside);
        //            if(intersection.SegmentCount > 0) {
        //                intersectCoord = intersection.GetCurveSegment(0).GetEndPoint(1);
        //            }
        //        }
        //    }
        //    //Line testLine = _linesFromOpening.CreateLineFromOpening(intersectCoord, opening,
        //    //    600, DirectionEnum.Left);
        //    //_linesFromOpening.CreateTestModelLine(testLine);
        //    return intersectCoord;
        //}
        //public XYZ GetRightDepthPoint(FamilyInstance opening) {
        //    XYZ rightFrontPoint = GetRightFrontPoint(opening);
        //    const double depthLineLength = 1000;
        //    Line depthLine = _linesFromOpening.CreateLineFromOpening(
        //        rightFrontPoint,
        //        opening, depthLineLength,
        //        DirectionEnum.Back);
        //    IEnumerable<Solid> openingSolids = opening.GetSolids();
        //    XYZ intersectCoord = null;
        //    XYZ closestPoint = null;
        //    if(openingSolids.Count() > 0) {
        //        SolidCurveIntersectionOptions intersectOptOutside = new SolidCurveIntersectionOptions() {
        //            ResultType = SolidCurveIntersectionMode.CurveSegmentsOutside
        //        };
        //        double closestDist = double.PositiveInfinity;
        //        foreach(Solid solid in openingSolids) {
        //            SolidCurveIntersection intersection = solid.IntersectWithCurve(depthLine, intersectOptOutside);
        //            if(intersection.SegmentCount > 0) {
        //                intersectCoord = intersection.GetCurveSegment(0).GetEndPoint(1);
        //                double currentDist = _revitRepository
        //                    .ConvertToMillimeters(rightFrontPoint.DistanceTo(intersectCoord));
        //                if(currentDist < closestDist) {
        //                    closestDist = currentDist;
        //                    closestPoint = intersectCoord;
        //                }
        //            }
        //        }
        //    }
        //    //Line testLine = _linesFromOpening.CreateLineFromOpening(closestPoint, opening,
        //    //    600, DirectionEnum.Forward);
        //    //_linesFromOpening.CreateTestModelLine(testLine);
        //    return closestPoint;
        //}
        //public XYZ GetHorizontalCenterPoint(FamilyInstance opening) {
        //    Line forwardOffsetLine = CreateLineFromOffsetPoint(opening);
        //    XYZ origin = forwardOffsetLine.GetEndPoint(0);
        //    XYZ depthPoint = GetRightDepthPoint(opening);
        //    XYZ directionForwardLine = forwardOffsetLine.Direction;
        //    double t;

        //    if(Math.Abs(directionForwardLine.X) > double.Epsilon)
        //        t = (depthPoint.X - origin.X) / directionForwardLine.X;
        //    else
        //        t = (depthPoint.Y - origin.Y) / directionForwardLine.Y; // Используем y-компоненту

        //    // Вычисление координат точки пересечения
        //    double intersectionX = origin.X + t * directionForwardLine.X;
        //    double intersectionY = origin.Y + t * directionForwardLine.Y;
        //    double intersectionZ = origin.Z + t * directionForwardLine.Z;
        //    XYZ intersectCoord = new XYZ(intersectionX, intersectionY, intersectionZ);
        //    //Line testLine = _linesFromOpening.CreateLineFromOpening(intersectCoord, opening,
        //    //    600, DirectionEnum.Forward);
        //    //_linesFromOpening.CreateTestModelLine(testLine);
        //    return intersectCoord;
        //}

        //public XYZ GetVerticalCenterPoint(FamilyInstance opening) {
        //    XYZ intersectCoord = null;
        //    XYZ verticalCenter = null;
        //    XYZ origin = GetHorizontalCenterPoint(opening);
        //    XYZ openingOrigin = _revitRepository.GetOpeningLocation(opening);
        //    Line upwardLine = _linesFromOpening.CreateLineFromOpening(origin, opening, 4000, DirectionEnum.Top);
        //    Element topElement = _nearestElements.GetElementByRay(upwardLine);
        //    //_linesFromOpening.CreateTestModelLine(upwardLine);
        //    if(topElement == null) {
        //        return null;
        //    }
        //    Solid topSolid = _solidOperations.GetUnitedSolidFromHostElement(topElement);
        //    //DirectShape ds = DirectShape.CreateElement(_revitRepository.Document,
        //    //    new ElementId(BuiltInCategory.OST_GenericModel));
        //    //ds.SetShape(new GeometryObject[] { topSolid });
        //    if(topSolid != null) {

        //        SolidCurveIntersectionOptions intersectOptOutside = new SolidCurveIntersectionOptions() {
        //            ResultType = SolidCurveIntersectionMode.CurveSegmentsOutside
        //        };
        //        //SolidCurveIntersectionOptions intersectOptInside = new SolidCurveIntersectionOptions() {
        //        //    ResultType = SolidCurveIntersectionMode.CurveSegmentsInside
        //        //};
        //        SolidCurveIntersection intersection = topSolid.IntersectWithCurve(upwardLine, intersectOptOutside);
        //        if(intersection.SegmentCount > 0) {
        //            //intersection = topSolid.IntersectWithCurve(upwardLine, intersectOptOutside);
        //            intersectCoord = intersection.GetCurveSegment(0).GetEndPoint(1);
        //            double centerZ = (intersectCoord.Z + openingOrigin.Z) / 2;
        //            verticalCenter = new XYZ(intersectCoord.X, intersectCoord.Y, centerZ);
        //        } else {
        //            throw new ArgumentException("Над окном нет элемента, пересекающегося с окном");
        //        }
        //    }
        //    //Line testLine = _linesFromOpening.CreateLineFromOpening(verticalCenter, opening,
        //    //    600, DirectionEnum.Forward);
        //    //_linesFromOpening.CreateTestModelLine(testLine);
        //    return verticalCenter;
        //}
        //public double GetOpeningHeight(FamilyInstance opening) {
        //    XYZ verticalCenterPoint = GetVerticalCenterPoint(opening);
        //    XYZ openingOrigin = _revitRepository.GetOpeningLocation(opening);
        //    double openingHeight = (verticalCenterPoint.Z - openingOrigin.Z) * 2;
        //    return openingHeight;
        //}
        //public double GetOpeningWidth(FamilyInstance opening) {
        //    XYZ verticalCenterPoint = GetVerticalCenterPoint(opening);
        //    XYZ rightPoint = GetRightDepthPoint(opening);
        //    double openingWidth = Math.Sqrt(Math.Pow(verticalCenterPoint.X - rightPoint.X, 2)
        //        + Math.Pow(verticalCenterPoint.Y - rightPoint.Y, 2)) * 2;
        //    return openingWidth;
        //}
        //public double GetOpeningDepth(FamilyInstance opening) {
        //    XYZ rightDepthPoint = GetRightDepthPoint(opening);
        //    XYZ rightFrontPoint = GetRightFrontPoint(opening);
        //    return rightDepthPoint.DistanceTo(rightFrontPoint);
        //}
        public IList<SlopeCreationData> GetSlopeCreationData(PluginConfig config) {
            ICollection<FamilyInstance> openings = _revitRepository
                    .GetWindows(config.WindowsGetterMode);

            List<SlopeCreationData> slopeCreationData = new List<SlopeCreationData>();
            SlopeCreationData slopeData = null;
            foreach(FamilyInstance opening in openings) {
                //XYZ topXYZ = _openingTopXYZGetter.GetOpeningTopXYZ(opening);
                //if(topXYZ == null) {
                //    continue;
                //}
                OpeningHandler openingParameters = new OpeningHandler(_revitRepository, opening);
                //double height = GetOpeningHeight(opening);
                double height = openingParameters.OpeningHeight;
                if(height <= 0) {
                    continue;
                }

                //XYZ g = GetVerticalCenterPoint(opening);
                //GetDepthPoint(opening);
                //double width = GetOpeningWidth(opening);
                double width = openingParameters.OpeningWidth;
                if(width <= 0) {
                    continue;
                }
                //double depth = GetOpeningDepth(opening);
                //XYZ center = GetVerticalCenterPoint(opening);
                double depth = openingParameters.OpeningDepth;
                XYZ center = openingParameters.OpeningCenterPoint;
                //XYZ center = _openingCenterXYZGetter.GetOpeningCenter(opening);
                //XYZ frontPoint = _openingFrontPointGetter.GetFrontPoint(opening);
                slopeData = new SlopeCreationData(_revitRepository.Document) {
                    Height = height,
                    Width = width,
                    Depth = depth,
                    Center = center,
                    SlopeTypeId = config.SlopeTypeId
                };
                slopeCreationData.Add(slopeData);
            }
            return slopeCreationData;
        }
    }
}

