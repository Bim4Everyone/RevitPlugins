using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.SimpleServices;

using RevitClashDetective.Models;
using RevitClashDetective.Models.FilterModel;
using RevitClashDetective.ViewModels;
using RevitClashDetective.ViewModels.ClashDetective;
using RevitClashDetective.Views;

namespace RevitClashDetective {

    [Transaction(TransactionMode.Manual)]
    public class DetectiveClashesCommand : BasePluginCommand {
        public DetectiveClashesCommand() {
            PluginName = "Поиск коллизий";
        }

        protected override void Execute(UIApplication uiApplication) {
            ExecuteCommand(uiApplication);
        }

        public void ExecuteCommand(UIApplication uiApplication) {
            var revitRepository = new RevitRepository(uiApplication.Application, uiApplication.ActiveUIDocument.Document);
            var filterConfig = FiltersConfig.GetFiltersConfig(Path.Combine(revitRepository.GetObjectName(), revitRepository.GetDocumentName()));
            var checkConfig = ChecksConfig.GetChecksConfig(Path.Combine(revitRepository.GetObjectName(), revitRepository.GetDocumentName()));
            var mainViewModlel = new MainViewModel(checkConfig, filterConfig, revitRepository);
            var window = new MainWindow() { DataContext = mainViewModlel };
            GetPlatformService<IRootWindowService>().RootWindow = window;
            if(window.ShowDialog() == true) {
                GetPlatformService<INotificationService>()
                    .CreateNotification(PluginName, "Выполнение скрипта завершено успешно.", "C#")
                    .ShowAsync();
            } else {
                GetPlatformService<INotificationService>()
                    .CreateWarningNotification(PluginName, "Выполнение скрипта отменено.")
                    .ShowAsync();
            }
        }
    }
}
