using System.Windows.Interop;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;

using RevitOpeningPlacement.Models;
using RevitOpeningPlacement.Models.Configs;
using RevitOpeningPlacement.ViewModels.OpeningConfig;
using RevitOpeningPlacement.Views;

namespace RevitOpeningPlacement {
    /// <summary>
    /// Команда для задания настроек расстановки чистовых отверстий в файле КР
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    internal class SetOpeningRealsPlacementConfigCmd : BasePluginCommand {
        public SetOpeningRealsPlacementConfigCmd() {
            PluginName = "Настройки расстановки отверстий КР";
        }


        public void ExecuteCommand(UIApplication uiApplication) {
            Execute(uiApplication);
        }


        protected override void Execute(UIApplication uiApplication) {
            RevitRepository revitRepository = new RevitRepository(uiApplication.Application, uiApplication.ActiveUIDocument.Document);
            var openingConfig = OpeningRealsKrConfig.GetOpeningConfig(revitRepository.Doc);
            var viewModel = new OpeningRealsKrConfigViewModel(revitRepository, openingConfig);

            var window = new OpeningRealsKrSettingsView() { Title = PluginName, DataContext = viewModel };
            var helper = new WindowInteropHelper(window) { Owner = uiApplication.MainWindowHandle };

            window.ShowDialog();
        }
    }
}
