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
public class ApartmentsDeclarationCommand : BasePluginCommand {
    public ApartmentsDeclarationCommand() {
        PluginName = "15.2. О характеристиках жилых помещений";
    }

    protected override void Execute(UIApplication uiApplication) {
        using var kernel = uiApplication.CreatePlatformServices();
        kernel.Bind<RevitRepository>()
            .ToSelf()
            .InSingletonScope();
        kernel.Bind<ApartmentsSettings>()
            .ToSelf()
            .InSingletonScope();

        kernel.Bind<ApartmentsConfig>()
            .ToMethod(c => ApartmentsConfig.GetPluginConfig());

        kernel.Bind<ApartmentsMainVM>().ToSelf();
        kernel.Bind<ApartmentsMainWindow>().ToSelf()
            .WithPropertyValue(nameof(Window.Title), PluginName)
            .WithPropertyValue(nameof(Window.DataContext), c => c.Kernel.Get<ApartmentsMainVM>());

        Notification(kernel.Get<ApartmentsMainWindow>());
    }
}
