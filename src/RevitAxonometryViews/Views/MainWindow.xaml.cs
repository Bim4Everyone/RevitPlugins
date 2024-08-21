using System.Collections.ObjectModel;
using System.Windows;

using RevitAxonometryViews.Models;
using RevitAxonometryViews.ViewModels;

namespace RevitAxonometryViews.Views {
    public partial class MainWindow {
        private readonly MainViewModel _viewModel;

        internal MainWindow(MainViewModel viewModel) {
            InitializeComponent();
            _viewModel = viewModel;
            lvSystems.ItemsSource = viewModel.GetDataSource();
        }

        //public override string PluginName => nameof(RevitAxonometryViews);
        //public override string ProjectConfigName => nameof(MainWindow);

        //private void ButtonOk_Click(object sender, RoutedEventArgs e) {
        //    DialogResult = true;
        //}

        //private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
        //    DialogResult = false;
        //}
    }
}
