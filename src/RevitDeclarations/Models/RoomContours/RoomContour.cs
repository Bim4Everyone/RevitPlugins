using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

namespace RevitDeclarations.Models {
    internal class RoomContour {
        private readonly IReadOnlyList<Curve> _mainContour;
        private readonly double _curveTolerance;
        private bool _needToCheck;

        public RoomContour(Room room) {
            _curveTolerance = room.Document.Application.ShortCurveTolerance;

            SpatialElementBoundaryOptions options = new SpatialElementBoundaryOptions();
            IList<IList<BoundarySegment>> roomBoundaries = room
                .GetBoundarySegments(options)
                .ToList();
            IList<BoundarySegment> outerBoundary = GetOuterBoundary(roomBoundaries);

            if(outerBoundary.Count > 2) {
                IList<Curve> straightContour = GetStraightContour(outerBoundary);
                IList<Curve> updatedContour = UpdateContourOrder(straightContour);
                _mainContour = ConnectRoomContour(updatedContour)
                    .OrderBy(x => x.Length)
                    .ToList();
            } else {
                _mainContour = new List<Curve>();
            }
        }

        public IReadOnlyList<Curve> ContourCurves => _mainContour;
        public bool NeedToCheck => _needToCheck;

        private IList<Curve> GetStraightContour(IList<BoundarySegment> boundaries) {
            Curve prevCurve = boundaries[boundaries.Count - 1].GetCurve();
            List<Curve> contour = new List<Curve>();

            foreach(var boundary in boundaries) {
                Curve curve = boundary.GetCurve();
                double distance = prevCurve.GetEndPoint(1).DistanceTo(curve.GetEndPoint(1));

                if(distance > _curveTolerance) {
                    Curve straightCurve = Line.CreateBound(prevCurve.GetEndPoint(1), curve.GetEndPoint(1));
                    prevCurve = straightCurve;
                    contour.Add(straightCurve);
                } else {
                    _needToCheck = true;
                }
            }
            return contour;
        }

        // Метод для настройки порядка линий в контуре помещения.
        // Первой линией должна быть линия на углу контура.
        private IList<Curve> UpdateContourOrder(IList<Curve> contour) {
            Line firstLine = contour.First() as Line;
            Line lastLine = contour.Last() as Line;

            XYZ firstLineDirection = firstLine.Direction;
            XYZ lastLineDirection = lastLine.Direction;

            if(firstLineDirection.CrossProduct(lastLineDirection).IsAlmostEqualTo(XYZ.Zero)) {
                IList<Curve> newContour = contour.Skip(1).ToList();
                newContour.Add(contour[0]);

                return UpdateContourOrder(newContour);
            } else {
                return contour;
            }
        }

        private IList<Curve> ConnectRoomContour(IList<Curve> contour) {
            int contourLength = contour.Count();

            if(contourLength < 2) {
                return contour;
            }

            IList<Curve> leftContour = ConnectRoomContour(contour.Take(contourLength / 2).ToList());
            IList<Curve> rightContour = ConnectRoomContour(contour.Skip(contourLength / 2).ToList());

            Line endOfTheLeftContour = leftContour.Last() as Line;
            Line startOfTheRightContour = rightContour.First() as Line;

            XYZ directionOfLeftContour = endOfTheLeftContour.Direction;
            XYZ directionOfRightContour = startOfTheRightContour.Direction;

            List<Curve> connectedContour = new List<Curve>();
            if(directionOfLeftContour.CrossProduct(directionOfRightContour).IsAlmostEqualTo(XYZ.Zero)) {
                Curve curve = Line.CreateBound(endOfTheLeftContour.GetEndPoint(1), startOfTheRightContour.GetEndPoint(1));
                connectedContour.AddRange(leftContour.Take(leftContour.Count - 2));
                connectedContour.Add(curve);
                connectedContour.AddRange(rightContour.Skip(1));
            } else {
                connectedContour.AddRange(leftContour);
                connectedContour.AddRange(rightContour);
            }

            return connectedContour;
        }

        private IList<BoundarySegment> GetOuterBoundary(IList<IList<BoundarySegment>> boundaries) {
            int length = boundaries.Count;

            if(length == 0) {
                return new List<BoundarySegment>();
            } else if(length == 1) {
                return boundaries[0];
            } else {
                return boundaries
                    .OrderBy(x => x.Sum(b => b.GetCurve().Length))
                    .FirstOrDefault();
            }
        }
    }
}
