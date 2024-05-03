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
using dosymep.Bim4Everyone.SharedParams;
using dosymep.Bim4Everyone.Templates;
using dosymep.SimpleServices;

using RevitCreateViewSheet.ViewModels;
using RevitCreateViewSheet.Views;

namespace RevitCreateViewSheet {
    [Transaction(TransactionMode.Manual)]
    public class CreateViewSheetCommand : BasePluginCommand {
        public CreateViewSheetCommand() {
            PluginName = "Менеджер листов";
        }

        protected override void Execute(UIApplication uiApplication) {
            var application = uiApplication.Application;

            var uiDocument = uiApplication.ActiveUIDocument;
            var document = uiDocument.Document;

            var projectParameters = ProjectParameters.Create(application);
            projectParameters.SetupRevitParams(document, SharedParamsConfig.Instance.AlbumBlueprints, SharedParamsConfig.Instance.StampSheetNumber);

            var window = new CreateViewSheetWindow() {
                DataContext = new AppViewModel(uiApplication)
            };
            
            Notification(window);
        }
    }
}