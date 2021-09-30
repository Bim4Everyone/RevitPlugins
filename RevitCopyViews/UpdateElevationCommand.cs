using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep;
using dosymep.Revit;

using RevitCopyViews.ViewModels;

namespace RevitCopyViews {
    [Transaction(TransactionMode.Manual)]
    public class UpdateElevationCommand : IExternalCommand {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements) {
            AppDomain.CurrentDomain.AssemblyResolve += AppDomainExtensions.CurrentDomain_AssemblyResolve;
            try {
                Excecute(commandData);
            } catch(Exception ex) {
#if D2020 || D2021 || D2022
                TaskDialog.Show("Обновление отметок этажа.", ex.ToString());
#else
                TaskDialog.Show("Обновление отметок этажа.", ex.Message);
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

            var restrictedViewNames = new FilteredElementCollector(document)
                .OfClass(typeof(View))
                .Select(item => item.Name)
                .OrderBy(item => item)
                .Distinct()
                .ToArray();

            var errors = new List<View>();
            using(var transaction = new Transaction(document)) {
                transaction.Start("Обновление отметки этажа");

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
                string message = "Не были изменены имена у следующих видов:" + Environment.NewLine + " - " + string.Join(Environment.NewLine + " - ", errors.Select(item => $"{item.Id.IntegerValue} - {item.Name}"));
                TaskDialog.Show("Предупреждение!", message, TaskDialogCommonButtons.Ok);
            }
        }
    }
}
