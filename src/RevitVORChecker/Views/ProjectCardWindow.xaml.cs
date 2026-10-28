using System.Windows;

using dosymep.SimpleServices;

using RevitVORChecker.ViewModels;

namespace RevitVORChecker.Views;

/// <summary>
/// Окно «Данные проекта»: сведения из ЗнП и нормы армирования (табл. 2.6.4).
/// </summary>
public partial class ProjectCardWindow {
    public ProjectCardWindow(
        ILoggerService loggerService,
        ISerializationService serializationService,
        ILanguageService languageService, ILocalizationService localizationService,
        IUIThemeService uiThemeService, IUIThemeUpdaterService themeUpdaterService)
        : base(loggerService,
            serializationService,
            languageService, localizationService,
            uiThemeService, themeUpdaterService) {
        InitializeComponent();
    }

    /// <summary>
    /// Наименование плагина (для сохранения положения окна).
    /// </summary>
    public override string PluginName => nameof(RevitVORChecker);

    /// <summary>
    /// Наименование файла конфигурации (для сохранения положения окна).
    /// </summary>
    public override string ProjectConfigName => nameof(ProjectCardWindow);

    private void ButtonSave_Click(object sender, RoutedEventArgs e) {
        if(DataContext is ProjectCardViewModel viewModel && viewModel.TryAccept()) {
            DialogResult = true;
        }
    }

    private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
        DialogResult = false;
    }
}
