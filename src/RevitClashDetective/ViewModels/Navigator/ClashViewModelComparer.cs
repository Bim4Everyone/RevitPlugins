using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitClashDetective.ViewModels.Navigator;
internal class ClashViewModelComparer : IEqualityComparer<IClashViewModel> {
    public bool Equals(IClashViewModel x, IClashViewModel y) {
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

    public int GetHashCode(IClashViewModel obj) {
        int hash = 632234130;
        hash = hash * -1378137596 + EqualityComparer<ElementId>.Default.GetHashCode(obj?.FirstId);
        hash = hash * -1378137596 + EqualityComparer<ElementId>.Default.GetHashCode(obj?.SecondId);
        hash = hash * -1378137596 + EqualityComparer<string>.Default.GetHashCode(obj?.FirstDocumentName);
        hash = hash * -1378137596 + EqualityComparer<string>.Default.GetHashCode(obj?.SecondDocumentName);
        return hash;
    }
}
