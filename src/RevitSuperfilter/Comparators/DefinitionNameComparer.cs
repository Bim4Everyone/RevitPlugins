using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitSuperfilter.Comparators;

internal sealed class DefinitionNameComparer : IComparer<Definition>, IEqualityComparer<Definition> {
    public static readonly DefinitionNameComparer Instance = new();
    
    public bool Equals(Definition x, Definition y) {
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

    public int GetHashCode(Definition obj) {
        return (obj.Name != null ? obj.Name.GetHashCode() : 0);
    }

    public int Compare(Definition x, Definition y) {
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
