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
using RevitClashDetective.ViewModels.FilterCreatorViewModels;
using RevitClashDetective.Views;

namespace RevitClashDetective {

    [Transaction(TransactionMode.Manual)]
    public class CreateFiltersCommand : BasePluginCommand {
        public CreateFiltersCommand() {
            PluginName = "Поиск коллизий";
        }

        protected override void Execute(UIApplication uiApplication) {
            ExecuteCommand(uiApplication);
        }

        public void ExecuteCommand(UIApplication uiApplication, string selectedFilter = null) {
            var revitRepository = new RevitRepository(uiApplication.Application, uiApplication.ActiveUIDocument.Document);

            var viewModlel = new FiltersViewModel(revitRepository, FiltersConfig.GetFiltersConfig(Path.Combine(revitRepository.GetObjectName(), revitRepository.GetDocumentName())));
            var window = new FilterCreatorView() { DataContext = viewModlel };
            if(selectedFilter != null) {
                viewModlel.SelectedFilter = viewModlel.Filters.FirstOrDefault(item => item.Name.Equals(selectedFilter, StringComparison.CurrentCultureIgnoreCase));
            }
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
