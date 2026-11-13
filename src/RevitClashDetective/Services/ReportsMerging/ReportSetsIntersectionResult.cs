using System;
using System.Collections.Generic;
using System.Linq;

using dosymep.SimpleServices;

namespace RevitClashDetective.Services.ReportsMerging;

/// <summary>
/// Результат пересечения 2-ч множеств отчетов о коллизиях 
/// </summary>
internal class ReportSetsIntersectionResult {
    private static readonly ReportsNamesIgnoreCaseComparer _comparer = new();
    public ReportSetsIntersectionResult(
        ReportSet leftOuterSet,
        ReportSet leftInnerSet,
        ReportSet rightOuterSet,
        ReportSet rightInnerSet
    ) {
        LeftOuterSet = leftOuterSet ?? throw new ArgumentNullException(nameof(leftOuterSet));
        RightOuterSet = rightOuterSet ?? throw new ArgumentNullException(nameof(rightOuterSet));
        LeftInnerSet = leftInnerSet ?? throw new ArgumentNullException(nameof(leftInnerSet));
        RightInnerSet = rightInnerSet ?? throw new ArgumentNullException(nameof(rightInnerSet));

        if((LeftInnerSet.Count != RightInnerSet.Count)
           || LeftInnerSet.Reports.Select(r => r.Name)
               .Intersect(RightInnerSet.Reports.Select(r => r.Name))
               .Count()
           != LeftInnerSet.Count) {
            throw new ArgumentException("Правое множество пересечения не соответствует левому", nameof(leftInnerSet));
        }
    }

    /// <summary>
    /// Разность левого и правого множеств
    /// </summary>
    public ReportSet LeftOuterSet { get; }

    /// <summary>
    /// Разность правого и левого множеств
    /// </summary>
    public ReportSet RightOuterSet { get; }

    /// <summary>
    /// Множество левых объектов из пересечения множеств
    /// </summary>
    public ReportSet LeftInnerSet { get; }

    /// <summary>
    /// Множество правых объектов из пересечения множеств
    /// </summary>
    public ReportSet RightInnerSet { get; }

    public bool HasIntersection => LeftInnerSet.Count > 0;

    public ICollection<ReportsMergePair> GetMergePairs(ILocalizationService localizationService) {
        var leftIntersection = LeftInnerSet.Reports.OrderBy(r => r, _comparer).ToArray();
        var rightIntersection = RightInnerSet.Reports.OrderBy(r => r, _comparer).ToArray();
        List<ReportsMergePair> intersection = [];
        for(int i = 0; i < leftIntersection.Length; i++) {
            intersection.Add(new ReportsMergePair(localizationService, leftIntersection[i], rightIntersection[i]));
        }

        return intersection;
    }
}
