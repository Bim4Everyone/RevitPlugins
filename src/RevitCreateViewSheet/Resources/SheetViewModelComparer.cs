using System.ComponentModel;

using dosymep.Revit.Comparators;

using RevitCreateViewSheet.ViewModels;

namespace RevitCreateViewSheet.Resources {
    public class SheetViewModelComparer : ICustomSorter {
        private readonly LogicalStringComparer _comparer;

        public SheetViewModelComparer() {
            _comparer = new LogicalStringComparer();
        }


        public ListSortDirection SortDirection { get; set; }

        public int Compare(object x, object y) {
            var vm1 = x as SheetViewModel;
            var vm2 = y as SheetViewModel;
            if(vm1 is not null && vm2 is not null) {

                return SortDirection == ListSortDirection.Ascending
                    ? _comparer.Compare(vm1.SheetNumber, vm2.SheetNumber)
                    : _comparer.Compare(vm2.SheetNumber, vm1.SheetNumber);
            } else {
                return 0;
            }
        }
    }
}
