using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

using Autodesk.Revit.DB;

using RevitAxonometryViews.Models;
using RevitAxonometryViews.ViewModels;

namespace RevitAxonometryViews.Views {
    public partial class MainWindow {
        private readonly MainViewModel _viewModel;
        internal ObservableCollection<HvacSystem> Items;

        internal MainWindow(MainViewModel viewModel) {
            InitializeComponent();
            _viewModel = viewModel;
            Items = viewModel.GetDataSource();
            lvSystems.ItemsSource = Items;
        }

        private void Button_Click_Ok(object sender, RoutedEventArgs e) {
            var selectedItems = Items.Where(item => item.IsSelected).ToList();
            _viewModel.CreateViews(selectedItems, useFopVisName.IsChecked, useOneView.IsChecked);
            DialogResult = true;
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
            DialogResult = false;
        }
    }
}
