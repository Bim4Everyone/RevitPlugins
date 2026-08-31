using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;

using RevitLintelsManager.ViewModels;
using RevitLintelsManager.Views.Pages;

using Wpf.Ui.Abstractions;

namespace RevitLintelsManager.Views;

/// <summary>
/// Класс главного окна плагина.
/// </summary>
public partial class SettingsWindow {
    /// <summary>
    /// Иницализирует главное окно плагина.
    /// </summary>
    public SettingsWindow(INavigationViewPageProvider navigationViewPageProvider,
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
            _rootNavigationView.Navigate(typeof(FamilySettingsPage));
        });
    }
    
    public override string PluginName => nameof(RevitLintelsManager);
    
    public override string ProjectConfigName => nameof(SettingsWindow);
    
    private void ButtonOk_Click(object sender, RoutedEventArgs e) {
        DialogResult = true;
    }

    private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
        DialogResult = false;
    }
}
