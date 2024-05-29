using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep;
using dosymep.Bim4Everyone;
using dosymep.SimpleServices;

using RevitMarkPlacement.Models;
using RevitMarkPlacement.ViewModels;
using RevitMarkPlacement.Views;

namespace RevitMarkPlacement {

    [Transaction(TransactionMode.Manual)]
    public class PlaceAnnotationCommand : BasePluginCommand {
        public PlaceAnnotationCommand() {
            PluginName = "Расстановщик отметок";
        }

        protected override void Execute(UIApplication uiApplication) {
            var config = AnnotationsConfig.GetAnnotationsConfig();
            var revitRepository = new RevitRepository(uiApplication.Application, uiApplication.ActiveUIDocument.Document);

            var viewModel = new MainViewModel(revitRepository, config);
            Window view = null;
            if(!viewModel.CanPlaceAnnotation()) {
                view = new ReportView() { DataContext = viewModel.InfoElementsViewModel };

            } else {
                view = new MainWindow() { DataContext = viewModel };
            }
            if(view?.ShowDialog() == true) {
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
