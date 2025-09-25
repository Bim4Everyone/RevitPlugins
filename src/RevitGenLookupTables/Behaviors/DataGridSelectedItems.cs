using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace RevitGenLookupTables.Behaviors {
    public static class DataGridSelectedItems {
        public static readonly DependencyProperty SelectedItemsProperty =
            DependencyProperty.RegisterAttached(
                "SelectedItems",
                typeof(IList),
                typeof(DataGridSelectedItems),
                new PropertyMetadata(null, OnBindableSelectedItemsChanged));

        public static void SetSelectedItems(DependencyObject element, IList value) {
            element.SetValue(SelectedItemsProperty, value);
        }

        public static IList GetSelectedItems(DependencyObject element) {
            return (IList) element.GetValue(SelectedItemsProperty);
        }

        private static void OnBindableSelectedItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if(d is DataGrid dataGrid) {
                dataGrid.Unloaded += DataGridOnUnloaded;
                dataGrid.SelectionChanged -= DataGrid_SelectionChanged;
                dataGrid.SelectionChanged += DataGrid_SelectionChanged;
            }
        }

        private static void DataGridOnUnloaded(object sender, RoutedEventArgs e) {
            if(sender is DataGrid dataGrid) {
                dataGrid.Unloaded -= DataGridOnUnloaded;
                dataGrid.SelectionChanged -= DataGrid_SelectionChanged;
            }
        }

        private static void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if(sender is DataGrid dataGrid) {
                var list = GetSelectedItems(dataGrid);
                if(list == null) return;

                list.Clear();
                foreach(object item in dataGrid.SelectedItems) {
                    list.Add(item);
                }
            }
        }
    }
}
