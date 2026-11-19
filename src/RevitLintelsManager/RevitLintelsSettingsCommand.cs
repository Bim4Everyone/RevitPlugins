using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SimpleServices;

using Ninject;

using RevitLintelsManager.Services;

namespace RevitLintelsManager;

[Transaction(TransactionMode.Manual)]
public class RevitLintelsSettingsCommand : BasePluginCommand {
    /// <summary>Создаёт самостоятельную команду настроек.</summary>
    public RevitLintelsSettingsCommand() {
        PluginName = "Менеджер перемычек";
    }

    /// <summary>Открывает настройки модально с отдельным контейнером.</summary>
    protected override void Execute(UIApplication uiApplication) {
        using IKernel kernel = uiApplication.CreatePlatformServices();
        RevitLintelsManagerCommand.ConfigureDependencies(kernel);
        RevitLintelsManagerCommand.ConfigureSettingsDependencies(kernel);
        SettingsPlatformServices.ShowSettingsWindow(kernel);
    }
}
