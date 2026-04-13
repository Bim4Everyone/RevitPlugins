using System;
using System.Collections.Generic;
using System.Linq;

using RevitClashDetective.ViewModels.Navigator;

namespace RevitClashDetective.Services.ReportsMerging;

internal class ReportsMergePair {
    private static readonly ReportsNamesIgnoreCaseComparer _reportsComparer = new();
    private static readonly ClashIdDocComparer _clashComparer = new();

    public ReportsMergePair(ReportViewModel existing, ReportViewModel importing) {
        Existing = existing ?? throw new ArgumentNullException(nameof(existing));
        Importing = importing ?? throw new ArgumentNullException(nameof(importing));
        if(!_reportsComparer.Equals(Existing, Importing)) {
            throw new ArgumentException("Отчеты не соответствуют друг другу", nameof(importing));
        }

        ExistingOuterClashes = Existing.GetClashes().Except(importing.GetClashes(), _clashComparer).ToArray();
        ImportingOuterClashes = Importing.GetClashes().Except(existing.GetClashes(), _clashComparer).ToArray();
        IntersectionClashes = new ClashesMergePairGroups(GetIntersection(Existing, Importing));
    }

    public ReportViewModel Existing { get; }
    public ReportViewModel Importing { get; }

    /// <summary>
    /// Множество коллизий из существующего отчета, которых нет в импортируемом
    /// </summary>
    public ICollection<ClashViewModel> ExistingOuterClashes { get; }

    /// <summary>
    /// Множество коллизий из импортируемого отчета, которых нет в существующем
    /// </summary>
    public ICollection<ClashViewModel> ImportingOuterClashes { get; }

    /// <summary>
    /// Множество коллизий, которые есть в обоих отчетах
    /// </summary>
    public ClashesMergePairGroups IntersectionClashes { get; }

    private ICollection<ClashMergePairViewModel> GetIntersection(ReportViewModel left, ReportViewModel right) {
        var leftIntersection = left.GetClashes()
            .Intersect(right.GetClashes(), _clashComparer)
            .OrderBy(x => x, _clashComparer)
            .ToArray();
        var rightIntersection = right.GetClashes()
            .Intersect(left.GetClashes(), _clashComparer)
            .OrderBy(x => x, _clashComparer)
            .ToArray();
        List<ClashMergePairViewModel> intersection = [];
        for(int i = 0; i < leftIntersection.Length; i++) {
            intersection.Add(new ClashMergePairViewModel(leftIntersection[i], rightIntersection[i]));
        }

        return intersection;
    }
}
