using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep;
using dosymep.Bim4Everyone;
using dosymep.Revit;
using dosymep.SimpleServices;

using RevitCopyViews.ViewModels;

namespace RevitCopyViews {
    [Transaction(TransactionMode.Manual)]
    public class AddElevationCommand : BasePluginCommand {
        public AddElevationCommand() {
            PluginName = "Добавить отметки этажа";
        }

        protected override void Execute(UIApplication uiApplication) {
            var uiDocument = uiApplication.ActiveUIDocument;
            var document = uiDocument.Document;

            var restrictedViewNames = new FilteredElementCollector(document)
                .OfClass(typeof(View))
                .Select(item => item.Name)
                .OrderBy(item => item)
                .Distinct()
                .ToArray();

            var errors = new List<View>();
            using(var transaction = document.StartTransaction("Добавление отметки этажа")) {
                var selectedViews = uiDocument.GetSelectedElements().OfType<View>();
                foreach(var view in selectedViews) {
                    var splittedName = Delimiter.SplitViewName(view.Name,
                        new SplitViewOptions() {ReplacePrefix = false, ReplaceSuffix = false});
                    splittedName.Elevations = SplittedViewName.GetElevation(view);

                    string viewName = Delimiter.CreateViewName(splittedName);
                    if(view.Name.Equals(viewName)) {
                        continue;
                    }

                    if(restrictedViewNames.Any(item => viewName.Equals(item))) {
                        errors.Add(view);
                        continue;
                    }

                    view.Name = Delimiter.CreateViewName(splittedName);
                }

                transaction.Commit();
            }

            if(errors.Count > 0) {
                Notification(false);
                string message = "Не были изменены имена у следующих видов:"
                                 + Environment.NewLine +
                                 " - "
                                 + string.Join(Environment.NewLine + " - ",
                                     errors.Select(item => $"{item.Id.GetIdValue()} - {item.Name}"));
                TaskDialog.Show("Предупреждение!", message, TaskDialogCommonButtons.Ok);
            } else {
                Notification(true);
            }
        }
    }
}