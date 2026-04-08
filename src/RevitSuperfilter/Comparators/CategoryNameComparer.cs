using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitSuperfilter.Comparators;

internal sealed class CategoryNameComparer : IComparer<Category>, IEqualityComparer<Category> {
    public static readonly CategoryNameComparer Instance = new();
    
    public bool Equals(Category x, Category y) {
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

        return x.Name == y.Name;
    }

    public int GetHashCode(Category obj) {
        return (obj.Name != null ? obj.Name.GetHashCode() : 0);
    }

    public int Compare(Category x, Category y) {
        if(ReferenceEquals(x, y)) {
            return 0;
        }

        if(y is null) {
            return 1;
        }

        if(x is null) {
            return -1;
        }

        return string.Compare(x.Name, y.Name, StringComparison.Ordinal);
    }
}
