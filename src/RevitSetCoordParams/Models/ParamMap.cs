using dosymep.Bim4Everyone;

namespace RevitSetCoordParams.Models;

internal class ParamMap {
    public RevitParam SourceParam { get; set; }
    public RevitParam TargetParam { get; set; }
    public string LocalizationKey { get; set; }
    public bool IsPair => SourceParam != null && TargetParam != null;
}
