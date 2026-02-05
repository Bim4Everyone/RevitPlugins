using dosymep.Bim4Everyone;

using RevitSetCoordParams.Models.Enums;

namespace RevitSetCoordParams.Models;

internal class ParamMap {
    public ParamType Type { get; set; }
    public RevitParam SourceParam { get; set; }
    public RevitParam TargetParam { get; set; }
}
