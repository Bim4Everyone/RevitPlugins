#region Namespaces
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

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
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Application app = uiapp.Application;
            Document doc = uidoc.Document;

            var revitPrint = new RevitPrint(doc);
            var viewSheets = revitPrint.GetViewSheets();

            var values = viewSheets
                .SelectMany(item => revitPrint.FilterParameterNames.Select(paramName => item.LookupParameter(paramName)?.AsString()))
                .Where(item => !string.IsNullOrEmpty(item))
                .Distinct()
                .OrderBy(item => item)
                .ToList();

            revitPrint.FilterParameterValue = values.FirstOrDefault();
            revitPrint.Execute();

            if(revitPrint.Errors.Count > 0) {
                message = string.Join(Environment.NewLine + "- ", revitPrint.Errors);
                return Result.Failed;
            }

            return Result.Succeeded;
        }
    }
}
