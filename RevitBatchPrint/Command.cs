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

#endregion

namespace RevitBatchPrint {
    [Transaction(TransactionMode.Manual)]
    public class Command : IExternalCommand {
        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements) {

            new PyRevitCommand().Execute(commandData.Application);
            return Result.Succeeded;
        }
    }

    public class PyRevitCommand {
        public void Execute(UIApplication uiapp) {
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Application app = uiapp.Application;
            Document doc = uidoc.Document;

            try {
                var revitPrint = new RevitPrint(doc);
                var viewSheets = revitPrint.GetViewSheets();

                var viewSheetNames = viewSheets
                    .SelectMany(item => revitPrint.FilterParameterNames.Select(paramName => item.LookupParameter(paramName)?.AsString()))
                    .Where(item => !string.IsNullOrEmpty(item))
                    .Distinct()
                    .OrderBy(item => item)
                    .ToList();

                var window = new PrintViewSheetNamesWindow() {
                    DataContext = new ViewSheetNamesViewModel() {
                        Names = viewSheetNames,
                        SelectedName = viewSheetNames.FirstOrDefault()
                    }
                };

                new WindowInteropHelper(window) { Owner = uiapp.MainWindowHandle };
                if(window.ShowDialog() == true) {
                    revitPrint.FilterParameterValue = viewSheetNames.FirstOrDefault();
                    revitPrint.Execute();

                    if(revitPrint.Errors.Count > 0) {
                        TaskDialog.Show("Пакетная печать.", string.Join(Environment.NewLine + "- ", revitPrint.Errors));
                    } else {
                        TaskDialog.Show("Пакетная печать.", "Готово!");
                    }
                }
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
