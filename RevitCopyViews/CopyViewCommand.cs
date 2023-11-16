using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using dosymep.Bim4Everyone.Templates;
using dosymep.Revit;
using dosymep.SimpleServices;

using RevitCopyViews.ViewModels;
using RevitCopyViews.Views;

namespace RevitCopyViews {
    [Transaction(TransactionMode.Manual)]
    public class CopyViewCommand : BasePluginCommand {
        public CopyViewCommand() {
            PluginName = "Копирование видов";
        }

        protected override void Execute(UIApplication uiApplication) {
            var application = uiApplication.Application;

            var uiDocument = uiApplication.ActiveUIDocument;
            var document = uiDocument.Document;

            var projectParameters = ProjectParameters.Create(application);
            projectParameters.SetupBrowserOrganization(document);
            projectParameters.SetupRevitParams(document, ProjectParamsConfig.Instance.ViewGroup, ProjectParamsConfig.Instance.ProjectStage);

            var selectedViews = uiDocument.Selection.GetElementIds()
                .Select(item => document.GetElement(item))
                .OfType<View>()
                .Where(item => IsView(item))
                .ToList();

            if(selectedViews.Count == 0) {
                TaskDialog.Show("Предупреждение!", "Выберите виды, которые требуется скопировать.");
                return;
            }

            var views = new FilteredElementCollector(document)
                .OfClass(typeof(View))
                .OfType<View>()
                .Where(item => IsView(item))
                .ToList();

            var groupViews = views
                 .Select(item => (string) item.GetParamValueOrDefault(ProjectParamsConfig.Instance.ViewGroup))
                 .Where(item => !string.IsNullOrEmpty(item))
                 .OrderBy(item => item)
                 .Distinct();

            var window = new CopyViewWindow() {
                DataContext = new CopyViewViewModel(selectedViews) {
                    Document = document,
                    UIDocument = uiDocument,
                    Application = application,

                    GroupViews = new ObservableCollection<string>(groupViews),
                    RestrictedViewNames = views.Select(item => item.Name).ToList()
                }
            };

            Notification(window);
        }

        private static bool IsView(View item) {
            return item.Category?.Id != new ElementId(BuiltInCategory.OST_Schedules) && item.Category?.Id != new ElementId(BuiltInCategory.OST_Sheets);
        }
    }
}
