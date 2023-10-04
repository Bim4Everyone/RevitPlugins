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

using RevitCopyViews.ViewModels;

namespace RevitCopyViews {
    [Transaction(TransactionMode.Manual)]
    public class UpdateElevationCommand : BasePluginCommand {
        public UpdateElevationCommand() {
            PluginName = "Обновить отметки этажа";
        }

        protected override void Execute(UIApplication uiApplication)  {
            var application = uiApplication.Application;

            var uiDocument = uiApplication.ActiveUIDocument;
            var document = uiDocument.Document;

            var restrictedViewNames = new FilteredElementCollector(document)
                .OfClass(typeof(View))
                .Select(item => item.Name)
                .OrderBy(item => item)
                .Distinct()
                .ToArray();

            var errors = new List<View>();
            using(var transaction = document.StartTransaction("Обновление отметки этажа")) {
                var views = new FilteredElementCollector(document)
                   .OfClass(typeof(View))
                   .WhereElementIsNotElementType()
                   .OfType<View>()
                   .Where(item => !item.IsTemplate)
                   .Where(item => item.ViewType == ViewType.FloorPlan
                       || item.ViewType == ViewType.CeilingPlan
                       || item.ViewType == ViewType.AreaPlan
                       || item.ViewType == ViewType.EngineeringPlan)
                   .ToArray();

                foreach(var view in views) {
                    var splittedName = Delimiter.SplitViewName(view.Name, new SplitViewOptions() { ReplacePrefix = false, ReplaceSuffix = false });

                    if(splittedName.HasElevation) {
                        splittedName.Elevations = SplittedViewName.GetElevation(view);

                        string viewName = Delimiter.CreateViewName(splittedName);
                        if(view.Name.Equals(viewName)) {
                            continue;
                        }

                        if(restrictedViewNames.Any(item => viewName.Equals(item))) {
                            errors.Add(view);
                            continue;
                        }

                        view.Name = viewName;
                    }
                }

                transaction.Commit();
            }

            if(errors.Count > 0) {
                Notification(false);
                string message = "Не были изменены имена у следующих видов:" + Environment.NewLine + " - " + string.Join(Environment.NewLine + " - ", errors.Select(item => $"{item.Id.GetIdValue()} - {item.Name}"));
                TaskDialog.Show("Предупреждение!", message, TaskDialogCommonButtons.Ok);
            } else {
                Notification(true);
            }
        }
    }
}
