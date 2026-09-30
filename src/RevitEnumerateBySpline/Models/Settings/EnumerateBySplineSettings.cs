using System.Collections.Generic;

using dosymep.Bim4Everyone;

namespace RevitEnumerateBySpline.Models.Settings;

internal class EnumerateBySplineSettings {
    public ConfigSettings? ConfigSettings { get; set; }
    public List<SpatialModel?>? SpatialModels { get; set; }
    public List<CurveModel?>? CurveModels { get; set; }
}
