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
using dosymep.Revit;

using RevitCopyViews.ViewModels;
using RevitCopyViews.Views;

namespace RevitCopyViews {
    [Transaction(TransactionMode.Manual)]
    public class CopyUserCommand : IExternalCommand {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements) {
            AppDomain.CurrentDomain.AssemblyResolve += AppDomainExtensions.CurrentDomain_AssemblyResolve;
            try {
                Excecute(commandData);
            } catch(Exception ex) {
#if DEBUG
                System.Windows.MessageBox.Show(ex.ToString(), "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
#else
                System.Windows.MessageBox.Show(ex.Message, "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
#endif
            } finally {
                AppDomain.CurrentDomain.AssemblyResolve -= AppDomainExtensions.CurrentDomain_AssemblyResolve;
            }

            return Result.Succeeded;
        }

        private void Excecute(ExternalCommandData commandData) {
            var uiApplication = commandData.Application;
            var application = uiApplication.Application;

            var uiDocument = uiApplication.ActiveUIDocument;
            var document = uiDocument.Document;

            var selectedViews = uiDocument.Selection.GetElementIds()
                .Select(item => document.GetElement(item))
                .OfType<View>()
                .Where(item => item.Name.StartsWith("User_"))
                .ToList();

            if(selectedViews.Count == 0) {
                TaskDialog.Show("Предупреждение!", "Выберите виды, которые требуется копировать.");
                return;
            }

            var views = new FilteredElementCollector(document).OfClass(typeof(View)).ToElements();
            var groupViews = views
                .Select(item => (string) item.GetParamValueOrDefault("_Группа Видов"))
                .Where(item => !string.IsNullOrEmpty(item))
                .OrderBy(item => item)
                .Distinct()
                .ToList();

            var restrictedViewNames = views
                .Select(item => item.Name)
                .OrderBy(item => item)
                .ToList();

            var window = new CopyUserWindow() {
                DataContext = new CopyUserViewModel() {
                    Document = document,
                    Application = application,

                    Views = selectedViews,
                    GroupViews = groupViews,
                    RestrictedViewNames = restrictedViewNames
                }
            };

            new WindowInteropHelper(window) { Owner = uiApplication.MainWindowHandle };
            window.ShowDialog();
        }
    }
}
