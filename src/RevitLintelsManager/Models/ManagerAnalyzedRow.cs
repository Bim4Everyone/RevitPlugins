namespace RevitLintelsManager.Models;

/// <summary>Проанализированная строка для существующих представлений менеджера.</summary>
internal sealed class ManagerAnalyzedRow(ManagerItemSnapshot snapshot, ManagerElementPresentation opening,
    ManagerElementPresentation lintel) {
    public ManagerItemSnapshot Snapshot { get; } = snapshot;
    public ManagerElementPresentation Opening { get; } = opening;
    public ManagerElementPresentation Lintel { get; } = lintel;
    internal OpeningLintelMatch Match { get; set; }
    internal LintelComparison Comparison { get; set; }
}
