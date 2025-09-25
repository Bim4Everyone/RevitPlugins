using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.SimpleServices;

using RevitLintelPlacement.Models;
using RevitLintelPlacement.ViewModels;
using RevitLintelPlacement.Views;

namespace RevitLintelPlacement;

[Transaction(TransactionMode.Manual)]
public class ConfigurateLintelPluginCommand : BasePluginCommand {
    public ConfigurateLintelPluginCommand() {
        PluginName = "Расстановщик перемычек";
    }

    protected override void Execute(UIApplication uiApplication) {
        var lintelsConfig = LintelsConfig.GetLintelsConfig();
        var revitRepository = new RevitRepository(
            uiApplication.Application,
            uiApplication.ActiveUIDocument.Document,
            lintelsConfig);

        var configViewModel = new ConfigViewModel(revitRepository);
        var window = new LintelsConfigView { DataContext = configViewModel };
        if(window.ShowDialog() == true) {
            GetPlatformService<INotificationService>()
                .CreateNotification(PluginName, "Выполнение скрипта завершено успешно.", "C#")
                .ShowAsync();
        } else {
            GetPlatformService<INotificationService>()
                .CreateWarningNotification(PluginName, "Выполнение скрипта отменено.")
                .ShowAsync();
        }
    }
}
