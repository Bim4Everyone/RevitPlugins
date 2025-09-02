using System.Globalization;
using System.Reflection;
using System.Windows;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectConfigs;
using dosymep.Bim4Everyone.SimpleServices;
using dosymep.WpfCore.Ninject;
using dosymep.WpfUI.Core.Ninject;

using Ninject;

using RevitShakeSpecs.Models;
using RevitShakeSpecs.ViewModels;
using RevitShakeSpecs.Views;

namespace RevitShakeSpecs;
[Transaction(TransactionMode.Manual)]
public class RevitShakeSpecsCommand : BasePluginCommand {
    public RevitShakeSpecsCommand() {
        PluginName = "Встряхнуть спецификации";
    }

    protected override void Execute(UIApplication uiApplication) {
        using var kernel = uiApplication.CreatePlatformServices();

        kernel.Bind<RevitRepository>()
            .ToSelf()
            .InSingletonScope();

        // Настройка конфигурации плагина
        kernel.Bind<PluginConfig>()
            .ToMethod(c => PluginConfig.GetPluginConfig(c.Kernel.Get<IConfigSerializer>()));

        // Используем сервис обновления тем для WinUI
        kernel.UseWpfUIThemeUpdater();

        // Настройка локализации,
        // получение имени сборки откуда брать текст
        string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;

        // Настройка локализации,
        // установка дефолтной локализации "ru-RU"
        kernel.UseWpfLocalization($"/{assemblyName};component/assets/localization/Language.xaml",
            CultureInfo.GetCultureInfo("ru-RU"));

        kernel.Bind<MainViewModel>().ToSelf();
        kernel.Bind<MainWindow>().ToSelf()
            .WithPropertyValue(nameof(Window.Title), PluginName)
            .WithPropertyValue(nameof(Window.DataContext),
                c => c.Kernel.Get<MainViewModel>());

        Notification(kernel.Get<MainWindow>());
    }
}
