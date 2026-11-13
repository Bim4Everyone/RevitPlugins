using System.Collections.Generic;

namespace RevitDocumenter.Models.RoomDimensions.Geometry;

/// <summary>
/// Копия доступной геометрии двери без объектов Revit API, пригодная для журнала и моделирования.
/// </summary>
internal class DoorGeometryData {
    public string Source { get; set; }
    public int CurveLimit { get; set; }
    public int PointsPerCurveLimit { get; set; }
    public bool Truncated { get; set; }
    public List<DoorSymbolCurve> Curves { get; set; } = [];
    public List<string> Errors { get; set; } = [];
}

/// <summary>
/// Кривая символики или ребро тела; координаты копируются в миллиметрах модели и вида.
/// </summary>
internal class DoorSymbolCurve {
    public string Kind { get; set; }
    public string Source { get; set; }
    public string GraphicsCategory { get; set; }
    public int OriginalPointCount { get; set; }
    public bool PointsTruncated { get; set; }
    public List<double[]> ModelPointsMm { get; set; } = [];
    public List<double[]> ViewPointsMm { get; set; } = [];
}
