using System.Collections.Generic;

namespace RevitEnumerateBySpline.Models.Settings;

/// <summary>Настройки запуска нумерации по сплайну.</summary>
internal class EnumerateBySplineSettings {
    public ConfigSettings? ConfigSettings { get; set; }
    public List<SpatialModel?>? SpatialModels { get; set; }
    public List<CurveModel?>? CurveModels { get; set; }
}
