using System.Windows;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SimpleServices;
using dosymep.WpfCore.Ninject;
using dosymep.WpfUI.Core.Ninject;

using Ninject;
using Ninject.Activation;

using RevitDeclarations.Models;
using RevitDeclarations.ViewModels;
using RevitDeclarations.Views;

using Wpf.Ui.Abstractions;

namespace RevitDeclarations;
[Transaction(TransactionMode.Manual)]
public class ApartmentsDeclarationCommand : BasePluginCommand {
    public ApartmentsDeclarationCommand() {
        PluginName = "15.2. О характеристиках жилых помещений";
    }

    protected override void Execute(UIApplication uiApplication) {
        using var kernel = uiApplication.CreatePlatformServices();

        kernel.Bind<DeclarationTabItem>()
            .ToSelf()
            .InSingletonScope();
        kernel.Bind<PrioritiesTabItem>()
            .ToSelf()
            .InSingletonScope();
        kernel.Bind<ParamsApartmentsTabItem>()
            .ToSelf()
            .InSingletonScope();


        kernel.Bind<RevitRepository>()
            .ToSelf()
            .InSingletonScope();
        kernel.Bind<ApartmentsSettings>()
            .ToSelf()
            .InSingletonScope();

        kernel.Bind<ApartmentsConfig>()
            .ToMethod(c => ApartmentsConfig.GetPluginConfig());

        kernel.Bind<INavigationViewPageProvider>()
            .To<NavigationViewPageProvider>()
            .InSingletonScope();
        
        // Используем сервис обновления тем для WinUI
        kernel.UseWpfUIThemeUpdater();

        kernel.Bind<MainViewModel>().To<ApartmentsMainVM>();
        kernel.BindMainWindow<ApartmentsMainVM, ApartmentsMainWindow>();
        
        Notification(kernel.Get<ApartmentsMainWindow>());
    }
}
