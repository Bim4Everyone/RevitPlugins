using Autodesk.Revit.DB;

namespace RevitStairRailing.Models;

/// <summary>
/// Траектория ограждения: цепочка прямых участков (марши и площадки) по ходу подъёма.
/// </summary>
internal class RailingPath {
    /// <summary>
    /// Допуск совпадения концов соседних линий, футы (1 мм).
    /// </summary>
    private const double JoinTolerance = 1 / 304.8;

    /// <summary>
    /// Наибольшее удлинение смещения во внутреннем углу (острые углы не дают «выстрел» в сторону).
    /// </summary>
    private const double MaxMiterRatio = 4;

    private RailingPath(List<XYZ> points) {
        Points = points;
    }

    /// <summary>
    /// Вершины траектории по ходу подъёма; участок — между соседними вершинами.
    /// </summary>
    public List<XYZ> Points { get; }

    public int SegmentCount => Points.Count - 1;

    /// <summary>
    /// Собирает траекторию из отрезков в одну цепочку и направляет её снизу вверх.
    /// </summary>
    /// <exception cref="RailingException">Линии не отрезки или не образуют одну незамкнутую цепочку.</exception>
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
            Line next = remaining.FirstOrDefault(line => line.GetEndPoint(0).IsAlmostEqualTo(last, JoinTolerance)
                                                         || line.GetEndPoint(1).IsAlmostEqualTo(last, JoinTolerance));
            if(next == null) {
                throw new RailingException("Path.NotChain");
            }

            remaining.Remove(next);
            points.Add(next.GetEndPoint(0).IsAlmostEqualTo(last, JoinTolerance) ? next.GetEndPoint(1) : next.GetEndPoint(0));
        }

        if(points[points.Count - 1].Z < points[0].Z) {
            points.Reverse();
        }

        for(int index = 1; index < points.Count; index++) {
            if(GetPlanVector(points[index - 1], points[index]).GetLength() < JoinTolerance) {
                throw new RailingException("Path.Vertical");
            }
        }

        return new RailingPath(points);
    }

    /// <summary>
    /// Траектория, смещённая в плане на <paramref name="distance"/> влево или вправо по ходу подъёма.
    /// Во внутренних вершинах смещение идёт по биссектрисе, чтобы участки сходились в одной точке.
    /// </summary>
    public RailingPath Offset(double distance, RailingSide side) {
        if(distance == 0) {
            return new RailingPath(Points.ToList());
        }

        double signedDistance = side == RailingSide.Left ? distance : -distance;
        var offsetPoints = new List<XYZ>();
        for(int index = 0; index < Points.Count; index++) {
            XYZ previousNormal = index > 0 ? GetLeftNormal(Points[index - 1], Points[index]) : null;
            XYZ nextNormal = index < Points.Count - 1 ? GetLeftNormal(Points[index], Points[index + 1]) : null;
            XYZ shift;
            if(previousNormal == null || nextNormal == null) {
                shift = (previousNormal ?? nextNormal) * signedDistance;
            } else {
                XYZ bisector = previousNormal + nextNormal;
                if(bisector.GetLength() < 1e-9) {
                    // разворот на 180°: участки параллельны, смещаем по нормали предыдущего
                    shift = previousNormal * signedDistance;
                } else {
                    bisector = bisector.Normalize();
                    double cosine = Math.Max(bisector.DotProduct(previousNormal), 1 / MaxMiterRatio);
                    shift = bisector * (signedDistance / cosine);
                }
            }

            offsetPoints.Add(Points[index] + shift);
        }

        return new RailingPath(offsetPoints);
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

    private static bool HasNeighbour(XYZ point, Line line, IEnumerable<Line> lines) {
        return lines.Any(other => !ReferenceEquals(other, line)
                                  && (other.GetEndPoint(0).IsAlmostEqualTo(point, JoinTolerance)
                                      || other.GetEndPoint(1).IsAlmostEqualTo(point, JoinTolerance)));
    }

    private static XYZ GetPlanVector(XYZ start, XYZ end) {
        return new XYZ(end.X - start.X, end.Y - start.Y, 0);
    }

    private static XYZ GetLeftNormal(XYZ start, XYZ end) {
        XYZ direction = GetPlanVector(start, end).Normalize();
        return new XYZ(-direction.Y, direction.X, 0);
    }
}
