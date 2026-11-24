using System;
using System.Collections.Generic;
using System.Linq;

using RevitLintelsManager.Models.Configs;

namespace RevitLintelsManager.Models;

/// <summary>Координаты в футах Revit. Оси хранятся единичными векторами.</summary>
internal readonly struct GeometryPoint(double x, double y, double z) {
    public double X { get; } = x;
    public double Y { get; } = y;
    public double Z { get; } = z;
}

/// <summary>Мировые границы для предварительного отбора и выбора конечного предела поиска.</summary>
internal sealed class GeometryBounds(GeometryPoint min, GeometryPoint max) {
    public GeometryPoint Min { get; } = min;
    public GeometryPoint Max { get; } = max;

    /// <summary>Проверяет наложение проекции отрезка на три координатные оси рамки.</summary>
    public bool Overlaps(GeometryPoint a, GeometryPoint b, double epsilon) {
        return Min.X <= Math.Max(a.X, b.X) + epsilon && Max.X >= Math.Min(a.X, b.X) - epsilon
            && Min.Y <= Math.Max(a.Y, b.Y) + epsilon && Max.Y >= Math.Min(a.Y, b.Y) - epsilon
            && Min.Z <= Math.Max(a.Z, b.Z) + epsilon && Max.Z >= Math.Min(a.Z, b.Z) - epsilon;
    }
}

/// <summary>Правая система прямой вертикальной стены с началом посередине между боковыми плоскостями.</summary>
internal sealed class WallFrame(GeometryPoint origin, double xCos, double xSin, double thickness) {
    public GeometryPoint Origin { get; } = origin;
    public double Thickness { get; } = thickness;
    public GeometryPoint Along { get; } = new(xCos, xSin, 0);
    public GeometryPoint Normal { get; } = new(-xSin, xCos, 0);

    /// <summary>Проецирует смещение от начала на продольную ось стены, её нормаль и вертикаль.</summary>
    public GeometryPoint ToLocal(GeometryPoint point) {
        double dx = point.X - Origin.X;
        double dy = point.Y - Origin.Y;
        return new GeometryPoint(dx * Along.X + dy * Along.Y, dx * Normal.X + dy * Normal.Y, point.Z - Origin.Z);
    }

    /// <summary>Возвращает точку модели по координатам в системе стены.</summary>
    public GeometryPoint ToWorld(double x, double y, double z) {
        return new GeometryPoint(Origin.X + Along.X * x + Normal.X * y, Origin.Y + Along.Y * x + Normal.Y * y,
            Origin.Z + z);
    }
}

/// <summary>Результат сравнения числовой высоты с измерениями одного выреза.</summary>
internal enum CutHeightMatch { Unavailable, Neither, CutHeight, HeadAboveLevel, Both }

/// <summary>Положение начала семейства и фактические направления с уже применёнными отражениями.</summary>
internal sealed class InstancePlacement(GeometryPoint origin, GeometryPoint hand, GeometryPoint facing,
    bool mirrored, bool handFlipped, bool facingFlipped) {
    public GeometryPoint Origin { get; } = origin;
    public GeometryPoint Hand { get; } = hand;
    public GeometryPoint Facing { get; } = facing;
    public bool Mirrored { get; } = mirrored;
    public bool HandFlipped { get; } = handFlipped;
    public bool FacingFlipped { get; } = facingFlipped;
}

/// <summary>Прямоугольный вырез хоста в футах и числовая сверка сопоставленных параметров.</summary>
internal sealed class OpeningCut(GeometryPoint center, AlignmentPlanes targetPlanes, double width, double height,
    bool? widthMatches, CutHeightMatch heightMatch) {
    public GeometryPoint Center { get; } = center;
    public AlignmentPlanes TargetPlanes { get; } = targetPlanes;
    public GeometryPoint TopCenter => TargetPlanes.Center;
    public double Width { get; } = width;
    public double Height { get; } = height;
    public bool? WidthMatches { get; } = widthMatches;
    public CutHeightMatch HeightMatch { get; } = heightMatch;
}

/// <summary>Три плоскости установки заданы общей точкой и единичными нормалями в координатах модели.</summary>
internal sealed class AlignmentPlanes(GeometryPoint center, GeometryPoint centerNormal,
    GeometryPoint longitudinalNormal, GeometryPoint heightNormal) {
    public GeometryPoint Center { get; } = center;

    /// <summary>Нормаль поперечной плоскости через середину ширины, положительное направление оси X.</summary>
    public GeometryPoint CenterNormal { get; } = centerNormal;

    /// <summary>Нормаль продольной плоскости через середину толщины стены.</summary>
    public GeometryPoint LongitudinalNormal { get; } = longitudinalNormal;

    /// <summary>Нормаль нижней опорной плоскости семейства, устанавливаемой на верх выреза.</summary>
    public GeometryPoint HeightNormal { get; } = heightNormal;
}

/// <summary>Факты экземпляра и техническая причина недоступности привязки для последующего анализа.</summary>
internal sealed class GeometryFacts(InstancePlacement placement, WallFrame wall, OpeningCut cut,
    AlignmentPlanes anchor, string limitation) {
    public InstancePlacement Placement { get; } = placement;
    public WallFrame Wall { get; } = wall;
    public OpeningCut Cut { get; } = cut;
    public AlignmentPlanes Anchor { get; } = anchor;
    public string Limitation { get; } = limitation;
}

/// <summary>Дистанции от начала направленного конечного отрезка во внутренних единицах Revit.</summary>
internal readonly struct MaterialInterval(double start, double end) {
    public double Start { get; } = start;
    public double End { get; } = end;

    /// <summary>Объединяет только перекрывающиеся или соприкасающиеся интервалы без округления зазоров.</summary>
    public static IReadOnlyList<MaterialInterval> Union(IEnumerable<MaterialInterval> intervals) {
        var result = new List<MaterialInterval>();
        foreach(var next in intervals.OrderBy(item => item.Start)) {
            int last = result.Count - 1;
            if(last < 0 || next.Start > result[last].End) {
                result.Add(next);
            } else {
                result[last] = new MaterialInterval(result[last].Start, Math.Max(next.End, result[last].End));
            }
        }
        return result.AsReadOnly();
    }
}

/// <summary>Непрерывный начальный участок вдоль заданного направления.</summary>
internal class ContinuousMaterial {
    /// <summary>Проверяет непрерывный начальный участок упорядоченных интервалов материала стены.</summary>
    public ContinuousMaterial(IReadOnlyList<MaterialInterval> intervals, double minimumMm, double roundoffFeet,
        string limitation = null) {
        MinimumMm = minimumMm;
        Limitation = limitation;
        Intervals = intervals;
        if(intervals is null) {
            return;
        }
        double length = 0;
        foreach(var interval in intervals) {
            if(interval.Start > length + roundoffFeet) {
                break;
            }
            length = Math.Max(length, interval.End);
        }
        LengthMm = length * SystemPluginConfig.MillimetersPerFoot;
        HasMaterial = length > 0;
        MeetsMinimum = HasMaterial.Value
            && (length + roundoffFeet) * SystemPluginConfig.MillimetersPerFoot >= minimumMm;
    }

    public double MinimumMm { get; }
    public double? LengthMm { get; }
    public bool? HasMaterial { get; }
    public bool? MeetsMinimum { get; }
    public IReadOnlyList<MaterialInterval> Intervals { get; }
    public string Limitation { get; }
}
