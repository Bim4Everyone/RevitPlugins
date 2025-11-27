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

using RevitParamsChecker.Models;
using RevitParamsChecker.Models.Rules;
using RevitParamsChecker.ViewModels;
using RevitParamsChecker.Views;

namespace RevitParamsChecker;

[Transaction(TransactionMode.Manual)]
public class RevitParamsCheckerCommand : BasePluginCommand {
    public RevitParamsCheckerCommand() {
        PluginName = "Проверка параметров";
    }

    protected override void Execute(UIApplication uiApplication) {
        using IKernel kernel = uiApplication.CreatePlatformServices();

        kernel.Bind<RevitRepository>()
            .ToSelf()
            .InSingletonScope();

        kernel.UseWpfUIThemeUpdater();

        kernel.BindMainWindow<MainViewModel, MainWindow>();

        string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;

        kernel.UseWpfLocalization(
            $"/{assemblyName};component/assets/localization/language.xaml",
            CultureInfo.GetCultureInfo("ru-RU"));

        Notification(kernel.Get<MainWindow>());
    }
}
