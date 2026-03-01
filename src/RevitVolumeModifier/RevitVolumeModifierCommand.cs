using System.Globalization;
using System.Reflection;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectConfigs;
using dosymep.Bim4Everyone.SimpleServices;
using dosymep.WpfCore.Ninject;
using dosymep.WpfUI.Core.Ninject;

using Ninject;

using RevitVolumeModifier.Interfaces;
using RevitVolumeModifier.Models;
using RevitVolumeModifier.Services;
using RevitVolumeModifier.ViewModels;
using RevitVolumeModifier.Views;

namespace RevitVolumeModifier;

[Transaction(TransactionMode.Manual)]
public class RevitVolumeModifierCommand : BasePluginCommand {

    public RevitVolumeModifierCommand() {
        PluginName = "Объединить объемные элементы";
    }

    protected override void Execute(UIApplication uiApplication) {
        // Создание контейнера зависимостей плагина с сервисами из платформы
        using var kernel = uiApplication.CreatePlatformServices();

        // Настройка доступа к Revit
        kernel.Bind<RevitRepository>()
            .ToSelf()
            .InSingletonScope();

        // Настройка доступа к сервису проверки параметров
        kernel.Bind<IParamAvailabilityService>()
            .To<ParamAvailabilityService>()
            .InSingletonScope();

        // Настройка конфигурации плагина
        kernel.Bind<PluginConfig>()
            .ToMethod(c => PluginConfig.GetPluginConfig(c.Kernel.Get<IConfigSerializer>()));

        // Используем сервис обновления тем для WinUI
        kernel.UseWpfUIThemeUpdater();

        // Настройка запуска основного окна
        kernel.BindMainWindow<MainViewModel, MainWindow>();


        // Настройка локализации,
        // получение имени сборки откуда брать текст
        string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;

        // Настройка локализации,
        // установка дефолтной локализации "ru-RU"
        kernel.UseWpfLocalization(
            $"/{assemblyName};component/assets/localization/language.xaml",
            CultureInfo.GetCultureInfo("ru-RU"));

        var window = kernel.Get<MainWindow>();
        window.Show();
    }
}
