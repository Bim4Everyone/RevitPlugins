using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using RevitCreatingFiltersByValues.Models;

namespace RevitCreatingFiltersByValues.Views {
    public partial class MainWindow {
        public MainWindow() {
            InitializeComponent();

            //categories.Items.Filter = CategoryFilter;
            
            //categories.Items.Filter = item => String.IsNullOrEmpty(searchCategory.Text) ? true : 
            //    ((CategoryElements) item).CategoryName.IndexOf(searchCategory.Text, StringComparison.OrdinalIgnoreCase) >= 0;
        }


        //private bool CategoryFilter(object item) {
        //    if(String.IsNullOrEmpty(searchCategory.Text))
        //        return true;
        //    else
        //        return (item as CategoryElements).CategoryName.IndexOf(searchCategory.Text, StringComparison.OrdinalIgnoreCase) >= 0;
        //}

        //private void searchCategory_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e) {
        //    CollectionViewSource.GetDefaultView(categories.ItemsSource).Refresh();
        //}





        public override string PluginName => nameof(RevitCreatingFiltersByValues);
        public override string ProjectConfigName => nameof(MainWindow);

        private void ButtonOk_Click(object sender, RoutedEventArgs e) {
            DialogResult = true;
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
            DialogResult = false;
        }


        private void SelectAllValues(object sender, RoutedEventArgs e) {
            values.SelectAll();
        }
        private void UnselectAllValues(object sender, RoutedEventArgs e) {
            values.UnselectAll();
        }

        private void window_Loaded(object sender, RoutedEventArgs e) {
            expander.MaxHeight = window.ActualHeight * 0.88;
        }
    }
}