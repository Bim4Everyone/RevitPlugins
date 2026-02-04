using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

namespace RevitDeclarations.Models;
internal class RoomContour {
    private readonly double _curveTolerance;

    public RoomContour(Room room) {
        _curveTolerance = room.Document.Application.ShortCurveTolerance;

        var options = new SpatialElementBoundaryOptions {
            SpatialElementBoundaryLocation = SpatialElementBoundaryLocation.Finish
        };
        IList<IList<BoundarySegment>> roomBoundaries = room
            .GetBoundarySegments(options)
            .ToList();
        var outerBoundary = GetOuterBoundary(roomBoundaries);

        if(outerBoundary.Count > 2) {
            var straightContour = GetStraightContour(outerBoundary);
            var updatedContour = UpdateContourOrder(straightContour);
            ContourCurves = ConnectRoomContour(updatedContour)
                .OrderBy(x => x.Length)
                .ToList();
        } else {
            ContourCurves = [];
        }
    }

    public IReadOnlyList<Curve> ContourCurves { get; }
    public bool NeedToCheck { get; private set; }

    // Метод создает вместо каждой линии контура прямую линию по начальной и конечной точке.
    // Это необходимо для замены арок в контуре.
    // Если имеется арка, то она заменяется на две прямые линии.
    private IList<Curve> GetStraightContour(IList<BoundarySegment> boundaries) {
        var prevCurve = boundaries[boundaries.Count - 1].GetCurve();
        List<Curve> contour = [];

        foreach(var boundary in boundaries) {
            var curve = boundary.GetCurve();
            double distance = prevCurve.GetEndPoint(1).DistanceTo(curve.GetEndPoint(1));

            // evaluateCoef определяет минимальную часть арки при делении на прямые линии.
            // При проверке _curveTolerance необходимо учитывать именно минимальную часть арки.
            double evaluateCoef = 0.5;
            if(distance > _curveTolerance / evaluateCoef) {
                if(curve is Arc) {
                    var midPoint = curve.Evaluate(0.5, true);
                    Curve straightCurveLeft = Line.CreateBound(prevCurve.GetEndPoint(1), midPoint);
                    contour.Add(straightCurveLeft);

                    Curve straightCurveRight = Line.CreateBound(midPoint, curve.GetEndPoint(1));
                    prevCurve = straightCurveRight;
                    contour.Add(straightCurveRight);

                    NeedToCheck = true;
                } else {
                    Curve straightCurve = Line.CreateBound(prevCurve.GetEndPoint(1), curve.GetEndPoint(1));
                    prevCurve = straightCurve;
                    contour.Add(straightCurve);
                }
            } else {
                NeedToCheck = true;
            }
        }
        return contour;
    }

    // Метод для настройки порядка линий в контуре помещения.
    // Первой линией должна быть линия на углу контура.
    private IList<Curve> UpdateContourOrder(IList<Curve> contour) {
        var firstLine = contour.First() as Line;
        var lastLine = contour.Last() as Line;

        var firstLineDirection = firstLine.Direction;
        var lastLineDirection = lastLine.Direction;

        if(firstLineDirection.CrossProduct(lastLineDirection).IsAlmostEqualTo(XYZ.Zero)) {
            IList<Curve> newContour = contour.Skip(1).ToList();
            newContour.Add(contour[0]);

            return UpdateContourOrder(newContour);
        } else {
            return contour;
        }
    }

    // Метод для объединения линий, которые лежат на одной прямой и идут друг за другом.
    private IList<Curve> ConnectRoomContour(IList<Curve> contour) {
        int contourLength = contour.Count();

        if(contourLength < 2) {
            return contour;
        }

        var leftContour = ConnectRoomContour(contour.Take(contourLength / 2).ToList());
        var rightContour = ConnectRoomContour(contour.Skip(contourLength / 2).ToList());

        var endOfTheLeftContour = leftContour.Last() as Line;
        var startOfTheRightContour = rightContour.First() as Line;

        var directionOfLeftContour = endOfTheLeftContour.Direction;
        var directionOfRightContour = startOfTheRightContour.Direction;

        List<Curve> connectedContour = [];
        if(directionOfLeftContour.CrossProduct(directionOfRightContour).IsAlmostEqualTo(XYZ.Zero)) {
            Curve curve = Line.CreateBound(endOfTheLeftContour.GetEndPoint(0), startOfTheRightContour.GetEndPoint(1));
            connectedContour.AddRange(leftContour.Take(leftContour.Count - 1));
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

        return length == 0
            ? []
            : length == 1
                ? boundaries[0]
                : boundaries
                                .OrderBy(x => x.Sum(b => b.GetCurve().Length))
                                .FirstOrDefault();
    }
}
