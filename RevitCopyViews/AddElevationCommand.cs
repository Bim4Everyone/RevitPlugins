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
    public class AddElevationCommand : IExternalCommand {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements) {
            AppDomain.CurrentDomain.AssemblyResolve += AppDomainExtensions.CurrentDomain_AssemblyResolve;
            try {
                Excecute(commandData);
            } catch(Exception ex) {
#if DEBUG
                System.Windows.MessageBox.Show(ex.ToString(), "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
#else
                System.Windows.MessageBox.Show(ex.Message, "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
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
                transaction.Start("Добавление отметки этажа");

                var selectedViews = uiDocument.GetSelectedElements().OfType<View>();
                foreach(var view in selectedViews) {
                    var splittedName = Delimiter.SplitViewName(view.Name, new SplitViewOptions() { ReplacePrefix = false, ReplaceSuffix = false });
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
                string message = "Не были изменены имена у следующих видов:" + Environment.NewLine + " - " + string.Join(Environment.NewLine + " - ", errors.Select(item => $"{item.Id.IntegerValue} - {item.Name}"));
                TaskDialog.Show("Предупреждение!", message, TaskDialogCommonButtons.Ok);
            }
        }
    }
}
