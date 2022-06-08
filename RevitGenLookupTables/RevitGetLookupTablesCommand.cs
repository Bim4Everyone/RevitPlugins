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
            window.ShowDialog();
        }
    }
}
