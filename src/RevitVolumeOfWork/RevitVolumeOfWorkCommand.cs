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
        
        // Используем сервис обновления тем для WinUI
        kernel.UseWpfUIThemeUpdater();

        kernel.BindMainWindow<MainViewModel, MainWindow>();

        UpdateParams(uiApplication);

        Notification(kernel.Get<MainWindow>());
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
