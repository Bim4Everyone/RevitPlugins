using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitSuperfilter.Models;

internal class ElementTypeFamilyNameComparer : IEqualityComparer<ElementType> {
    public bool Equals(ElementType x, ElementType y) {
        if(x == null
           && y == null) {
            return true;
        }

        if(x == null
           || y == null) {
            return false;
        }

        return x.FamilyName.Equals(y.FamilyName);
    }

    public int GetHashCode(ElementType obj) {
        return obj?.FamilyName.GetHashCode() ?? 0;
    }
}
