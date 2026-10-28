using System.Windows;

using dosymep.SimpleServices;

using RevitVORChecker.ViewModels;

namespace RevitVORChecker.Views;

/// <summary>
/// Главное окно плагина: параметры запуска проверки.
/// </summary>
public partial class MainWindow {
    public MainWindow(
        ILoggerService loggerService,
        ISerializationService serializationService,
        ILanguageService languageService, ILocalizationService localizationService,
        IUIThemeService uiThemeService, IUIThemeUpdaterService themeUpdaterService)
        : base(loggerService,
            serializationService,
            languageService, localizationService,
            uiThemeService, themeUpdaterService) {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    /// <summary>
    /// Наименование плагина (для сохранения положения окна).
    /// </summary>
    public override string PluginName => nameof(RevitVORChecker);

    /// <summary>
    /// Наименование файла конфигурации (для сохранения положения окна).
    /// </summary>
    public override string ProjectConfigName => nameof(MainWindow);

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
        if(e.OldValue is MainViewModel oldViewModel) {
            oldViewModel.CloseRequested -= OnCloseRequested;
        }

        if(e.NewValue is MainViewModel newViewModel) {
            newViewModel.CloseRequested += OnCloseRequested;
        }
    }

    private void OnCloseRequested(object sender, EventArgs e) {
        DialogResult = true;
    }

    private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
        DialogResult = false;
    }
}
