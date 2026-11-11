using RevitEnumerateBySpline.Models.Enums;

namespace RevitEnumerateBySpline.Models.Warnings;

internal class WarningElement {
    public WarningType WarningType { get; set; }
    public SpatialModel SpatialModel { get; set; } = null!;
}
