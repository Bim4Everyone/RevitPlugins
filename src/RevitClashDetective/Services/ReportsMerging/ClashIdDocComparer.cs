using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitClashDetective.ViewModels.Navigator;

namespace RevitClashDetective.Services.ReportsMerging;

internal class ClashIdDocComparer : IEqualityComparer<ClashViewModel>, IComparer<ClashViewModel> {
    public bool Equals(ClashViewModel x, ClashViewModel y) {
        if(x is null || y is null) {
            return false;
        }
        if(ReferenceEquals(x, y)) {
            return true;
        }
        return x.FirstDocumentName == y.FirstDocumentName
            && x.SecondDocumentName == y.SecondDocumentName
            && x.FirstId == y.FirstId
            && x.SecondId == y.SecondId;
    }

    public int GetHashCode(ClashViewModel obj) {
        int hash = 632234130;
        hash = hash * -1378137596 + EqualityComparer<ElementId>.Default.GetHashCode(obj?.FirstId);
        hash = hash * -1378137596 + EqualityComparer<ElementId>.Default.GetHashCode(obj?.SecondId);
        hash = hash * -1378137596 + EqualityComparer<string>.Default.GetHashCode(obj?.FirstDocumentName);
        hash = hash * -1378137596 + EqualityComparer<string>.Default.GetHashCode(obj?.SecondDocumentName);
        return hash;
    }

    public int Compare(ClashViewModel x, ClashViewModel y) {
        if(ReferenceEquals(x, y)) {
            return 0;
        }

        if(x is null) {
            return -1;
        }

        if(y is null) {
            return 1;
        }

        int result = string.Compare(x.FirstDocumentName, y.FirstDocumentName, StringComparison.Ordinal);
        if(result != 0) {
            return result;
        }

        result = string.Compare(x.SecondDocumentName, y.SecondDocumentName, StringComparison.Ordinal);
        if(result != 0) {
            return result;
        }

        result = x.FirstId.Compare(y.FirstId);
        if(result != 0) {
            return result;
        }

        return x.SecondId.Compare(y.SecondId);
    }
}
