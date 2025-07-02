using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitListOfSchedules.ViewModels;

namespace RevitListOfSchedules.Comparators {
    internal class SheetViewModelComparer : IComparer<SheetViewModel> {
        public int Compare(SheetViewModel x, SheetViewModel y) {
            int numberComparison = CompareNamesSafe(x.Number, y.Number);
            return numberComparison != 0 ? numberComparison : CompareNamesSafe(x.AlbumName, y.AlbumName);
        }

        private int CompareNamesSafe(string x, string y) {
            if(x == null && y == null) {
                return 0;
            }

            return x == null ? -1 : y == null ? 1 : NamingUtils.CompareNames(x, y);
        }
    }
}
