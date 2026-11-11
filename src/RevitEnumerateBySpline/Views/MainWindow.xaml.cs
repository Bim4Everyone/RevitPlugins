using System;
using System.Windows;
using System.Windows.Threading;

using dosymep.SimpleServices;

using RevitEnumerateBySpline.Views.Pages;

using Wpf.Ui.Abstractions;

namespace RevitEnumerateBySpline.Views;

internal enum MainWindowAction {
    None,
    SelectCurves
}

/// <summary>
/// Класс главного окна плагина.
/// </summary>
public partial class MainWindow {
    /// <summary>
    /// Иницализирует главное окно плагина.
    /// </summary>
    public MainWindow(
        INavigationViewPageProvider navigationViewPageProvider,
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
        
        _rootNavigationView.SetPageProviderService(navigationViewPageProvider);

        Dispatcher.BeginInvoke(DispatcherPriority.Loaded, () => {
            _rootNavigationView.Navigate(typeof(CommonSettingsPage));
        });
    }
    
    public override string PluginName => nameof(RevitEnumerateBySpline);
    
    public override string ProjectConfigName => nameof(MainWindow);

    internal MainWindowAction DialogAction { get; private set; }

    internal void RequestCurveSelection() {
        DialogAction = MainWindowAction.SelectCurves;
        DialogResult = false;
    }
    
    
    private void ButtonOk_Click(object sender, RoutedEventArgs e) {
        DialogAction = MainWindowAction.None;

        // ButtonBase raises Click before executing its bound Command. Defer closing
        // until AcceptViewCommand has saved the settings and built the run context.
        Dispatcher.BeginInvoke(
            DispatcherPriority.Normal,
            new Action(() => DialogResult = true));
    }

    private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
        DialogAction = MainWindowAction.None;
        DialogResult = false;
    }
}
