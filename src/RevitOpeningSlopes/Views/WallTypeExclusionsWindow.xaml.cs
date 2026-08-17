using System;
using System.Windows;

using RevitOpeningSlopes.ViewModels;

namespace RevitOpeningSlopes.Views {
    internal partial class WallTypeExclusionsWindow {
        public WallTypeExclusionsWindow(WallTypeExclusionsViewModel viewModel) {
            InitializeComponent();
            ViewModel = viewModel
                ?? throw new ArgumentNullException(nameof(viewModel));
            DataContext = ViewModel;
        }

        public override string PluginName => nameof(RevitOpeningSlopes);
        public override string ProjectConfigName => nameof(WallTypeExclusionsWindow);
        internal WallTypeExclusionsViewModel ViewModel { get; }
        internal bool SelectionInModelRequested { get; private set; }

        private void ButtonSelectInModel_Click(object sender, RoutedEventArgs e) {
            ViewModel.Save();
            SelectionInModelRequested = true;
            DialogResult = false;
        }

        private void ButtonOk_Click(object sender, RoutedEventArgs e) {
            ViewModel.Save();
            DialogResult = true;
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
            DialogResult = false;
        }
    }
}
