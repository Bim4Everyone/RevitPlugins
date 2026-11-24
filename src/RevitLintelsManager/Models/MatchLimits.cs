using RevitLintelsManager.Models.Configs;

namespace RevitLintelsManager.Models;

/// <summary>Линейные пределы сопоставления для каждой из трёх плоскостей, в миллиметрах.</summary>
internal sealed class MatchLimits(double alignmentMm, double searchMm) {
    public static MatchLimits Default { get; } = new(SystemPluginConfig.DefaultMatchAlignmentMm,
        SystemPluginConfig.DefaultMatchSearchMm);

    public double AlignmentMm { get; } = alignmentMm;
    public double SearchMm { get; } = searchMm;
    public bool IsValid => AlignmentMm >= 0 && SearchMm >= AlignmentMm && !double.IsInfinity(SearchMm);
}
