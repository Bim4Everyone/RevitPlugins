using dosymep.SimpleServices;

namespace RevitLintelsManager.Models;

/// <summary>Начальные настройки отдельного окна прогресса.</summary>
internal sealed class BatchProgressOptions {
    public int Count { get; set; }
    public string Language { get; set; }
    public UIThemes Theme { get; set; }
    public string ProgressFormat { get; set; }
    public string CancelText { get; set; }
}
