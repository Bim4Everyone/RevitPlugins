using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace RevitCreateViewSheet.Resources {
    internal static class DataGridSelectedItemsProperty {
        public static readonly DependencyProperty SelectedItemsProperty =
            DependencyProperty.RegisterAttached(
                "SelectedItems",
                typeof(IList),
                typeof(DataGridSelectedItemsProperty),
                new FrameworkPropertyMetadata(default, OnSelectedItemsChanged));

        public static IList GetSelectedItems(DataGrid dataGrid) {
            return (IList) dataGrid.GetValue(SelectedItemsProperty);
        }

        public static void SetSelectedItems(DataGrid dataGrid, IList value) {
            dataGrid.SetValue(SelectedItemsProperty, value);
        }

        private static void OnSelectedItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if(d is DataGrid dataGrid) {
                dataGrid.SelectionChanged -= OnSelectionChanged;
                dataGrid.SelectionChanged += OnSelectionChanged;
            }
        }

        private static void OnSelectionChanged(object sender, SelectionChangedEventArgs e) {
            if(sender is DataGrid dataGrid) {
                IList selectedItems = GetSelectedItems(dataGrid);
                selectedItems.Clear();
                foreach(var item in dataGrid.SelectedItems) {
                    selectedItems.Add(item);
                }
            }
        }
    }
}
