using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitDeclarations.Comparators;

public sealed class RevitLogicalStringComparer : IComparer<string> {
    public static readonly IComparer<string> Instance = new RevitLogicalStringComparer();

    public int Compare(string x, string y) {
        if (ReferenceEquals(x, y)) return 0;
        if (x is null) {
            return -1;
        }
        return y is null 
            ? 1 
            : NamingUtils.CompareNames(x, y);
    }
}
