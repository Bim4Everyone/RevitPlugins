using System.Globalization;
using System.Reflection;
using System.Windows;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectParams;
using dosymep.Bim4Everyone.SimpleServices;
using dosymep.Bim4Everyone.Templates;
using dosymep.SimpleServices;
using dosymep.WpfCore.Ninject;
using dosymep.WpfUI.Core.Ninject;

using Ninject;

using RevitVolumeOfWork.Models;
using RevitVolumeOfWork.ViewModels;
using RevitVolumeOfWork.Views;

using Application = Autodesk.Revit.ApplicationServices.Application;

namespace RevitVolumeOfWork; 
[Transaction(TransactionMode.Manual)]
public class RevitVolumeOfWorkCommand : BasePluginCommand {
    public RevitVolumeOfWorkCommand() {
        PluginName = "Характеристики ВОР Кладка";
    }

    protected override void Execute(UIApplication uiApplication) {
        using IKernel kernel = uiApplication.CreatePlatformServices();

        // Настройка доступа к Revit
        kernel.Bind<RevitRepository>()
            .ToSelf()
            .InSingletonScope();

        kernel.Bind<PluginConfig>()
            .ToMethod(c => PluginConfig.GetPluginConfig());

        kernel.Bind<MainViewModel>().ToSelf();
        kernel.Bind<MainWindow>().ToSelf()
            .WithPropertyValue(nameof(Window.Title), PluginName)
            .WithPropertyValue(nameof(Window.DataContext),
                c => c.Kernel.Get<MainViewModel>());

        UpdateParams(uiApplication);

        var window = kernel.Get<MainWindow>();
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

    private static void UpdateParams(UIApplication uiApplication) {
        var projectParameters = ProjectParameters.Create(uiApplication.Application);
        projectParameters.SetupRevitParams(uiApplication.ActiveUIDocument.Document,
            ProjectParamsConfig.Instance.RelatedRoomName,
            ProjectParamsConfig.Instance.RelatedRoomNumber,
            ProjectParamsConfig.Instance.RelatedRoomID,
            ProjectParamsConfig.Instance.RelatedRoomGroup);
    }
}
