using System.Collections.Generic;

using RevitClashDetective.ViewModels.Navigator;

namespace RevitClashDetective.Services.ReportsMerging;

internal class ReportsMergeServant {
    public ReportMergeDetails Calculate(IList<ClashViewModel> existingClashes, IList<ClashViewModel> newClashes) {
        return new ReportMergeDetails(
            existingClashes,
            newClashes,
            new List<ClashMergeViewModel>(),
            new List<ClashMergeViewModel>());
    }
}
