using System.Windows.Interop;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SimpleServices;

using Ninject;

using RevitClashDetective.Models.GraphicView;
using RevitClashDetective.Models.Handlers;

using RevitOpeningPlacement.Models;
using RevitOpeningPlacement.Models.Configs;
using RevitOpeningPlacement.ViewModels.OpeningConfig;

using RevitOpeningPlacement.Views;

namespace RevitOpeningPlacement;
/// <summary>
/// Команда для задания настроек расстановки чистовых отверстий в файле АР
/// </summary>
[Transaction(TransactionMode.Manual)]
internal class SetOpeningRealsArPlacementConfigCmd : BasePluginCommand {
    public SetOpeningRealsArPlacementConfigCmd() {
        PluginName = "Настройки расстановки отверстий АР";
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

        var revitRepository = kernel.Get<RevitRepository>();
        var openingConfig = OpeningRealsArConfig.GetOpeningConfig(revitRepository.Doc);
        var viewModel = new OpeningRealsArConfigViewModel(revitRepository, openingConfig);

        var window = new OpeningRealsArSettingsView() { Title = PluginName, DataContext = viewModel };
        var helper = new WindowInteropHelper(window) { Owner = uiApplication.MainWindowHandle };

        window.ShowDialog();
    }
}
