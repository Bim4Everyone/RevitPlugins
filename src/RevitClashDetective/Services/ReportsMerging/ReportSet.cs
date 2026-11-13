using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using RevitClashDetective.ViewModels.Navigator;

namespace RevitClashDetective.Services.ReportsMerging;

/// <summary>
/// Множество отчетов о коллизиях
/// </summary>
internal class ReportSet {
    private static readonly ReportsNamesIgnoreCaseComparer _comparer = new();

    public ReportSet(IEnumerable<ReportViewModel> reports) {
        Reports = new ReadOnlyCollection<ReportViewModel>(
            reports?.ToArray()
            ?? throw new ArgumentNullException(nameof(reports)));
    }

    public ICollection<ReportViewModel> Reports { get; }

    public int Count => Reports.Count;

    /// <summary>
    /// Находит пересечение текущего множества с другим множеством
    /// </summary>
    /// <param name="other">Множество отчетов о коллизиях</param>
    /// <returns>Результат пересечения множеств</returns>
    public ReportSetsIntersectionResult Intersect(ReportSet other) {
        if(other == null) {
            throw new ArgumentNullException(nameof(other));
        }

        var thisOuter = new ReportSet(Reports.Except(other.Reports, _comparer));
        var thisInner = new ReportSet(Reports.Intersect(other.Reports, _comparer));
        var otherOuter = new ReportSet(other.Reports.Except(Reports, _comparer));
        var otherInner = new ReportSet(other.Reports.Intersect(Reports, _comparer));

        return new ReportSetsIntersectionResult(thisOuter, thisInner, otherOuter, otherInner);
    }

    /// <summary>
    /// Возвращает объединение текущего множества с другими множествами
    /// </summary>
    /// <param name="others">Множества отчетов о коллизиях</param>
    /// <returns>Результат объединения множеств</returns>
    public ReportSet Union(params ReportSet[] others) {
        if(others == null) {
            throw new ArgumentNullException(nameof(others));
        }

        IEnumerable<ReportViewModel> reports = Reports;
        foreach(var other in others) {
            reports = reports.Union(other.Reports, _comparer);
        }

        return new ReportSet(reports);
    }
}
