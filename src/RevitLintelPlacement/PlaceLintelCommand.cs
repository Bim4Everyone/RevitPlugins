using System;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;

using RevitLintelPlacement.Models;
using RevitLintelPlacement.ViewModels;
using RevitLintelPlacement.Views;

namespace RevitLintelPlacement;

[Transaction(TransactionMode.Manual)]
public class PlaceLintelCommand : BasePluginCommand {
    public PlaceLintelCommand() {
        PluginName = "Расстановщик перемычек";
    }

    protected override void Execute(UIApplication uiApplication) {
        var activeView = uiApplication.ActiveUIDocument.ActiveGraphicalView;
        if(!(activeView.ViewType == ViewType.ThreeD
             || activeView.ViewType == ViewType.Schedule
             || activeView.ViewType == ViewType.FloorPlan)) {
            throw new Exception("Откройте 3Д вид, план этажа или спецификацию.");
        }

        var lintelsConfig = LintelsConfig.GetLintelsConfig();
        var revitRepository = new RevitRepository(
            uiApplication.Application,
            uiApplication.ActiveUIDocument.Document,
            lintelsConfig);

        CheckConfig(revitRepository.LintelsCommonConfig);

        var mainViewModel = new MainViewModel(revitRepository);
        var window = new MainWindow { DataContext = mainViewModel };
        Notification(window);
    }

    private void CheckConfig(LintelsCommonConfig lintelsConfig) {
        if(lintelsConfig.IsEmpty()) {
            throw new Exception("Необходимо заполнить настройки плагина.");
        }
    }
}
