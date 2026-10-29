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

using RevitStairRailing.Models;
using RevitStairRailing.Services;
using RevitStairRailing.ViewModels;
using RevitStairRailing.Views;

namespace RevitStairRailing;

/// <summary>
/// Команда «Ограждение лестниц»: ограждение КМ из семейств «Каркас несущий» по линиям модели траектории.
/// </summary>
[Transaction(TransactionMode.Manual)]
public class RevitStairRailingCommand : BasePluginCommand {
    /// <summary>
    /// Инициализирует команду плагина.
    /// </summary>
    public RevitStairRailingCommand() {
        PluginName = "Ограждение лестниц";
    }

    /// <summary>
    /// Метод выполнения основного кода плагина.
    /// </summary>
    /// <param name="uiApplication">Интерфейс взаимодействия с Revit.</param>
    protected override void Execute(UIApplication uiApplication) {
        using IKernel kernel = uiApplication.CreatePlatformServices();

        kernel.Bind<RevitRepository>()
            .ToSelf()
            .InSingletonScope();
        kernel.Bind<RailingPlacement>()
            .ToSelf()
            .InSingletonScope();

        kernel.Bind<PluginConfig>()
            .ToMethod(c => PluginConfig.GetPluginConfig(c.Kernel.Get<IConfigSerializer>()));

        kernel.UseWpfUIThemeUpdater();

        kernel.BindMainWindow<MainViewModel, MainWindow>();

        string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
        kernel.UseWpfLocalization(
            $"/{assemblyName};component/assets/localization/language.xaml",
            CultureInfo.GetCultureInfo("ru-RU"));

        // окно закрывается до выбора линий: выбор в Revit идёт без открытого модального окна
        var mainWindow = kernel.Get<MainWindow>();
        if(mainWindow.ShowDialog() == true) {
            kernel.Get<RailingPlacement>().Run(((MainViewModel) mainWindow.DataContext).Parameters);
        }
    }
}
