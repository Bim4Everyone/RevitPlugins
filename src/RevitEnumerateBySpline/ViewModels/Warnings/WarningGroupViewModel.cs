using System.Collections.Generic;

namespace RevitEnumerateBySpline.ViewModels.Warnings;

/// <summary>Группа предупреждений для отображения.</summary>
internal sealed class WarningGroupViewModel {
    public string Caption { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int WarningQuantity { get; set; }
    public IReadOnlyList<WarningElementViewModel> Warnings { get; set; } = [];
}
