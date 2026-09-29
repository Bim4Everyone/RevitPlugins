using System.Collections.Generic;

using dosymep.Bim4Everyone;

namespace RevitEnumerateBySpline.Models.Settings;

internal class EnumeratorBySplineSettings {
    public double? StartNumber { get; set; }
    public string? Prefix { get; set; }
    public string? Suffix { get; set; }
    public string? SearchKey { get; set; }
    public RevitParam? DependentSearchParam { get; set; }
    public RevitParam? DependentParam { get; set; }
    public List<SpatialModel?>? SpatialModels { get; set; }
    public List<CurveModel?>? CurveModels { get; set; }
}
