using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace RevitCreateViewSheet.Resources {
    public class CustomSortBehavior {
        public static readonly DependencyProperty CustomSorterProperty =
            DependencyProperty.RegisterAttached(
                "CustomSorter",
                typeof(ICustomSorter),
                typeof(CustomSortBehavior));

        public static readonly DependencyProperty AllowCustomSortProperty =
            DependencyProperty.RegisterAttached(
                "AllowCustomSort",
                typeof(bool),
                typeof(CustomSortBehavior),
                new UIPropertyMetadata(false, OnAllowCustomSortChanged));

        public static ICustomSorter GetCustomSorter(DataGridColumn gridColumn) {
            return (ICustomSorter) gridColumn.GetValue(CustomSorterProperty);
        }

        public static void SetCustomSorter(DataGridColumn gridColumn, ICustomSorter value) {
            gridColumn.SetValue(CustomSorterProperty, value);
        }

        public static bool GetAllowCustomSort(DataGrid grid) {
            return (bool) grid.GetValue(AllowCustomSortProperty);
        }

        public static void SetAllowCustomSort(DataGrid grid, bool value) {
            grid.SetValue(AllowCustomSortProperty, value);
        }

        private static void OnAllowCustomSortChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if(d is not DataGrid existing) {
                return;
            }

            var oldAllow = (bool) e.OldValue;
            var newAllow = (bool) e.NewValue;

            if(!oldAllow && newAllow) {
                existing.Sorting += HandleCustomSorting;
            } else {
                existing.Sorting -= HandleCustomSorting;
            }
        }

        private static void HandleCustomSorting(object sender, DataGridSortingEventArgs e) {
            if(sender is not DataGrid dataGrid || !GetAllowCustomSort(dataGrid)) {
                return;
            }

            if(dataGrid.ItemsSource is not ListCollectionView listColView) {
                return;
            }

            var sorter = GetCustomSorter(e.Column);
            if(sorter is null) {
                return;
            }

            e.Handled = true;

            var direction = (e.Column.SortDirection != ListSortDirection.Ascending)
                ? ListSortDirection.Ascending
                : ListSortDirection.Descending;

            e.Column.SortDirection = sorter.SortDirection = direction;
            listColView.CustomSort = sorter;
        }
    }
}
