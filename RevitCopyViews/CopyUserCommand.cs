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
using dosymep.Bim4Everyone.ProjectParams;
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
#if D2020 || D2021 || D2022
                TaskDialog.Show("Новый пользователь.", ex.ToString());
#else
                TaskDialog.Show("Новый пользователь.", ex.Message);
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

            
            var views = new FilteredElementCollector(document).OfClass(typeof(View)).ToElements();
            var selectedViews = views
                .OfType<View>()
                .Where(item => item.Name.StartsWith("User_"))
                .ToList();

            if(selectedViews.Count == 0) {
                TaskDialog.Show("Предупреждение!", "Не были найдены пользовательские виды, которые можно копировать.");
                return;
            }
            
            var groupViews = views
                .Select(item => (string) item.GetParamValueOrDefault(ProjectParamsConfig.Instance.ViewGroup))
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
                    UIDocument = uiDocument,
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
