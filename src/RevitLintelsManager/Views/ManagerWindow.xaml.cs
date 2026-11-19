using dosymep.SimpleServices;

using RevitLintelsManager.Services;

namespace RevitLintelsManager.Views;

public partial class ManagerWindow {
    /// <summary>Создаёт немодальное окно с отсоединяемыми платформенными событиями.</summary>
    public ManagerWindow(ILoggerService loggerService, ISerializationService serializationService,
        WindowPlatformEvents platformEvents, ILocalizationService localizationService,
        IUIThemeUpdaterService themeUpdaterService)
        : base(loggerService, serializationService, platformEvents, localizationService,
            platformEvents, themeUpdaterService) {
        InitializeComponent();
    }

    public override string PluginName => nameof(RevitLintelsManager);
    public override string ProjectConfigName => nameof(ManagerWindow);
}
