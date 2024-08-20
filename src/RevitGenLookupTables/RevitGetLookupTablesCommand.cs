using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep;
using dosymep.Bim4Everyone;
using dosymep.SimpleServices;

using RevitGenLookupTables.ViewModels;
using RevitGenLookupTables.Views;

namespace RevitGenLookupTables {
    [Transaction(TransactionMode.Manual)]
    public class RevitGenLookupTablesCommand : BasePluginCommand {
        public RevitGenLookupTablesCommand() {
            PluginName = "Генерация таблицы выбора";
        }

        protected override void Execute(UIApplication uiApplication) {
            var application = uiApplication.Application;
            var document = uiApplication.ActiveUIDocument.Document;

            var window = new LookupTablesWindow() {
                DataContext = new FamilyViewModel(new Models.RevitRepository(application, document))
            };
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
