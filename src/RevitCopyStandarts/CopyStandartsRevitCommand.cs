#region Namespaces

using System.Globalization;
using System.IO;
using System.Reflection;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectConfigs;
using dosymep.Bim4Everyone.SimpleServices;
using dosymep.WpfCore.Ninject;
using dosymep.WpfUI.Core.Ninject;

using Ninject;

using RevitCopyStandarts.Models;
using RevitCopyStandarts.Services;
using RevitCopyStandarts.ViewModels;
using RevitCopyStandarts.Views;

#endregion

namespace RevitCopyStandarts;

[Transaction(TransactionMode.Manual)]
public class CopyStandartsRevitCommand : BasePluginCommand {
    public CopyStandartsRevitCommand() {
        PluginName = "Копирование стандартов";
    }

    protected override void Execute(UIApplication uiApplication) {
        // Создание контейнера зависимостей плагина с сервисами из платформы
        using IKernel kernel = uiApplication.CreatePlatformServices();
        
        // Настройка доступа к Revit
        kernel.Bind<RevitRepository>()
            .ToSelf()
            .InSingletonScope();

        // Настройка конфигурации плагина
        kernel.Bind<PluginConfig>()
            .ToMethod(c => PluginConfig.GetPluginConfig(c.Kernel.Get<IConfigSerializer>()));

        // Используем сервис обновления тем для WinUI
        kernel.UseWpfUIThemeUpdater();

        // Настройка запуска окна
        kernel.BindMainWindow<MainViewModel, MainWindow>();

        // Настройка локализации,
        // получение имени сборки откуда брать текст
        string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;

        // Настройка локализации,
        // установка дефолтной локализации "ru-RU"
        kernel.UseWpfLocalization(
            $"/{assemblyName};component/assets/localizations/Language.xaml",
            CultureInfo.GetCultureInfo("ru-RU"));

        kernel.UseWpfUIProgressDialog<MainViewModel>(stepValue: 1);


        kernel.Bind<IStandartsService>().To<StandartsService>();

        // Вызывает стандартное уведомление
        kernel.Get<MainWindow>().ShowDialog();
    }
}
