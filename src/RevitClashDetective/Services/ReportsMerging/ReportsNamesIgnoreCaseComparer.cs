using System;
using System.Collections.Generic;

using RevitClashDetective.ViewModels.Navigator;

namespace RevitClashDetective.Services.ReportsMerging;

internal class ReportsNamesIgnoreCaseComparer : IEqualityComparer<ReportViewModel>, IComparer<ReportViewModel> {
    public bool Equals(ReportViewModel x, ReportViewModel y) {
        if(x is null || y is null || string.IsNullOrWhiteSpace(x.Name) || string.IsNullOrWhiteSpace(y.Name)) {
            return false;
        }
        if(ReferenceEquals(x, y)) {
            return true;
        }

        return x.Name.Equals(y.Name, StringComparison.Ordinal);
    }

    public int GetHashCode(ReportViewModel obj) {
        return EqualityComparer<string>.Default.GetHashCode(obj.Name);
    }

    public int Compare(ReportViewModel x, ReportViewModel y) {
        if(ReferenceEquals(x, y)) {
            return 0;
        }

        if(x is null) {
            return -1;
        }

        if(y is null) {
            return 1;
        }

        return string.Compare(x.Name, y.Name, StringComparison.Ordinal);
    }
}
