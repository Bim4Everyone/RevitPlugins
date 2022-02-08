#region Namespaces
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Interop;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

using dosymep;

using RevitCopyStandarts.ViewModels;

#endregion

namespace RevitCopyStandarts {
    [Transaction(TransactionMode.Manual)]
    public class CopyStandartsRevitCommand : IExternalCommand {
        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements) {
            UIApplication uiApplication = commandData.Application;
            UIDocument uiDocument = uiApplication.ActiveUIDocument;
            Application application = uiApplication.Application;
            Document document = uiDocument.Document;

            AppDomain.CurrentDomain.AssemblyResolve += AppDomainExtensions.CurrentDomain_AssemblyResolve;
            try {
                new PyRevitCommand().Execute(commandData.Application);
            } catch(Exception ex) {
#if D2020 || D2021 || D2022
                TaskDialog.Show("Копирование стандартов.", ex.ToString());
#else
                TaskDialog.Show("Копирование стандартов.", ex.Message);
#endif
            } finally {
                AppDomain.CurrentDomain.AssemblyResolve -= AppDomainExtensions.CurrentDomain_AssemblyResolve;
            }

            return Result.Succeeded;
        }
    }

    public class PyRevitCommand {
        public void Execute(UIApplication uiApplication) {
            UIDocument uiDocument = uiApplication.ActiveUIDocument;
            Application application = uiApplication.Application;
            Document document = uiDocument.Document;

            var mainWindow = new MainWindow() { BimCategories = new ViewModels.BimCategoriesViewModel(@"T:\Проектный институт\Отдел стандартизации BIM и RD\BIM-Ресурсы\5-Надстройки\Шаблоны и настройки", document, application) };
            new WindowInteropHelper(mainWindow) { Owner = uiApplication.MainWindowHandle };
            mainWindow.ShowDialog();
        }
    }
}
