using System.Collections.Generic;
using System.Linq;

namespace RevitLintelsManager.Models;

/// <summary>Данные одного чтения диапазона для последующей записи без хранения объектов Revit.</summary>
internal sealed class ProcessingReadData {
    internal LintelFamilyFacts LintelFamily { get; set; }
    internal Dictionary<string, (string Name, double Offset)> OpeningPlanes { get; } = [];
    internal ILookup<long, string> FloorPlans { get; set; }
    internal LintelAnnotationIndex Annotations { get; set; }
    internal IReadOnlyList<EndWallSnapshot> EndWalls { get; set; } = [];
    internal IReadOnlyList<ManagerOpeningSnapshot> MatchOpenings { get; set; }
    internal IReadOnlyList<ManagerLintelSnapshot> MatchLintels { get; set; }
    internal MatchColumns<ManagerOpeningSnapshot> OpeningCenters { get; set; }
    internal Dictionary<string, GeometryBounds> MatchHostBounds { get; } = [];
}

/// <summary>Имена опорных плоскостей и координата плоскости ширины в определении семейства.</summary>
internal readonly struct FamilyOriginFacts((string X, string Y, string Z)? full, (string Name, double Offset)? width) {
    public (string X, string Y, string Z)? Full { get; } = full;
    public (string Name, double Offset)? Width { get; } = width;
}

/// <summary>Границы стены для выбора соседей после изменения фактической ширины перемычки.</summary>
internal sealed class EndWallSnapshot(string uniqueId, GeometryBounds bounds, bool concrete) {
    internal string UniqueId { get; } = uniqueId;
    internal GeometryBounds Bounds { get; } = bounds;
    internal bool Concrete { get; } = concrete;
}
