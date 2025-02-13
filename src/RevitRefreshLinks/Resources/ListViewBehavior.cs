using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace RevitRefreshLinks.Resources {
    internal static class ListViewBehavior {
        public static readonly DependencyProperty SelectedItemsProperty =
            DependencyProperty.RegisterAttached(
                "SelectedItems",
                typeof(IList),
                typeof(ListViewBehavior),
                new FrameworkPropertyMetadata(default, OnSelectedItemsChanged));

        public static IList GetSelectedItems(ListView listView) {
            return (IList) listView.GetValue(SelectedItemsProperty);
        }

        public static void SetSelectedItems(ListView listView, IList value) {
            listView.SetValue(SelectedItemsProperty, value);
        }

        private static void OnSelectedItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if(d is ListView listView) {
                listView.SelectionChanged -= OnSelectionChanged;
                listView.SelectionChanged += OnSelectionChanged;
            }
        }

        private static void OnSelectionChanged(object sender, SelectionChangedEventArgs e) {
            if(sender is ListView listView) {
                IList selectedItems = GetSelectedItems(listView);
                selectedItems.Clear();
                foreach(var item in listView.SelectedItems) {
                    selectedItems.Add(item);
                }
            }
        }
    }
}
