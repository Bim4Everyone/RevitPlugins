using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitClashDetective.Models.Clashes;
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
            && x.SecondId == y.SecondId
            && (x.Clash?.MainElement?.TransformModel?.Equals(y.Clash?.MainElement?.TransformModel) ?? false)
            && (x.Clash?.OtherElement?.TransformModel?.Equals(y.Clash?.OtherElement?.TransformModel) ?? false);
    }

    public int GetHashCode(ClashViewModel obj) {
        int hash = 632234130;
        hash = hash * -1378137596 + EqualityComparer<ElementId>.Default.GetHashCode(obj?.FirstId);
        hash = hash * -1378137596 + EqualityComparer<ElementId>.Default.GetHashCode(obj?.SecondId);
        hash = hash * -1378137596 + EqualityComparer<string>.Default.GetHashCode(obj?.FirstDocumentName);
        hash = hash * -1378137596 + EqualityComparer<string>.Default.GetHashCode(obj?.SecondDocumentName);
        hash = hash * -1378137596
               + EqualityComparer<TransformModel>.Default.GetHashCode(obj?.Clash.MainElement.TransformModel);
        hash = hash * -1378137596
               + EqualityComparer<TransformModel>.Default.GetHashCode(obj?.Clash.OtherElement.TransformModel);
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

        result = x.SecondId.Compare(y.SecondId);
        if(result != 0) {
            return result;
        }

        result = CompareTransformModel(
            x.Clash?.MainElement?.TransformModel,
            y.Clash?.MainElement?.TransformModel);
        if(result != 0) {
            return result;
        }

        return CompareTransformModel(
            x.Clash?.OtherElement?.TransformModel,
            y.Clash?.OtherElement?.TransformModel);
    }

    private int CompareTransformModel(TransformModel x, TransformModel y) {
        if(ReferenceEquals(x, y)) {
            return 0;
        }

        if(x is null) {
            return -1;
        }

        if(y is null) {
            return 1;
        }

        int result = CompareXYZModel(x.Origin, y.Origin);
        if(result != 0) {
            return result;
        }

        result = CompareXYZModel(x.BasisX, y.BasisX);
        if(result != 0) {
            return result;
        }

        result = CompareXYZModel(x.BasisY, y.BasisY);
        if(result != 0) {
            return result;
        }

        return CompareXYZModel(x.BasisZ, y.BasisZ);
    }

    private int CompareXYZModel(XYZModel x, XYZModel y) {
        if(ReferenceEquals(x, y)) {
            return 0;
        }

        if(x is null) {
            return -1;
        }

        if(y is null) {
            return 1;
        }

        int result = x.X.CompareTo(y.X);
        if(result != 0) {
            return result;
        }

        result = x.Y.CompareTo(y.Y);
        if(result != 0) {
            return result;
        }

        return x.Z.CompareTo(y.Z);
    }
}
