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
public class CommercialDeclarationCommand : BasePluginCommand {
    public CommercialDeclarationCommand() {
        PluginName = "15.3. О характеристиках нежилых помещений";
    }

    protected override void Execute(UIApplication uiApplication) {
        using var kernel = uiApplication.CreatePlatformServices();

        kernel.Bind<DeclarationCommercialPage>()
            .ToSelf()
            .InSingletonScope();
        kernel.Bind<ParamsCommercialPage>()
            .ToSelf()
            .InSingletonScope();

        kernel.Bind<RevitRepository>()
            .ToSelf()
            .InSingletonScope();
        kernel.Bind<CommercialSettings>()
            .ToSelf()
            .InSingletonScope();

        kernel.Bind<CommercialConfig>()
            .ToMethod(c => CommercialConfig.GetPluginConfig());

        //kernel.Bind<CommercialMainVM>().ToSelf();
        //kernel.Bind<CommercialMainWindow>().ToSelf()
        //    .WithPropertyValue(nameof(Window.Title), PluginName)
        //    .WithPropertyValue(nameof(Window.DataContext), c => c.Kernel.Get<CommercialMainVM>());

        kernel.Bind<INavigationViewPageProvider>()
            .To<NavigationViewPageProvider>()
            .InSingletonScope();

        // Используем сервис обновления тем для WinUI
        kernel.UseWpfUIThemeUpdater();

        kernel.BindMainWindow<CommercialMainVM, CommercialMainWindow>();

        Notification(kernel.Get<CommercialMainWindow>());
    }
}
