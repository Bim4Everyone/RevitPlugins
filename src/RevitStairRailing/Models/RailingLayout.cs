using Autodesk.Revit.DB;

namespace RevitStairRailing.Models;

/// <summary>
/// Раскладка элементов ограждения по траектории: линии размещения стоек, поручня, прутков, пластин и анкеров.
/// </summary>
/// <remarks>
/// Только геометрия, без обращения к модели. Траектория — ось ограждения на уровне низа стоек.
/// </remarks>
internal static class RailingLayout {
    private const double MillimetersToFeet = 1 / 304.8;

    public static List<RailingMember> Create(RailingPath path, RailingParameters parameters) {
        double postHeight = parameters.PostHeight * MillimetersToFeet;
        double maxPostStep = parameters.MaxPostStep * MillimetersToFeet;
        var members = new List<RailingMember>();
        for(int segmentIndex = 0; segmentIndex < path.SegmentCount; segmentIndex++) {
            XYZ start = path.Points[segmentIndex];
            XYZ end = path.Points[segmentIndex + 1];
            XYZ planDirection = path.GetPlanDirection(segmentIndex);

            members.Add(new RailingMember(RailingMemberRole.Handrail, Raise(start, end, postHeight)));
            foreach(double rodOffset in parameters.RodOffsets) {
                members.Add(new RailingMember(
                    RailingMemberRole.Rod,
                    Raise(start, end, postHeight - rodOffset * MillimetersToFeet)));
            }

            // стойки равномерно, с шагом в плане не больше заданного; общая вершина участков — одна стойка
            int spanCount = Math.Max(1, (int) Math.Ceiling(path.GetPlanLength(segmentIndex) / maxPostStep - 1e-6));
            for(int postNumber = segmentIndex == 0 ? 0 : 1; postNumber <= spanCount; postNumber++) {
                XYZ postBottom = start + (end - start) * ((double) postNumber / spanCount);
                AddPost(members, postBottom, planDirection, postHeight, parameters);
            }
        }

        return members;
    }

    private static void AddPost(
        List<RailingMember> members,
        XYZ postBottom,
        XYZ planDirection,
        double postHeight,
        RailingParameters parameters) {

        members.Add(new RailingMember(
            RailingMemberRole.Post,
            Line.CreateBound(postBottom, postBottom + XYZ.BasisZ * postHeight)));

        if(parameters.PlateTypeId != null) {
            XYZ halfPlate = planDirection * (parameters.PlateLength * MillimetersToFeet / 2);
            members.Add(new RailingMember(
                RailingMemberRole.Plate,
                Line.CreateBound(postBottom - halfPlate, postBottom + halfPlate)));
        }

        if(parameters.AnchorTypeId != null) {
            members.Add(new RailingMember(
                RailingMemberRole.Anchor,
                Line.CreateBound(postBottom, postBottom - XYZ.BasisZ * (parameters.AnchorLength * MillimetersToFeet))));
        }
    }

    private static Line Raise(XYZ start, XYZ end, double height) {
        XYZ shift = XYZ.BasisZ * height;
        return Line.CreateBound(start + shift, end + shift);
    }
}

/// <summary>
/// Элемент ограждения: роль и линия размещения семейства.
/// </summary>
internal class RailingMember {
    public RailingMember(RailingMemberRole role, Line line) {
        Role = role;
        Line = line;
    }

    public RailingMemberRole Role { get; }

    public Line Line { get; }
}

internal enum RailingMemberRole {
    Post,
    Handrail,
    Rod,
    Plate,
    Anchor
}
