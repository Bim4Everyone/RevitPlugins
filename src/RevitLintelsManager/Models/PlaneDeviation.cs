using System;

using RevitLintelsManager.Models.Configs;

namespace RevitLintelsManager.Models;

/// <summary>Смещение привязки перемычки относительно трёх плоскостей установки проёма.</summary>
internal sealed class PlaneDeviation {
    /// <summary>Измеряет три проекции и три угла с учётом пределов текущей сессии.</summary>
    public PlaneDeviation(AlignmentPlanes target, AlignmentPlanes actual, MatchLimits limits) {
        var delta = new GeometryPoint(actual.Center.X - target.Center.X, actual.Center.Y - target.Center.Y,
            actual.Center.Z - target.Center.Z);
        AlongMm = Dot(delta, target.CenterNormal) * SystemPluginConfig.MillimetersPerFoot;
        AcrossMm = Dot(delta, target.LongitudinalNormal) * SystemPluginConfig.MillimetersPerFoot;
        HeightMm = Dot(delta, target.HeightNormal) * SystemPluginConfig.MillimetersPerFoot;
        CenterAngle = PlaneAngle(target.CenterNormal, actual.CenterNormal);
        LongitudinalAngle = PlaneAngle(target.LongitudinalNormal, actual.LongitudinalNormal);
        HeightAngle = PlaneAngle(target.HeightNormal, actual.HeightNormal);
        Distance = Math.Sqrt(Dot(delta, delta));
        double maximumOffset = Math.Max(Math.Abs(AlongMm), Math.Max(Math.Abs(AcrossMm), Math.Abs(HeightMm)));
        InRange = maximumOffset <= limits.SearchMm + SystemPluginConfig.MatchRoundoffMm;
        double maximumAngle = Math.Max(CenterAngle, Math.Max(LongitudinalAngle, HeightAngle));
        IsShifted = maximumOffset > limits.AlignmentMm + SystemPluginConfig.MatchRoundoffMm
            || maximumAngle > SystemPluginConfig.MatchAngleToleranceDegrees + SystemPluginConfig.MatchRoundoffDegrees;
    }

    public double AlongMm { get; }
    public double AcrossMm { get; }
    public double HeightMm { get; }
    public double CenterAngle { get; }
    public double LongitudinalAngle { get; }
    public double HeightAngle { get; }
    public double Distance { get; }
    public bool InRange { get; }
    public bool IsShifted { get; }

    // Противоположные нормали описывают параллельные плоскости, в том числе после отражения семейства.
    private static double PlaneAngle(GeometryPoint a, GeometryPoint b) {
        double x = a.Y * b.Z - a.Z * b.Y;
        double y = a.Z * b.X - a.X * b.Z;
        double z = a.X * b.Y - a.Y * b.X;
        return Math.Atan2(Math.Sqrt(x * x + y * y + z * z), Math.Abs(Dot(a, b))) * 180 / Math.PI;
    }

    // Знак проекции сохраняет фактическую сторону отклонения.
    private static double Dot(GeometryPoint a, GeometryPoint b) {
        return a.X * b.X + a.Y * b.Y + a.Z * b.Z;
    }
}
