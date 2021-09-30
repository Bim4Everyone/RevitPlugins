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

using RevitCreateViewSheet.ViewModels;
using RevitCreateViewSheet.Views;

namespace RevitCreateViewSheet {
    [Transaction(TransactionMode.Manual)]
    public class CreateViewSheetCommand : IExternalCommand {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements) {
            AppDomain.CurrentDomain.AssemblyResolve += AppDomainExtensions.CurrentDomain_AssemblyResolve;
            try {
                Excecute(commandData);
            } catch(Exception ex) {
#if D2020 || D2021 || D2022
                TaskDialog.Show("Менеджер листов.", ex.ToString());
#else
                TaskDialog.Show("Менеджер листов.", ex.Message);
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

            var window = new CreateViewSheetWindow() { DataContext = new AppViewModel(uiApplication) };
            new WindowInteropHelper(window) { Owner = uiApplication.MainWindowHandle };
            window.ShowDialog();
        }
    }
}
