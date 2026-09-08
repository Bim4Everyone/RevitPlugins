using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitBatchPrint.Comparators;

internal sealed class NamingComparator : IComparer<string> {
    public int Compare(string x, string y) {
        if(ReferenceEquals(x, y)) {
            return 0;
        }

        if(string.IsNullOrEmpty(x)) {
            return -1;
        }

        if(string.IsNullOrEmpty(y)) {
            return 1;
        }

        return NamingUtils.CompareNames(x, y);
    }
}
