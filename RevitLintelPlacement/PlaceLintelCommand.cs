using System;
using System.IO;
using System.Linq;
using System.Windows.Interop;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep;

using RevitLintelPlacement.Models;
using RevitLintelPlacement.ViewModels;
using RevitLintelPlacement.Views;

namespace RevitLintelPlacement {

    [Transaction(TransactionMode.Manual)]
    public class PlaceLintelCommand : IExternalCommand {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements) {
            AppDomain.CurrentDomain.AssemblyResolve += AppDomainExtensions.CurrentDomain_AssemblyResolve;
            try {
                var lintelsConfig = LintelsConfig.GetLintelsConfig();
                var revitRepository = new RevitRepository(commandData.Application.Application, commandData.Application.ActiveUIDocument.Document, lintelsConfig);
               
                var mainViewModel = new MainViewModel(revitRepository);
                var window = new MainWindow() { DataContext = mainViewModel };
                WindowInteropHelper windowInteropHelper = new WindowInteropHelper(window) { Owner = commandData.Application.MainWindowHandle };
                window.ShowDialog();
            } catch(Exception ex) {
#if D2020 || D2021 || D2022
                TaskDialog.Show("Расстановщик перемычек.", ex.ToString()); //TODO: придумать название плагину
#else
                TaskDialog.Show("Расстановщик перемычек.", ex.Message);
#endif
            } finally {
                AppDomain.CurrentDomain.AssemblyResolve -= AppDomainExtensions.CurrentDomain_AssemblyResolve;
            }

            return Result.Succeeded;
        }
    }
}
