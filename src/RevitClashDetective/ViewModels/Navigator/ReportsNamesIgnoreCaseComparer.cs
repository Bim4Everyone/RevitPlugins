using System.Collections.Generic;

namespace RevitClashDetective.ViewModels.Navigator;
internal class ReportsNamesIgnoreCaseComparer : IEqualityComparer<ReportViewModel> {
    public bool Equals(ReportViewModel x, ReportViewModel y) {
        if(x is null || y is null || string.IsNullOrWhiteSpace(x.Name) || string.IsNullOrWhiteSpace(y.Name)) {
            return false;
        }
        if(ReferenceEquals(x, y)) {
            return true;
        }
        return x.Name.Equals(y.Name, System.StringComparison.CurrentCultureIgnoreCase);
    }

    public int GetHashCode(ReportViewModel obj) {
        return EqualityComparer<string>.Default.GetHashCode(obj.Name?.ToLower());
    }
}
