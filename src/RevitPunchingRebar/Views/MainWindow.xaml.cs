using System;
using System.Windows;

using dosymep.SimpleServices;

using RevitPunchingRebar.ViewModels;

namespace RevitPunchingRebar.Views;

/// <summary>
/// Класс главного окна плагина.
/// </summary>
public partial class MainWindow {
    /// <summary>
    /// Иницализирует главное окно плагина.
    /// </summary>
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

        this.Loaded += MainWindow_Loaded;
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e) {
        if(DataContext is MainViewModel vm) {
            vm.RequestHideWindow += OnRequestHideWindow;
            vm.RequestShowWindow += OnRequestShowWindow;
            vm.RequestCloseWindow += OnRequestCloseWindow;
        }
    }

    private void OnRequestCloseWindow() {
        this.Close();
    }

    private void OnRequestHideWindow() {
        this.Hide();
    }

    private void OnRequestShowWindow() {
        this.Show();
        this.Activate();
    }

    /// <summary>
    /// Наименование плагина.
    /// </summary>
    /// <remarks>
    /// Используется для сохранения положения окна.
    /// </remarks>
    public override string PluginName => nameof(RevitPunchingRebar);
    
    /// <summary>
    /// Наименование файла конфигурации.
    /// </summary>
    /// <remarks>
    /// Используется для сохранения положения окна.
    /// </remarks>
    public override string ProjectConfigName => nameof(MainWindow);

    private void ButtonOk_Click(object sender, RoutedEventArgs e) {
        DialogResult = true;
    }

    private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
        DialogResult = false;
    }
}
