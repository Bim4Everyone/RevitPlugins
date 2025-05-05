using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace RevitCreateViewSheet.Resources {
    public static class DataGridSyncEditToSelectedItemProperty {
        public static readonly DependencyProperty SyncEditToSelectedItemProperty =
            DependencyProperty.RegisterAttached(
                "SyncEditToSelectedItem",
                typeof(bool),
                typeof(DataGridSyncEditToSelectedItemProperty),
                new PropertyMetadata(false, OnSyncEditToSelectedItemChanged));

        public static bool GetSyncEditToSelectedItem(DependencyObject obj) {
            return (bool) obj.GetValue(SyncEditToSelectedItemProperty);
        }

        public static void SetSyncEditToSelectedItem(DependencyObject obj, bool value) {
            obj.SetValue(SyncEditToSelectedItemProperty, value);
        }

        private static void OnSyncEditToSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if(d is DataGrid dataGrid) {
                if((bool) e.NewValue) {
                    dataGrid.CurrentCellChanged += OnDataGridCurrentCellChanged;
                } else {
                    dataGrid.CurrentCellChanged -= OnDataGridCurrentCellChanged;
                }
            }
        }

        private static void OnDataGridCurrentCellChanged(object sender, EventArgs e) {
            bool modifierIsPressed =
                (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control
                || (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift;

            if(sender is DataGrid dataGrid) {
                dataGrid.Dispatcher.InvokeAsync(() => {
                    object[] selectedItems = [.. dataGrid.SelectedItems];
                    var currentItem = dataGrid.CurrentItem;
                    if(currentItem is not null && !Equals(dataGrid.SelectedItem, currentItem)) {
                        dataGrid.SelectedItem = currentItem;
                        if(modifierIsPressed && selectedItems.Any(i => ReferenceEquals(i, currentItem))) {
                            foreach(var item in selectedItems) {
                                if(!ReferenceEquals(item, currentItem)) {
                                    dataGrid.SelectedItems.Add(item);
                                }
                            }
                        }
                    }
                    var sortedSelectedItems = dataGrid.SelectedItems
                        .Cast<object>()
                        .OrderBy(item => dataGrid.Items.IndexOf(item))
                        .ToArray();
                    dataGrid.SelectedItems.Clear();
                    for(int i = 0; i < sortedSelectedItems.Length; i++) {
                        dataGrid.SelectedItems.Add(sortedSelectedItems[i]);
                    }
                }, System.Windows.Threading.DispatcherPriority.Input);
            }
        }
    }
}
