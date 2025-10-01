using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SimpleServices;

using Ninject;

using RevitClashDetective.Models.GraphicView;
using RevitClashDetective.Models.Handlers;

using RevitOpeningPlacement.Models;
using RevitOpeningPlacement.Models.Configs;

namespace RevitOpeningPlacement;
/// <summary>
/// Класс команды для объединения исходящих заданий на отверстия
/// </summary>
[Transaction(TransactionMode.Manual)]
public class UniteOpeningTasksCmd : BasePluginCommand {
    public UniteOpeningTasksCmd() {
        PluginName = "Объединение заданий";
    }


    public void ExecuteCommand(UIApplication uiApplication) {
        Execute(uiApplication);
    }


    protected override void Execute(UIApplication uiApplication) {
        using var kernel = uiApplication.CreatePlatformServices();
        kernel.Bind<RevitRepository>()
            .ToSelf()
            .InSingletonScope();
        kernel.Bind<RevitClashDetective.Models.RevitRepository>()
            .ToSelf()
            .InSingletonScope();
        kernel.Bind<RevitEventHandler>()
            .ToSelf()
            .InSingletonScope();
        kernel.Bind<ParameterFilterProvider>()
            .ToSelf()
            .InSingletonScope();

        var revitRepository = kernel.Get<RevitRepository>();
        var openingTasks = revitRepository.PickManyOpeningMepTasksOutcoming();
        var config = OpeningConfig.GetOpeningConfig(revitRepository.Doc);

        var placedOpeningTask = revitRepository.UniteOpenings(openingTasks, config);
        uiApplication.ActiveUIDocument.Selection.SetElementIds(new ElementId[] { placedOpeningTask.Id });
    }
}
