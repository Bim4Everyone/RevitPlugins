using System.Collections;
using System.Linq;

using DevExpress.Data.Filtering;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.Grid;

namespace RevitClashDetective.Controls;
internal class DataGridControl : GridControl {
    private readonly ObservableCollectionCore<object> mySelectedItems;
    public IList MySelectedItems => mySelectedItems;
    public DataGridControl() {
        SelectionChanged += MyGridControl_SelectionChanged;
        mySelectedItems = [];
    }

    private readonly Hashtable selection = [];
    private IEnumerable OrderedSelection => selection.Keys.Cast<int>().OrderBy(x => x);
    protected override void OnItemsSourceChanged(object oldValue, object newValue) {
        base.OnItemsSourceChanged(oldValue, newValue);
        selection.Clear();
        if(newValue is not IEnumerable itemsSource) {
            return;
        }

        int i = 0;
        foreach(object item in itemsSource) {
            selection[i++] = false;
        }
    }


    private readonly Locker updateLocker = new();
    private void MyGridControl_SelectionChanged(object sender, GridSelectionChangedEventArgs e) {
        if(updateLocker.IsLocked) {
            return;
        }

        for(int i = 0; i < VisibleRowCount; i++) {
            int rowHandle = GetRowHandleByVisibleIndex(i);
            selection[GetListIndexByRowHandle(rowHandle)] = View.IsRowSelected(rowHandle);
        }
        mySelectedItems.BeginUpdate();
        mySelectedItems.Clear();
        foreach(int index in OrderedSelection) {
            if((bool) selection[index]) {
                mySelectedItems.Add(GetRowByListIndex(index));
            }
        }
        mySelectedItems.EndUpdate();
    }

    protected override void ApplyFilter(CriteriaOperator op, FilterGroupSortChangingEventArgs filterSortArgs, bool skipIfFilterEquals) {
        updateLocker.DoLockedAction(() => {
            base.ApplyFilter(op, filterSortArgs, skipIfFilterEquals);
            BeginSelection();
            foreach(int index in OrderedSelection) {
                if((bool) selection[index]) {
                    SelectItem(GetRowHandleByListIndex(index));
                }
            }
            EndSelection();
        });
    }


}
