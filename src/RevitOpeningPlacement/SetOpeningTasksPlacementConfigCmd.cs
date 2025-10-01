using System.Windows;
using System.Windows.Interop;

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
using RevitOpeningPlacement.Services;
using RevitOpeningPlacement.ViewModels.OpeningConfig;
using RevitOpeningPlacement.Views;

namespace RevitOpeningPlacement;
/// <summary>
/// Команда для задания настроек расстановки заданий на отверстия в файле ВИС
/// </summary>
[Transaction(TransactionMode.Manual)]
public class SetOpeningTasksPlacementConfigCmd : BasePluginCommand {
    public SetOpeningTasksPlacementConfigCmd() {
        PluginName = "Настройки расстановки заданий";
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
        kernel.Bind<IDocTypesProvider>()
            .ToMethod(c => {
                return new DocTypesProvider(new DocTypeEnum[] { DocTypeEnum.AR, DocTypeEnum.KR });
            })
            .InSingletonScope();
        kernel.Bind<IRevitLinkTypesSetter>()
            .To<DocTypeLoadedLinksSetter>()
            .InTransientScope();
        kernel.Bind<IDocTypesHandler>()
            .To<DocTypesHandler>()
            .InSingletonScope();
        kernel.Bind<MainViewModel>()
            .ToSelf()
            .InSingletonScope();
        kernel.Bind<MainWindow>()
            .ToSelf()
            .WithPropertyValue(nameof(Window.Title), PluginName)
            .WithPropertyValue(nameof(Window.DataContext),
                c => c.Kernel.Get<MainViewModel>());
        kernel.Bind<WindowInteropHelper>()
            .ToConstructor(c => new WindowInteropHelper(kernel.Get<MainWindow>()))
            .WithPropertyValue(nameof(Window.Owner), uiApplication.MainWindowHandle);
        kernel.Bind<OpeningConfig>()
            .ToMethod(c =>
                OpeningConfig.GetOpeningConfig(uiApplication.ActiveUIDocument.Document)
            );
        kernel.Bind<ConfigFileService>()
            .ToSelf()
            .InSingletonScope();

        kernel.Get<IRevitLinkTypesSetter>().SetRevitLinkTypes();

        Notification(kernel.Get<MainWindow>());
    }
}
