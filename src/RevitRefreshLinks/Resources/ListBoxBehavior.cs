using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace RevitRefreshLinks.Resources {
    internal static class ListBoxBehavior {
        public static readonly DependencyProperty SelectedItemsProperty =
            DependencyProperty.RegisterAttached(
                "SelectedItems",
                typeof(IList),
                typeof(ListBoxBehavior),
                new FrameworkPropertyMetadata(default, OnSelectedItemsChanged));

        public static IList GetSelectedItems(ListBox listBox) {
            return (IList) listBox.GetValue(SelectedItemsProperty);
        }

        public static void SetSelectedItems(ListBox listBox, IList value) {
            listBox.SetValue(SelectedItemsProperty, value);
        }

        private static void OnSelectedItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if(d is ListBox listBox) {
                listBox.SelectionChanged -= OnSelectionChanged;
                listBox.SelectionChanged += OnSelectionChanged;
            }
        }

        private static void OnSelectionChanged(object sender, SelectionChangedEventArgs e) {
            if(sender is ListBox listBox) {
                IList selectedItems = GetSelectedItems(listBox);
                selectedItems.Clear();
                foreach(var item in listBox.SelectedItems) {
                    selectedItems.Add(item);
                }
            }
        }
    }
}
