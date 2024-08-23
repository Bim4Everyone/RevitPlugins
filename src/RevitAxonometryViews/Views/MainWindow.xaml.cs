using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

using Autodesk.Revit.DB;

using RevitAxonometryViews.Models;
using RevitAxonometryViews.ViewModels;

namespace RevitAxonometryViews.Views {
    public partial class MainWindow {
        private readonly MainViewModel _viewModel;
        internal ObservableCollection<HvacSystem> Items;
        protected CollectionViewSource SystemsCollection;

        internal MainWindow(MainViewModel viewModel) {
            InitializeComponent();
            filterCriterion.ItemsSource = new List<string>() {
                AxonometryConfig.SystemName, 
                AxonometryConfig.FopVisSystemName 
            };
            filterCriterion.SelectedIndex = 0;

            _viewModel = viewModel;
            Items = viewModel.GetDataSource();
            lvSystems.ItemsSource = Items;

        }

        private void FilterUpdated(object sender, TextChangedEventArgs e) {
            Items = _viewModel.GetDataSource();
            if(!string.IsNullOrEmpty(filter.Text)){
                Items = _viewModel.UpdateDataSourceByFilter(Items, filter.Text, filterCriterion.Text);
            }
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
