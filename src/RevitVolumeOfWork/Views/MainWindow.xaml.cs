using System.Windows;

using dosymep.SimpleServices;

using RevitVolumeOfWork.ViewModels;

using Wpf.Ui.Controls;

namespace RevitVolumeOfWork.Views {
    public partial class MainWindow {
        public MainWindow(ILoggerService loggerService,
                          ISerializationService serializationService,
                          ILanguageService languageService, ILocalizationService localizationService,
                          IUIThemeService uiThemeService, IUIThemeUpdaterService themeUpdaterService)         
            : base(loggerService,
                   serializationService,
                   languageService, localizationService,
                   uiThemeService, themeUpdaterService) {
            InitializeComponent();
        }

        public override string PluginName => nameof(RevitVolumeOfWork);
        public override string ProjectConfigName => nameof(MainWindow);

        private void ButtonOk_Click(object sender, RoutedEventArgs e) {
            DialogResult = true;
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
            DialogResult = false;
        }
        
        
        private void CheckBox_Checked(object sender, RoutedEventArgs e) {
            ChangeSelected(true);
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e) {
            ChangeSelected(false);
        }

        private void ChangeSelected(bool state) {
            var dataGrid = (DataGrid) FindName("Levels");
            var levels = dataGrid.SelectedItems;
            foreach(LevelViewModel level in levels) {
                level.IsSelected = state;
            }
        }
    }
}
