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
public class CommercialDeclarationCommand : BasePluginCommand {
    public CommercialDeclarationCommand() {
        PluginName = "15.3. О характеристиках нежилых помещений";
    }

    protected override void Execute(UIApplication uiApplication) {
        using var kernel = uiApplication.CreatePlatformServices();
        kernel.Bind<RevitRepository>()
            .ToSelf()
            .InSingletonScope();
        kernel.Bind<CommercialSettings>()
            .ToSelf()
            .InSingletonScope();

        kernel.Bind<CommercialConfig>()
            .ToMethod(c => CommercialConfig.GetPluginConfig());

        kernel.Bind<CommercialMainVM>().ToSelf();
        kernel.Bind<CommercialMainWindow>().ToSelf()
            .WithPropertyValue(nameof(Window.Title), PluginName)
            .WithPropertyValue(nameof(Window.DataContext), c => c.Kernel.Get<CommercialMainVM>());

        Notification(kernel.Get<CommercialMainWindow>());
    }
}
