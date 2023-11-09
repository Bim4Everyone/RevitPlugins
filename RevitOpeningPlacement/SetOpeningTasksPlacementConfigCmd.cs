using System.Windows.Interop;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;

using RevitOpeningPlacement.Models;
using RevitOpeningPlacement.Models.Configs;
using RevitOpeningPlacement.ViewModels.OpeningConfig;
using RevitOpeningPlacement.Views;

namespace RevitOpeningPlacement {
    /// <summary>
    /// Команда для задания настроек расстановки заданий на отверстия в файле ВИС
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    public class SetOpeningTasksPlacementConfigCmd : BasePluginCommand {
        public SetOpeningTasksPlacementConfigCmd() {
            PluginName = "Настройки расстановки заданий";
        }


        public void ExecuteCommand(UIApplication uiApplication) {
            Execute(uiApplication);
        }


        protected override void Execute(UIApplication uiApplication) {
            RevitRepository revitRepository = new RevitRepository(uiApplication.Application, uiApplication.ActiveUIDocument.Document);
            var openingConfig = OpeningConfig.GetOpeningConfig(revitRepository.Doc);
            var viewModel = new MainViewModel(revitRepository, openingConfig);

            var window = new MainWindow() { Title = PluginName, DataContext = viewModel };
            var helper = new WindowInteropHelper(window) { Owner = uiApplication.MainWindowHandle };

            window.ShowDialog();
        }
    }
}
