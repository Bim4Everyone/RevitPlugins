using RevitEnumerateBySpline.Models.Enums;

namespace RevitEnumerateBySpline.Models.Warnings;

/// <summary>Предупреждение о выбранном помещении.</summary>
internal class WarningElement {
    public WarningType WarningType { get; set; }
    public SpatialModel SpatialModel { get; set; } = null!;
}
