using System.Globalization;
using System.Reflection;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SimpleServices;
using dosymep.WpfCore.Ninject;
using dosymep.WpfUI.Core.Ninject;

using Ninject;

using RevitClashDetective.Models.GraphicView;
using RevitClashDetective.Models.Handlers;

using RevitOpeningPlacement.Models;
using RevitOpeningPlacement.Models.Configs;
using RevitOpeningPlacement.ViewModels.OpeningConfig;
using RevitOpeningPlacement.Views.Settings;

namespace RevitOpeningPlacement;
/// <summary>
/// Команда для задания настроек расстановки чистовых отверстий в файле КР
/// </summary>
[Transaction(TransactionMode.Manual)]
internal class SetOpeningRealsKrPlacementConfigCmd : BasePluginCommand {
    public SetOpeningRealsKrPlacementConfigCmd() {
        PluginName = "Настройки расстановки отверстий КР";
    }


    public void ExecuteCommand(UIApplication uiApplication) {
        Execute(uiApplication);
    }


    protected override void Execute(UIApplication uiApplication) {
        using var kernel = uiApplication.CreatePlatformServices();
        kernel.Bind<RevitRepository>()
            .ToSelf()
            .InSingletonScope();
        kernel.Bind<RevitClashDetective.Models.RevitRepository>()
            .ToSelf()
            .InSingletonScope();
        kernel.Bind<RevitEventHandler>()
            .ToSelf()
            .InSingletonScope();
        kernel.Bind<ParameterFilterProvider>()
            .ToSelf()
            .InSingletonScope();
        kernel.Bind<OpeningRealsKrConfig>()
            .ToMethod(c =>
                    OpeningRealsKrConfig.GetOpeningConfig(uiApplication.ActiveUIDocument.Document)
                );
        kernel.BindMainWindow<OpeningRealsKrConfigViewModel, OpeningRealsKrSettingsView>();
        kernel.UseWpfUIThemeUpdater();
        string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
        kernel.UseWpfLocalization($"/{assemblyName};component/assets/localization/Language.xaml",
            CultureInfo.GetCultureInfo("ru-RU"));

        Notification(kernel.Get<OpeningRealsKrSettingsView>());
    }
}
