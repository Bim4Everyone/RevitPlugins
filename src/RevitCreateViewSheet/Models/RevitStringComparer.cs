using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitCreateViewSheet.Models;

internal class RevitStringComparer : IComparer<string> {
    public int Compare(string x, string y) {
        return NamingUtils.CompareNames(x, y);
    }
}
