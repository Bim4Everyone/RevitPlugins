using System;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit.Geometry;

using RevitOpeningSlopes.Models.Enums;

namespace RevitOpeningSlopes.Models {
    internal class Opening {
        private readonly RevitRepository _revitRepository;
        private readonly LinesFromOpening _linesFromOpening;
        private readonly SolidOperations _solidOperations;
        private readonly NearestElements _nearestElements;
        private readonly FamilyInstance _opening;
        private readonly double _openingHeight;
        private readonly double _openingWidth;
        private readonly XYZ _openingCenter;
        public Opening(RevitRepository revitRepository, FamilyInstance opening) {
            _opening = opening;
            _revitRepository = revitRepository;
            _linesFromOpening = new LinesFromOpening(revitRepository);
            _solidOperations = new SolidOperations(revitRepository);
            _nearestElements = new NearestElements(revitRepository);
            _openingHeight = GetOpeningHeight();
            _openingCenter = GetOpeningCenter();
            _openingWidth = GetOpeningWidth();
        }
        public double Height { get => _openingHeight; }
        public XYZ Center { get => _openingCenter; }
        public double Width { get => _openingWidth; }
        public XYZ GetOpeningCenter() {
            XYZ origin = _revitRepository.GetOpeningLocation(_opening);
            double height = _openingHeight;
            return new XYZ(origin.X, origin.Y, origin.Z + height / 2);
        }
        public double GetOpeningHeight() {
            XYZ intersectCoord = null;
            const double offsetZ = 200;
            XYZ origin = _revitRepository.GetOpeningLocation(_opening);
            Line upwardLine = _linesFromOpening.CreateLineFromOpening(_opening, DirectionEnum.Top,
                _revitRepository.ConvertToFeet(offsetZ));
            Element topElement = _nearestElements.GetElementByRay(upwardLine);
            //_linesFromOpening.CreateTestModelLine(upwardLine);
            if(topElement == null) {
                return 0;
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
                return origin.DistanceTo(intersectCoord);
            } else {
                return 0;
            }

        }
        public double GetOpeningWidth() {
            XYZ intersectCoord = null;
            XYZ origin = _openingCenter;
            Line rightLine = _linesFromOpening.CreateLineFromOpening(_opening, DirectionEnum.Right, _openingHeight / 2);
            Element rightElement = _nearestElements.GetElementByRay(rightLine);
            //_linesFromOpening.CreateTestModelLine(rightLine);
            if(rightElement == null) {
                return 0;
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
                return origin.DistanceTo(intersectCoord) * 2;
            } else {
                return 0;
            }
        }

    }
}
