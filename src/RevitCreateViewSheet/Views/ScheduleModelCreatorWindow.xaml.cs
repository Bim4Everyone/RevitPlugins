using System.Windows;

using dosymep.SimpleServices;

namespace RevitCreateViewSheet.Views {
    public partial class ScheduleModelCreatorWindow {
        public ScheduleModelCreatorWindow(
        ILoggerService loggerService,
        ISerializationService serializationService,
        ILanguageService languageService,
        ILocalizationService localizationService,
        IUIThemeService uiThemeService,
        IUIThemeUpdaterService themeUpdaterService)
        : base(loggerService,
              serializationService,
              languageService,
              localizationService,
              uiThemeService,
              themeUpdaterService) {
            InitializeComponent();
        }

        public override string PluginName => nameof(RevitCreateViewSheet);
        public override string ProjectConfigName => nameof(ScheduleModelCreatorWindow);

        private void ButtonOk_Click(object sender, RoutedEventArgs e) {
            DialogResult = true;
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
            DialogResult = false;
        }
    }
}
