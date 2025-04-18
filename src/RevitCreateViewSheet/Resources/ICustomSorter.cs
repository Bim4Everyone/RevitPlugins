using System.Collections;
using System.ComponentModel;

namespace RevitCreateViewSheet.Resources {
    public interface ICustomSorter : IComparer {
        ListSortDirection SortDirection { get; set; }
    }
}
