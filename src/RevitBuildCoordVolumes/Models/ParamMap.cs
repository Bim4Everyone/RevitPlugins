using dosymep.Bim4Everyone;

using RevitBuildCoordVolumes.Models.Enums;

namespace RevitBuildCoordVolumes.Models;

internal class ParamMap {
    public ParamType Type { get; set; }
    public ParamTypeValue TypeValue { get; set; }
    public RevitParam SourceParam { get; set; }
    public RevitParam TargetParam { get; set; }
}
