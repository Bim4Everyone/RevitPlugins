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
using dosymep.Bim4Everyone.Templates;
using dosymep.Revit;
using dosymep.SimpleServices;

using RevitCopyViews.ViewModels;
using RevitCopyViews.Views;

namespace RevitCopyViews {
    [Transaction(TransactionMode.Manual)]
    public class CopyUserCommand : BasePluginCommand {
        public CopyUserCommand() {
            PluginName = "Новый пользователь";
        }

        protected override void Execute(UIApplication uiApplication) {
            var application = uiApplication.Application;

            var uiDocument = uiApplication.ActiveUIDocument;
            var document = uiDocument.Document;

            var projectParameters = ProjectParameters.Create(application);
            projectParameters.SetupBrowserOrganization(document);
            projectParameters.SetupRevitParams(document, ProjectParamsConfig.Instance.ViewGroup, ProjectParamsConfig.Instance.ProjectStage);


            var views = new FilteredElementCollector(document).OfClass(typeof(View)).ToElements();
            var selectedViews = views
                .OfType<View>()
                .Where(item => item.Name.StartsWith("User_"))
                .ToList();

            if(selectedViews.Count == 0) {
                TaskDialog.Show("Предупреждение!", "Не были найдены виды начинающиеся на \"User_\".");
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
            
            Notification(window);
        }
    }
}
