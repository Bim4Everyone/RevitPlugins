using System.Globalization;
using System.Reflection;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectConfigs;
using dosymep.Bim4Everyone.SimpleServices;
using dosymep.WpfCore.Ninject;
using dosymep.WpfUI.Core.Ninject;

using Ninject;

using RevitArchitecturalDocumentation.Models;
using RevitArchitecturalDocumentation.Models.Options;
using RevitArchitecturalDocumentation.Models.Services;
using RevitArchitecturalDocumentation.ViewModels;
using RevitArchitecturalDocumentation.Views;

namespace RevitArchitecturalDocumentation;
[Transaction(TransactionMode.Manual)]
public class RevitCreatingARDocsCommand : BasePluginCommand {
    public RevitCreatingARDocsCommand() {
        PluginName = "Создать документацию ПСО и ДДУ";
    }

    protected override void Execute(UIApplication uiApplication) {
        using var kernel = uiApplication.CreatePlatformServices();

        kernel.Bind<RevitRepository>()
            .ToSelf()
            .InSingletonScope();

        kernel.Bind<PluginConfig>()
            .ToMethod(c => PluginConfig.GetPluginConfig(c.Kernel.Get<IConfigSerializer>()));

        // Используем сервис обновления тем для WinUI
        kernel.UseWpfUIThemeUpdater();

        // Настройка запуска окна
        kernel.BindMainWindow<CreatingARDocsVM, CreatingARDocsV>();

        // Настройка локализации,
        // получение имени сборки откуда брать текст
        string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;

        // Настройка локализации,
        // установка дефолтной локализации "ru-RU"
        kernel.UseWpfLocalization(
            $"/{assemblyName};component/assets/localization/Language.xaml",
            CultureInfo.GetCultureInfo("ru-RU"));

        kernel.Bind<MainOptions>()
            .ToSelf();
        kernel.Bind<SheetOptions>()
            .ToSelf();
        kernel.Bind<ViewOptions>()
            .ToSelf();
        kernel.Bind<SpecOptions>()
            .ToSelf();

        // Регистрируем сервис второстепенных окон
        kernel.Bind<IWindowService>()
            .To<WindowService>()
            .InSingletonScope();

        // Регистрируем окно отчета
        kernel.Bind<TreeReportV>()
            .ToSelf();

        Notification(kernel.Get<CreatingARDocsV>());
    }
}
