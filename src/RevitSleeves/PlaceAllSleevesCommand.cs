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

using RevitSleeves.Models;
using RevitSleeves.Models.Config;
using RevitSleeves.Services.Placing;

namespace RevitSleeves;
[Transaction(TransactionMode.Manual)]
internal class PlaceAllSleevesCommand : BasePluginCommand {
    public PlaceAllSleevesCommand() {
        PluginName = "Расставить все гильзы";
    }


    protected override void Execute(UIApplication uiApplication) {
        using var kernel = uiApplication.CreatePlatformServices();

        kernel.Bind<RevitRepository>()
            .ToSelf()
            .InSingletonScope();
        kernel.Bind<SleevePlacementSettingsConfig>()
            .ToMethod(c => SleevePlacementSettingsConfig.GetPluginConfig(c.Kernel.Get<IConfigSerializer>()));

        BindCoreServices(kernel);

        kernel.UseWpfUIThemeUpdater();

        string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;

        kernel.UseWpfLocalization(
            $"/{assemblyName};component/assets/localization/language.xaml",
            CultureInfo.GetCultureInfo("ru-RU"));

        Notification(true);
    }

    private void BindCoreServices(IKernel kernel) {
        kernel.Bind<ISleevePlacingOptsService>()
            .To<SleevePlacingOptsService>()
            .InSingletonScope();
        kernel.Bind<ISleevePlacerService>()
            .To<SleevePlacerService>()
            .InSingletonScope();
        kernel.Bind<ISleeveCleanupService>()
            .To<SleeveCleanupService>()
            .InSingletonScope();
        kernel.Bind<ISleeveMergeService>()
            .To<SleeveMergeService>()
            .InSingletonScope();
    }
}
