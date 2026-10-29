using Autodesk.Revit.DB;

namespace RevitStairRailing.Models;

/// <summary>
/// Раскладка элементов ограждения по траектории: линии размещения стоек, поручня, прутков и пластин, точки анкеров.
/// </summary>
/// <remarks>
/// Только геометрия, без обращения к модели. Траектория — ось ограждения по поверхности марша и площадки.
/// Поворот сечения вертикальных элементов в Revit отсчитывается от оси X проекта
/// (угол 0 — ширина сечения вдоль +X, 90° — вдоль −Y), поэтому он пересчитывается от направления участка.
/// </remarks>
internal static class RailingLayout {
    private const double MillimetersToFeet = 1 / 304.8;

    /// <summary>
    /// Участок с уклоном больше этого — марш, иначе — площадка.
    /// </summary>
    private const double FlightMinSlope = 0.05;

    public static List<RailingMember> Create(RailingPath path, RailingParameters parameters) {
        double postHeight = parameters.PostHeight * MillimetersToFeet;
        double maxPostStep = parameters.MaxPostStep * MillimetersToFeet;
        var members = new List<RailingMember>();
        for(int segmentIndex = 0; segmentIndex < path.SegmentCount; segmentIndex++) {
            XYZ start = path.Points[segmentIndex];
            XYZ end = path.Points[segmentIndex + 1];
            double planLength = path.GetPlanLength(segmentIndex);
            var post = new PostPlacement(
                path.GetPlanDirection(segmentIndex),
                parameters.Side,
                Math.Abs(end.Z - start.Z) / planLength > FlightMinSlope ? parameters.FlightMount : parameters.LandingMount);

            members.Add(RailingMember.Linear(RailingMemberRole.Handrail, Raise(start, end, postHeight)));
            foreach(double rodOffset in parameters.RodOffsets) {
                members.Add(RailingMember.Linear(
                    RailingMemberRole.Rod,
                    Raise(start, end, postHeight - rodOffset * MillimetersToFeet)));
            }

            // стойки равномерно, с шагом в плане не больше заданного; общая вершина участков — одна стойка
            int spanCount = Math.Max(1, (int) Math.Ceiling(planLength / maxPostStep - 1e-6));
            for(int postNumber = segmentIndex == 0 ? 0 : 1; postNumber <= spanCount; postNumber++) {
                XYZ postBottom = start + (end - start) * ((double) postNumber / spanCount);
                AddPost(members, postBottom, post, postHeight, parameters);
            }
        }

        return members;
    }

    private static void AddPost(
        List<RailingMember> members,
        XYZ trajectoryPoint,
        PostPlacement post,
        double postHeight,
        RailingParameters parameters) {

        XYZ postTop = trajectoryPoint + XYZ.BasisZ * postHeight;
        double postBendAngle = parameters.PostRotationDegrees * Math.PI / 180 + GetBendAngle(post.Direction);
        if(post.Mount == PostMount.Top) {
            // пластина лежит на поверхности, стойка стоит на пластине
            double plateThickness = parameters.BasePlate.Thickness * MillimetersToFeet;
            XYZ plateTop = trajectoryPoint + XYZ.BasisZ * plateThickness;
            XYZ halfLength = post.Direction * (parameters.BasePlateLength * MillimetersToFeet / 2);
            XYZ plateAxis = trajectoryPoint + XYZ.BasisZ * (plateThickness / 2);
            members.Add(RailingMember.Linear(
                RailingMemberRole.BasePlate,
                Line.CreateBound(plateAxis - halfLength, plateAxis + halfLength),
                Math.PI / 2,
                parameters.BasePlate));
            members.Add(RailingMember.Linear(RailingMemberRole.Post, Line.CreateBound(plateTop, postTop), postBendAngle));
            if(parameters.AnchorTypeId != null) {
                foreach(XYZ anchorPoint in GetAnchorPoints(plateTop, post, parameters)) {
                    members.Add(RailingMember.AtPoint(RailingMemberRole.Anchor, anchorPoint));
                }
            }

            return;
        }

        // щёки по обе стороны стойки от поверхности вверх, ширина сечения — в плоскости щеки
        bool isAlongRailing = parameters.CheekOrientation == CheekOrientation.AlongRailing;
        XYZ cheekPlaneDirection = isAlongRailing ? post.Direction : post.StairNormal;
        XYZ cheekShiftDirection = isAlongRailing ? post.StairNormal : post.Direction;
        double cheekShift = (parameters.CheekGap + parameters.Cheek.Thickness) / 2 * MillimetersToFeet;
        double cheekHeight = parameters.CheekHeight * MillimetersToFeet;
        foreach(double sign in new[] { -1.0, 1.0 }) {
            XYZ cheekBottom = trajectoryPoint + cheekShiftDirection * (cheekShift * sign);
            members.Add(RailingMember.Linear(
                RailingMemberRole.Cheek,
                Line.CreateBound(cheekBottom, cheekBottom + XYZ.BasisZ * cheekHeight),
                GetBendAngle(cheekPlaneDirection),
                parameters.Cheek));
        }

        members.Add(RailingMember.Linear(RailingMemberRole.Post, Line.CreateBound(trajectoryPoint, postTop), postBendAngle));
    }

