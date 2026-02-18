using System.Windows;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SimpleServices;
using dosymep.WpfCore.Ninject;
using dosymep.WpfUI.Core.Ninject;

using Ninject;

using RevitDeclarations.Models;
using RevitDeclarations.ViewModels;
using RevitDeclarations.Views;

using Wpf.Ui.Abstractions;

namespace RevitDeclarations;
[Transaction(TransactionMode.Manual)]
public class PublicAreasDeclarationCommand : BasePluginCommand {
    public PublicAreasDeclarationCommand() {
        PluginName = "16.1. О помещениях общего пользования";
    }

    protected override void Execute(UIApplication uiApplication) {
        using var kernel = uiApplication.CreatePlatformServices();

        kernel.Bind<DeclarationPublicAreasPage>()
            .ToSelf()
            .InSingletonScope();
        kernel.Bind<ParamsPublicAreasPage>()
            .ToSelf()
            .InSingletonScope();

        kernel.Bind<RevitRepository>()
            .ToSelf()
            .InSingletonScope();
        kernel.Bind<PublicAreasSettings>()
            .ToSelf()
            .InSingletonScope();

        kernel.Bind<PublicAreasConfig>()
            .ToMethod(c => PublicAreasConfig.GetPluginConfig());

        kernel.Bind<INavigationViewPageProvider>()
            .To<NavigationViewPageProvider>()
            .InSingletonScope();

        // Используем сервис обновления тем для WinUI
        kernel.UseWpfUIThemeUpdater();

        kernel.BindMainWindow<PublicAreasMainVM, PublicAreasMainWindow>();

        Notification(kernel.Get<PublicAreasMainWindow>());
    }
}
