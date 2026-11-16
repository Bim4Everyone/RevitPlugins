namespace RevitPunchingRebar.Models;

/// <summary>
/// Сохраняемое имя параметра. Для копирования имя одинаково у плиты и каркаса.
/// </summary>
internal sealed class FrameParameterBinding {
    public string TargetName { get; set; }
}