    /// <summary>
    /// Точки анкеров на верхней грани опорной пластины.
    /// </summary>
    private static IEnumerable<XYZ> GetAnchorPoints(XYZ plateTopCenter, PostPlacement post, RailingParameters parameters) {
        double alongOffset = (parameters.BasePlateLength / 2 - parameters.AnchorEdgeDistance) * MillimetersToFeet;
        double acrossOffset = (parameters.BasePlate.Width / 2 - parameters.AnchorEdgeDistance) * MillimetersToFeet;
        XYZ Point(double along, double across) {
            return plateTopCenter + post.Direction * along + post.StairNormal * across;
        }

        switch(parameters.AnchorLayout) {
            case AnchorLayout.Diagonal:
                yield return Point(-alongOffset, -acrossOffset);
                yield return Point(alongOffset, acrossOffset);
                break;
            case AnchorLayout.Triangle:
                yield return Point(-alongOffset, acrossOffset);
                yield return Point(alongOffset, acrossOffset);
                yield return Point(0, -acrossOffset);
                break;
            default:
                yield return Point(-alongOffset, -acrossOffset);
                yield return Point(alongOffset, -acrossOffset);
                yield return Point(-alongOffset, acrossOffset);
                yield return Point(alongOffset, acrossOffset);
                break;
        }
    }

    /// <summary>
    /// Угол поворота сечения вертикального элемента, при котором ширина сечения идёт вдоль <paramref name="planDirection"/>.
    /// </summary>
    private static double GetBendAngle(XYZ planDirection) {
        return Math.Atan2(-planDirection.Y, planDirection.X);
    }

    private static Line Raise(XYZ start, XYZ end, double height) {
        XYZ shift = XYZ.BasisZ * height;
        return Line.CreateBound(start + shift, end + shift);
    }

    /// <summary>
    /// Стойки участка: направление в плане, сторона лестницы и способ крепления.
    /// </summary>
    private class PostPlacement {
        public PostPlacement(XYZ direction, RailingSide side, PostMount mount) {
            Direction = direction;
            var leftNormal = new XYZ(-direction.Y, direction.X, 0);
            StairNormal = side == RailingSide.Left ? leftNormal : -leftNormal;
            Mount = mount;
        }

        public XYZ Direction { get; }

        /// <summary>
        /// Горизонталь поперёк ограждения в сторону, куда оно смещено от линий траектории.
        /// </summary>
        public XYZ StairNormal { get; }

        public PostMount Mount { get; }
    }
}

/// <summary>
/// Элемент ограждения: по линии (стойки, поручень, прутки, пластины) или в точке (анкеры).
/// </summary>
internal class RailingMember {
    private RailingMember(RailingMemberRole role) {
        Role = role;
    }

    public RailingMemberRole Role { get; }

    /// <summary>
    /// Линия размещения; null — элемент ставится в точке.
    /// </summary>
    public Line Line { get; private set; }

    public XYZ Point { get; private set; }

    /// <summary>
    /// Угол поворота сечения, радианы; null — не задаётся.
    /// </summary>
    public double? BendAngle { get; private set; }

    /// <summary>
    /// Размер пластины; задан — сечение по центру линии, типоразмер находится или создаётся по размеру.
    /// </summary>
    public PlateSize PlateSize { get; private set; }

    public static RailingMember Linear(
        RailingMemberRole role,
        Line line,
        double? bendAngle = null,
        PlateSize plateSize = null) {

        return new RailingMember(role) { Line = line, BendAngle = bendAngle, PlateSize = plateSize };
    }

    public static RailingMember AtPoint(RailingMemberRole role, XYZ point) {
        return new RailingMember(role) { Point = point };
    }
}

internal enum RailingMemberRole {
    Post,
    Handrail,
    Rod,
    BasePlate,
    Cheek,
    Anchor
}
