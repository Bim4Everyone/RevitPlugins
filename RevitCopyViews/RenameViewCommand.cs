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

using RevitCopyViews.ViewModels;
using RevitCopyViews.Views;

namespace RevitCopyViews {
    [Transaction(TransactionMode.Manual)]
    public class RenameViewCommand : BasePluginCommand {
        public RenameViewCommand() {
            PluginName = "Переименование видов";
        }

        protected override void Execute(UIApplication uiApplication) {
            var application = uiApplication.Application;

            var uiDocument = uiApplication.ActiveUIDocument;
            var document = uiDocument.Document;

            var selectedViews = uiDocument.Selection.GetElementIds().Select(item => document.GetElement(item)).OfType<View>().ToList();
            if(selectedViews.Count == 0) {
                TaskDialog.Show("Предупреждение!", "Выберите виды, которые требуется переименовать.");
                return;
            }

            var restrictedViewNames = new FilteredElementCollector(document)
                .OfClass(typeof(View))
                .Select(item => item.Name)
                .OrderBy(item => item)
                .Distinct()
                .Except(selectedViews.Select(item => item.Name))
                .ToList();

            var window = new RenameViewWindow() {
                DataContext = new RenameViewViewModel(selectedViews) {
                    Document = document,
                    UIDocument = uiDocument,
                    Application = application,

                    RestrictedViewNames = restrictedViewNames
                }
            };
           
            Notification(window);
        }
    }
}
