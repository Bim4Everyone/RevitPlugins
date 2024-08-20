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
using dosymep.Bim4Everyone;
using dosymep.SimpleServices;

using RevitCopyStandarts.ViewModels;

#endregion

namespace RevitCopyStandarts {
    [Transaction(TransactionMode.Manual)]
    public class CopyStandartsRevitCommand : BasePluginCommand {
        public CopyStandartsRevitCommand() {
            PluginName = "Копирование стандартов";
        }

        protected override void Execute(UIApplication uiApplication) {
            UIDocument uiDocument = uiApplication.ActiveUIDocument;
            Application application = uiApplication.Application;
            Document document = uiDocument.Document;

            var mainFolder =
                @"W:\Проектный институт\Отд.стандарт.BIM и RD\BIM-Ресурсы\5-Надстройки\Bim4Everyone\A101";

            mainFolder =
                Path.Combine(mainFolder, ModuleEnvironment.RevitVersion, "RevitCopyStandarts");
            
            var mainWindow = new MainWindow() {
                DataContext = new BimCategoriesViewModel(mainFolder, document, application)
            };

            mainWindow.ShowDialog();
        }
    }
}
