using Autodesk.Revit.DB;

namespace RevitStairRailing.Models;

/// <summary>
/// Траектория ограждения: цепочка прямых участков (марши и площадки) по ходу подъёма.
/// </summary>
internal class RailingPath {
    /// <summary>
    /// Допуск совпадения концов соседних линий, футы (1 мм).
    /// </summary>
    private const double _joinTolerance = 1 / 304.8;

    /// <summary>
    /// Участок с уклоном больше этого — марш, иначе — площадка.
    /// </summary>
    private const double _flightMinSlope = 0.05;

    private readonly List<bool> _isFlightSegment;

    private RailingPath(List<XYZ> points, List<bool> isFlightSegment) {
        Points = points;
        _isFlightSegment = isFlightSegment;
    }

    /// <summary>
    /// Вершины траектории по ходу подъёма; участок — между соседними вершинами.
    /// </summary>
    public List<XYZ> Points { get; }

    public int SegmentCount => Points.Count - 1;

    /// <summary>
    /// Собирает траекторию из отрезков в одну цепочку и направляет её снизу вверх.
    /// </summary>
    /// <exception cref="RailingException">Линии не образуют одну незамкнутую цепочку или есть вертикальная.</exception>
    public static RailingPath FromLines(IReadOnlyCollection<Line> lines) {
        if(lines.Count == 0) {
            throw new RailingException("Path.NoLines");
        }

        var remaining = lines.ToList();
        Line first = remaining.FirstOrDefault(line => !HasNeighbour(line.GetEndPoint(0), line, remaining))
                     ?? remaining.FirstOrDefault(line => !HasNeighbour(line.GetEndPoint(1), line, remaining));
        if(first == null) {
            throw new RailingException("Path.Closed");
        }

        remaining.Remove(first);
        bool startsFree = !HasNeighbour(first.GetEndPoint(0), first, lines);
        var points = startsFree
            ? new List<XYZ> { first.GetEndPoint(0), first.GetEndPoint(1) }
            : new List<XYZ> { first.GetEndPoint(1), first.GetEndPoint(0) };
        while(remaining.Count > 0) {
            XYZ last = points[points.Count - 1];
            Line next = remaining.FirstOrDefault(line => line.GetEndPoint(0).IsAlmostEqualTo(last, _joinTolerance)
                                                         || line.GetEndPoint(1).IsAlmostEqualTo(last, _joinTolerance));
            if(next == null) {
                throw new RailingException("Path.NotChain");
            }

            remaining.Remove(next);
            points.Add(next.GetEndPoint(0).IsAlmostEqualTo(last, _joinTolerance) ? next.GetEndPoint(1) : next.GetEndPoint(0));
        }

        if(points[points.Count - 1].Z < points[0].Z) {
            points.Reverse();
        }

        var isFlightSegment = new List<bool>();
        for(int index = 1; index < points.Count; index++) {
            double planLength = GetPlanVector(points[index - 1], points[index]).GetLength();
            if(planLength < _joinTolerance) {
                throw new RailingException("Path.Vertical");
            }

            isFlightSegment.Add(Math.Abs(points[index].Z - points[index - 1].Z) / planLength > _flightMinSlope);
        }

        return new RailingPath(points, isFlightSegment);
    }

    /// <summary>
    /// Участок — марш (наклонный); иначе — площадка.
    /// </summary>
    public bool IsFlight(int segmentIndex) {
        return _isFlightSegment[segmentIndex];
    }

    /// <summary>
    /// Траектория, смещённая в плане влево или вправо по ходу подъёма; у каждого участка своё смещение.
    /// В вершинах смещённые участки сходятся в точке пересечения; у параллельных участков с разным смещением
    /// добавляется горизонтальная перемычка.
    /// </summary>
    public RailingPath Offset(IReadOnlyList<double> segmentDistances, RailingSide side) {
        double sideSign = side == RailingSide.Left ? 1 : -1;
        var offsetPoints = new List<XYZ>();
        var isFlightSegment = new List<bool>();
        offsetPoints.Add(Points[0] + GetLeftNormal(0) * (segmentDistances[0] * sideSign));
        for(int segmentIndex = 0; segmentIndex < SegmentCount; segmentIndex++) {
            XYZ vertex = Points[segmentIndex + 1];
            XYZ shift = GetLeftNormal(segmentIndex) * (segmentDistances[segmentIndex] * sideSign);
            isFlightSegment.Add(_isFlightSegment[segmentIndex]);
            if(segmentIndex == SegmentCount - 1) {
                offsetPoints.Add(vertex + shift);
                break;
            }

            XYZ nextShift = GetLeftNormal(segmentIndex + 1) * (segmentDistances[segmentIndex + 1] * sideSign);
            XYZ? corner = IntersectInPlan(
                vertex + shift,
                GetPlanDirection(segmentIndex),
                vertex + nextShift,
                GetPlanDirection(segmentIndex + 1));
            if(corner != null) {
                offsetPoints.Add(new XYZ(corner.X, corner.Y, vertex.Z));
            } else if((shift - nextShift).GetLength() < _joinTolerance) {
                offsetPoints.Add(vertex + shift);
            } else {
                // участки параллельны, смещения разные: перемычка между ними
                offsetPoints.Add(vertex + shift);
                offsetPoints.Add(vertex + nextShift);
                isFlightSegment.Add(false);
            }
        }

        return new RailingPath(offsetPoints, isFlightSegment);
    }

    /// <summary>
    /// Направление участка в плане (единичный горизонтальный вектор).
    /// </summary>
    public XYZ GetPlanDirection(int segmentIndex) {
        return GetPlanVector(Points[segmentIndex], Points[segmentIndex + 1]).Normalize();
    }

    public double GetPlanLength(int segmentIndex) {
        return GetPlanVector(Points[segmentIndex], Points[segmentIndex + 1]).GetLength();
    }

    private XYZ GetLeftNormal(int segmentIndex) {
        XYZ direction = GetPlanDirection(segmentIndex);
        return new XYZ(-direction.Y, direction.X, 0);
    }

    /// <summary>
    /// Точка пересечения двух прямых в плане; null — прямые параллельны.
    /// </summary>
    private static XYZ? IntersectInPlan(XYZ firstPoint, XYZ firstDirection, XYZ secondPoint, XYZ secondDirection) {
        double denominator = firstDirection.X * secondDirection.Y - firstDirection.Y * secondDirection.X;
        if(Math.Abs(denominator) < 1e-6) {
            return null;
        }

        XYZ between = secondPoint - firstPoint;
        double firstParameter = (between.X * secondDirection.Y - between.Y * secondDirection.X) / denominator;
        return firstPoint + firstDirection * firstParameter;
    }

    private static bool HasNeighbour(XYZ point, Line line, IEnumerable<Line> lines) {
        return lines.Any(other => !ReferenceEquals(other, line)
                                  && (other.GetEndPoint(0).IsAlmostEqualTo(point, _joinTolerance)
                                      || other.GetEndPoint(1).IsAlmostEqualTo(point, _joinTolerance)));
    }

    private static XYZ GetPlanVector(XYZ start, XYZ end) {
        return new XYZ(end.X - start.X, end.Y - start.Y, 0);
    }
}
