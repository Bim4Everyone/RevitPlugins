#region Namespaces
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Interop;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

using dosymep;

using RevitBatchPrint.Models;
using RevitBatchPrint.ViewModels;
using RevitBatchPrint.Views;

#endregion

namespace RevitBatchPrint {
    [Transaction(TransactionMode.Manual)]
    public class BatchPrintCommand : IExternalCommand {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements) {
            AppDomain.CurrentDomain.AssemblyResolve += AppDomainExtensions.CurrentDomain_AssemblyResolve;
            try {
                new PyRevitCommand().Execute(commandData.Application);
                return Result.Succeeded;
            } finally {
                AppDomain.CurrentDomain.AssemblyResolve -= AppDomainExtensions.CurrentDomain_AssemblyResolve;
            }
        }
    }

    public class PyRevitCommand {
        public void Execute(UIApplication uiapp) {
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Application app = uiapp.Application;
            Document doc = uidoc.Document;

            try {
                var window = new BatchPrintWindow() { DataContext = new PrintAbumsViewModel(uiapp) };
                new WindowInteropHelper(window) { Owner = uiapp.MainWindowHandle };
                
                window.ShowDialog();
            } catch(Exception ex) {
#if DEBUG
                TaskDialog.Show("Пакетная печать.", ex.ToString());
#else
                TaskDialog.Show("Пакетная печать.", ex.Message);
#endif
            }
        }
    }
}
