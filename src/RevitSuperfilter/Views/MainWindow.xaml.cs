using System.Windows;

using Autodesk.Revit.UI;

using dosymep.SimpleServices;

using Ninject;

namespace RevitSuperfilter.Views;

/// <summary>
/// Класс главного окна плагина.
/// </summary>
public partial class MainWindow {
    private readonly IKernel _kernel;
    private readonly ExternalEvent _externalEvent;

    /// <summary>
    /// Инициализирует главное окно плагина.
    /// </summary>
    public MainWindow(
        IKernel kernel,
        ILoggerService loggerService,
        ISerializationService serializationService,
        ILanguageService languageService,
        ILocalizationService localizationService,
        IUIThemeService uiThemeService,
        IUIThemeUpdaterService themeUpdaterService)
        : base(
            loggerService,
            serializationService,
            languageService,
            localizationService,
            uiThemeService,
            themeUpdaterService) {
        _kernel = kernel;
        InitializeComponent();

        _externalEvent = ExternalEvent.Create(new ExternalEventHandler(kernel));
    }

    /// <summary>
    /// Наименование плагина.
    /// </summary>
    /// <remarks>
    /// Используется для сохранения положения окна.
    /// </remarks>
    public override string PluginName => nameof(RevitSuperfilter);

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

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e) {
        base.OnClosing(e);
        _externalEvent.Raise();
    }
}

internal class ExternalEventHandler : IExternalEventHandler {
    private readonly IKernel _kernel;

    public ExternalEventHandler(IKernel kernel) {
        _kernel = kernel;
    }

    public void Execute(UIApplication app) {
        _kernel.Dispose();
    }

    public string GetName() {
        return nameof(ExternalEventHandler);
    }
}
