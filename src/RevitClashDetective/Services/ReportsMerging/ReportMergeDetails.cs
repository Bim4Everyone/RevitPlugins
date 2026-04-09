using System;
using System.Collections.Generic;

using RevitClashDetective.ViewModels.Navigator;

namespace RevitClashDetective.Services.ReportsMerging;

internal class ReportMergeDetails {
    public ReportMergeDetails(
        IList<ClashViewModel> unchangedClashes,
        IList<ClashViewModel> newClashes,
        IList<ClashMergeViewModel> mergeConflictClashes,
        IList<ClashMergeViewModel> autoMergedClashes) {
        UnchangedClashes = unchangedClashes ?? throw new ArgumentNullException(nameof(unchangedClashes));
        NewClashes = newClashes ?? throw new ArgumentNullException(nameof(newClashes));
        MergeConflictClashes = mergeConflictClashes ?? throw new ArgumentNullException(nameof(mergeConflictClashes));
        AutoMergedClashes = autoMergedClashes ?? throw new ArgumentNullException(nameof(autoMergedClashes));
    }

    public IList<ClashViewModel> UnchangedClashes { get; }
    public IList<ClashViewModel> NewClashes { get; }
    public IList<ClashMergeViewModel> MergeConflictClashes { get; }
    public IList<ClashMergeViewModel> AutoMergedClashes { get; }
}
