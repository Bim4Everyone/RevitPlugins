using RevitRoundingOfAreas.Models.Enums;

namespace RevitRoundingOfAreas.Models.Warnings;

internal class WarningElement {
    public WarningType WarningType { get; set; }
    public SpatialElement SpatialElement { get; set; }
}
