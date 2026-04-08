using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitSuperfilter.Comparators;

internal sealed class ElementIdComparer : IComparer<Element>, IEqualityComparer<Element> {
    public static readonly ElementIdComparer Instance = new();
    
    public bool Equals(Element x, Element y) {
        if(ReferenceEquals(x, y)) {
            return true;
        }

        if(x is null) {
            return false;
        }

        if(y is null) {
            return false;
        }

        if(x.GetType() != y.GetType()) {
            return false;
        }

        return Equals(x.Id, y.Id);
    }

    public int GetHashCode(Element obj) {
        return (obj.Id != null ? obj.Id.GetHashCode() : 0);
    }

    public int Compare(Element x, Element y) {
        if(ReferenceEquals(x, y)) {
            return 0;
        }

        if(y is null) {
            return 1;
        }

        if(x is null) {
            return -1;
        }

        return x.Id.Compare(y.Id);
    }
}
