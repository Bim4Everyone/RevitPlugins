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

namespace RevitOpeningPlacement {
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
            using(IKernel kernel = uiApplication.CreatePlatformServices()) {
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
                var openingConfig = OpeningRealsKrConfig.GetOpeningConfig(revitRepository.Doc);
                var viewModel = new OpeningRealsKrConfigViewModel(revitRepository, openingConfig);

                var window = new OpeningRealsKrSettingsView() { Title = PluginName, DataContext = viewModel };
                var helper = new WindowInteropHelper(window) { Owner = uiApplication.MainWindowHandle };

                window.ShowDialog();
            }
        }
    }
}
