using System;
using System.Collections.Generic;
using System.Linq;

using RevitClashDetective.ViewModels.Navigator;

namespace RevitClashDetective.Services.ReportsMerging;

internal class ReportsMergePair {
    private static readonly ReportsNamesIgnoreCaseComparer _reportsComparer = new();
    private static readonly ClashIdDocComparer _clashComparer = new();
    private readonly ReportViewModel _left;
    private readonly ReportViewModel _right;

    public ReportsMergePair(ReportViewModel left, ReportViewModel right) {
        _left = left ?? throw new ArgumentNullException(nameof(left));
        _right = right ?? throw new ArgumentNullException(nameof(right));
        if(!_reportsComparer.Equals(_left, _right)) {
            throw new ArgumentException("Отчеты не соответствуют друг другу", nameof(right));
        }

        LeftOuterClashes = left.Clashes.Except(right.Clashes, _clashComparer).ToArray();
        RightOuterClashes = right.Clashes.Except(left.Clashes, _clashComparer).ToArray();
        ManualMergeClashes = GetManualMergeClashes();
        AutoMergeClashes = GetAutoMergeClashes();
    }

    public ICollection<ClashViewModel> LeftOuterClashes { get; }
    public ICollection<ClashViewModel> RightOuterClashes { get; }
    public ICollection<ClashMergeViewModel> ManualMergeClashes { get; }
    public ICollection<ClashMergeViewModel> AutoMergeClashes { get; }

    private ICollection<ClashMergeViewModel> GetManualMergeClashes() {
        throw new NotImplementedException();
    }

    private ICollection<ClashMergeViewModel> GetAutoMergeClashes() {
        throw new NotImplementedException();
    }
}
