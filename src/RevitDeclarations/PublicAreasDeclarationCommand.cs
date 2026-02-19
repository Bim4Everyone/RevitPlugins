using System.Globalization;
using System.Reflection;
using System.Windows;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SimpleServices;
using dosymep.WpfCore.Ninject;
using dosymep.WpfUI.Core.Ninject;

using Ninject;

using RevitDeclarations.Models;
using RevitDeclarations.Services;
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

        kernel.Bind<ErrorWindowService>()
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

        // Настройка локализации,
        // получение имени сборки откуда брать текст
        string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;

        // Настройка локализации,
        // установка дефолтной локализации "ru-RU"
        kernel.UseWpfLocalization(
            $"/{assemblyName};component/assets/localization/language.xaml",
            CultureInfo.GetCultureInfo("ru-RU"));

        kernel.BindMainWindow<PublicAreasMainVM, PublicAreasMainWindow>();

        Notification(kernel.Get<PublicAreasMainWindow>());
    }
}
