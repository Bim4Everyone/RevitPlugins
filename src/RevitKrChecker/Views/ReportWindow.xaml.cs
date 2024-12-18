using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

using RevitKrChecker.ViewModels;

namespace RevitKrChecker.Views {
    /// <summary>
    /// Логика взаимодействия для ReportWindow.xaml
    /// </summary>
    public partial class ReportWindow : Window {

        private readonly string _notListItem = "<Нет>";

        public ReportWindow() {
            InitializeComponent();

            ListCollectionView = (CollectionView) CollectionViewSource.GetDefaultView(reportList.ItemsSource);
            var comboboxList = new List<string>() { _notListItem };

            foreach(var prop in typeof(ReportItemVM).GetProperties()) {
                comboboxList.Add(prop.Name);
            }

            FirstLevelGrouping.ItemsSource = comboboxList;
            FirstLevelGrouping.SelectedIndex = 0;
            SecondLevelGrouping.ItemsSource = comboboxList;
            SecondLevelGrouping.SelectedIndex = 0;
            ThirdLevelGrouping.ItemsSource = comboboxList;
            ThirdLevelGrouping.SelectedIndex = 0;
        }

        public CollectionView ListCollectionView { get; set; }
        public string ComboBox1Value { get; set; }
        public string ComboBox2Value { get; set; }
        public string ComboBox3Value { get; set; }


        private void FirstLevelGrouping_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e) {
            if(FirstLevelGrouping.SelectedItem != null) {
                ComboBox1Value = FirstLevelGrouping.SelectedItem as string;
                if(ComboBox1Value == _notListItem) {
                    ComboBox2Value = ComboBox3Value = _notListItem;
                }
                ListGroupingUpdate();
            }
        }

        private void SecondLevelGrouping_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e) {
            if(SecondLevelGrouping.SelectedItem != null) {
                ComboBox2Value = SecondLevelGrouping.SelectedItem as string;
                if(ComboBox2Value == _notListItem) {
                    ComboBox3Value = _notListItem;
                }
                ListGroupingUpdate();
            }
        }

        private void ThirdLevelGrouping_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e) {
            if(ThirdLevelGrouping.SelectedItem != null) {
                ComboBox3Value = ThirdLevelGrouping.SelectedItem as string;
                ListGroupingUpdate();
            }
        }

        private void ListGroupingUpdate() {
            if(ListCollectionView is null) {
                ListCollectionView = (CollectionView) CollectionViewSource.GetDefaultView(reportList.ItemsSource);
            }
            if(ListCollectionView is null) {
                return;
            }

            ListCollectionView.GroupDescriptions.Clear();

            if(!string.IsNullOrEmpty(ComboBox1Value) && ComboBox1Value != _notListItem) {
                PropertyGroupDescription group1 = new PropertyGroupDescription(ComboBox1Value);
                ListCollectionView.GroupDescriptions.Add(group1);
            }

            if(!string.IsNullOrEmpty(ComboBox2Value) && ComboBox2Value != _notListItem) {
                PropertyGroupDescription group2 = new PropertyGroupDescription(ComboBox2Value);
                ListCollectionView.GroupDescriptions.Add(group2);
            }

            if(!string.IsNullOrEmpty(ComboBox3Value) && ComboBox3Value != _notListItem) {
                PropertyGroupDescription group3 = new PropertyGroupDescription(ComboBox3Value);
                ListCollectionView.GroupDescriptions.Add(group3);
            }
        }


        private void Button_Click_ListViewExpander_Expand(object sender, RoutedEventArgs e) {
            foreach(GroupItem gi in FindVisualChildren<GroupItem>(reportList))
                gi.Tag = true;
        }

        private void Button_Click_ListViewExpander_Collapse(object sender, RoutedEventArgs e) {
            foreach(GroupItem gi in FindVisualChildren<GroupItem>(reportList))
                gi.Tag = false;
        }

        private static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject {
            if(depObj != null) {
                for(int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++) {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if(child != null && child is T) {
                        yield return (T) child;
                    }

                    foreach(T childOfChild in FindVisualChildren<T>(child)) {
                        yield return childOfChild;
                    }
                }
            }
        }
    }
}
