﻿using System.Windows;

using dosymep.SimpleServices;

using Wpf.Ui.Abstractions;

namespace RevitPlatformSettings.Views {
    public partial class MainWindow {
        public MainWindow(
            INavigationViewPageProvider navigationViewPageProvider,
            ILoggerService loggerService,
            ISerializationService serializationService,
            ILanguageService languageService, ILocalizationService localizationService,
            IUIThemeService uiThemeService, IUIThemeUpdaterService themeUpdaterService)
            : base(loggerService,
                serializationService,
                languageService, localizationService,
                uiThemeService, themeUpdaterService) {
            InitializeComponent();
            _rootNavigationView.SetPageProviderService(navigationViewPageProvider);
        }
        
        public override string PluginName => nameof(RevitPlatformSettings);
        public override string ProjectConfigName => nameof(MainWindow);
        
        private void ButtonOk_OnClick(object sender, RoutedEventArgs e) {
            DialogResult = true;
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
            DialogResult = false;
        }
    }
}
