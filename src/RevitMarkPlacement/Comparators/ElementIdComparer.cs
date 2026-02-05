using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitMarkPlacement.Comparators;

internal class ElementIdComparer<T> : IEqualityComparer<T> where T : Element {
    public bool Equals(T x, T y) {
        return x?.Id == y?.Id;
    }

    public int GetHashCode(T obj) {
        return obj?.Id.GetHashCode() ?? 0;
    }
}
