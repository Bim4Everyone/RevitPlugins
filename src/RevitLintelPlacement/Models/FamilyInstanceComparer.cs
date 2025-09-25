using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitLintelPlacement.Models;

internal class FamilyInstanceComparer : IEqualityComparer<FamilyInstance> {
    public bool Equals(FamilyInstance x, FamilyInstance y) {
        return x?.Id == y?.Id;
    }

    public int GetHashCode(FamilyInstance obj) {
        return obj.Id.GetHashCode();
    }
}
