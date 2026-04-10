using System;
using System.Collections.Generic;
using System.Linq;

using RevitClashDetective.ViewModels.Navigator;

namespace RevitClashDetective.Services.ReportsMerging;

internal class ReportsMergePair {
    private static readonly ReportsNamesIgnoreCaseComparer _reportsComparer = new();
    private static readonly ClashIdDocComparer _clashComparer = new();


    public ReportsMergePair(ReportViewModel left, ReportViewModel right) {
        Left = left ?? throw new ArgumentNullException(nameof(left));
        Right = right ?? throw new ArgumentNullException(nameof(right));
        if(!_reportsComparer.Equals(Left, Right)) {
            throw new ArgumentException("Отчеты не соответствуют друг другу", nameof(right));
        }

        LeftOuterClashes = Left.Clashes.Except(right.Clashes, _clashComparer).ToArray();
        RightOuterClashes = Right.Clashes.Except(left.Clashes, _clashComparer).ToArray();
        MergeClashes = GetMergeClashes(Left, Right);
    }

    public ReportViewModel Left { get; }
    public ReportViewModel Right { get; }
    public ICollection<ClashViewModel> LeftOuterClashes { get; }
    public ICollection<ClashViewModel> RightOuterClashes { get; }
    public ICollection<ClashMergePair> MergeClashes { get; }

    private ICollection<ClashMergePair> GetMergeClashes(ReportViewModel left, ReportViewModel right) {
        var leftIntersection = left.Clashes.Intersect(right.Clashes, _clashComparer)
            .OrderBy(x => x, _clashComparer)
            .ToArray();
        var rightIntersection = right.Clashes.Intersect(left.Clashes, _clashComparer)
            .OrderBy(x => x, _clashComparer)
            .ToArray();
        List<ClashMergePair> mergeClashes = [];
        for(int i = 0; i < leftIntersection.Length; i++) {
            mergeClashes.Add(new ClashMergePair(leftIntersection[i], rightIntersection[i]));
        }

        return mergeClashes;
    }
}
