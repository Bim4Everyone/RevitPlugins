using System.Globalization;
using System.Reflection;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectConfigs;
using dosymep.Bim4Everyone.SimpleServices;
using dosymep.WpfCore.Ninject;
using dosymep.WpfUI.Core.Ninject;
using dosymep.Xpf.Core.Ninject;

using Ninject;

using RevitSleeves.Models;
using RevitSleeves.Models.Config;
using RevitSleeves.Services.Settings;
using RevitSleeves.ViewModels.Settings;
using RevitSleeves.Views.Settings;

namespace RevitSleeves;

[Transaction(TransactionMode.Manual)]
public class SleevesSettingsCommand : BasePluginCommand {
    public SleevesSettingsCommand() {
        PluginName = "Настройки расстановки гильз";
    }

    protected override void Execute(UIApplication uiApplication) {
        using var kernel = uiApplication.CreatePlatformServices();

        kernel.Bind<RevitRepository>()
            .ToSelf()
            .InSingletonScope();
        kernel.Bind<IFilterChecker>()
            .To<FilterChecker>()
            .InSingletonScope();
        kernel.Bind<FilterCheckerViewModel>()
            .ToSelf()
            .InTransientScope();
        kernel.Bind<FilterCheckerWindow>()
            .ToSelf()
            .InTransientScope();


        kernel.Bind<SleevePlacementSettingsConfig>()
            .ToMethod(c => SleevePlacementSettingsConfig.GetPluginConfig(c.Kernel.Get<IConfigSerializer>()));

        kernel.UseWpfUIThemeUpdater();

        kernel.BindMainWindow<SleevePlacementSettingsViewModel, SleevePlacementSettingsWindow>();
        kernel.UseXtraOpenFileDialog<SleevePlacementSettingsViewModel>(addExtension: true, filter: "|*.json");
        kernel.UseXtraSaveFileDialog<SleevePlacementSettingsViewModel>(addExtension: true, filter: "|*.json");
        kernel.UseWpfUIMessageBox<SleevePlacementSettingsViewModel>();

        string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;

        kernel.UseWpfLocalization(
            $"/{assemblyName};component/assets/localization/language.xaml",
            CultureInfo.GetCultureInfo("ru-RU"));

        Notification(kernel.Get<SleevePlacementSettingsWindow>());
    }
}
