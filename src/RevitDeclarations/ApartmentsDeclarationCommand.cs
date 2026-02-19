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
using Ninject.Activation;

using RevitDeclarations.Models;
using RevitDeclarations.Services;
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

        kernel.Bind<DeclarationApartPage>()
            .ToSelf()
            .InSingletonScope();
        kernel.Bind<PrioritiesPage>()
            .ToSelf()
            .InSingletonScope();
        kernel.Bind<ParamsApartmentsPage>()
            .ToSelf()
            .InSingletonScope();

        kernel.Bind<ErrorWindowService>()
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

        // Настройка локализации,
        // получение имени сборки откуда брать текст
        string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;

        // Настройка локализации,
        // установка дефолтной локализации "ru-RU"
        kernel.UseWpfLocalization(
            $"/{assemblyName};component/assets/localization/language.xaml",
            CultureInfo.GetCultureInfo("ru-RU"));

        kernel.BindMainWindow<ApartmentsMainVM, ApartmentsMainWindow>();

        Notification(kernel.Get<ApartmentsMainWindow>());
    }
}
