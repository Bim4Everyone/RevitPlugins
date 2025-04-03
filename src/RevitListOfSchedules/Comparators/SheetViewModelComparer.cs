using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitListOfSchedules.ViewModels;

namespace RevitListOfSchedules.Comparators {
    internal class SheetViewModelComparer : IComparer<SheetViewModel> {
        public int Compare(SheetViewModel x, SheetViewModel y) {
            int numberComparison = CompareNamesSafe(x.Number, y.Number);
            if(numberComparison != 0) {
                return numberComparison;
            }
            return CompareNamesSafe(x.AlbumName, y.AlbumName);
        }

        private int CompareNamesSafe(string x, string y) {
            if(x == null && y == null)
                return 0;
            if(x == null)
                return -1;
            if(y == null)
                return 1;
            return NamingUtils.CompareNames(x, y);
        }
    }
}
