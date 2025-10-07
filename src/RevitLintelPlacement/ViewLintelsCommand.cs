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
public class ViewLintelsCommand : BasePluginCommand {
    public ViewLintelsCommand() {
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
        if(!HasConfig(revitRepository.LintelsCommonConfig)) {
            return;
        }

        var elementInfos = new ElementInfosViewModel(revitRepository);
        var lintelsView = new LintelCollectionViewModel(revitRepository, elementInfos);
        var view = new LintelsView { DataContext = lintelsView };
        view.Show();
    }

    private bool HasConfig(LintelsCommonConfig lintelsConfig) {
        if(lintelsConfig.IsEmpty()) {
            TaskDialog.Show("BIM", "Необходимо заполнить настройки плагина");
            return false;
        }

        return true;
    }
}
