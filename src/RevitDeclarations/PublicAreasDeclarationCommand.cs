using System.Windows;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SimpleServices;

using Ninject;

using RevitDeclarations.Models;
using RevitDeclarations.ViewModels;
using RevitDeclarations.Views;

namespace RevitDeclarations;
[Transaction(TransactionMode.Manual)]
public class PublicAreasDeclarationCommand : BasePluginCommand {
    public PublicAreasDeclarationCommand() {
        PluginName = "16.1. О помещениях общего пользования";
    }

    protected override void Execute(UIApplication uiApplication) {
        using var kernel = uiApplication.CreatePlatformServices();
        kernel.Bind<RevitRepository>()
            .ToSelf()
            .InSingletonScope();
        kernel.Bind<PublicAreasSettings>()
            .ToSelf()
            .InSingletonScope();

        kernel.Bind<PublicAreasConfig>()
            .ToMethod(c => PublicAreasConfig.GetPluginConfig());

        kernel.Bind<PublicAreasMainVM>().ToSelf();
        kernel.Bind<PublicAreasMainWindow>().ToSelf()
            .WithPropertyValue(nameof(Window.Title), PluginName)
            .WithPropertyValue(nameof(Window.DataContext), c => c.Kernel.Get<PublicAreasMainVM>());

        Notification(kernel.Get<PublicAreasMainWindow>());
    }
}
