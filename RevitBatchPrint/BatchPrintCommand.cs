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
                var printerManager = new Models.Printing.PrintManager();
                if(!printerManager.HasPrinterName(RevitRepository.DefaultPrinterName)) {
                    TaskDialog.Show("Пакетная печать.", $"У вас не установлен принтер \"{RevitRepository.DefaultPrinterName}\".");
                    return;
                }

                var repository = new RevitRepository(uiapp);
                var printParamName = repository.GetPrintParamNames().FirstOrDefault(item => RevitRepository.PrintParamNames.Contains(item));
                if(string.IsNullOrEmpty(printParamName)) {
                    TaskDialog.Show("Пакетная печать.", "Не был найден атрибут группировки альбомов.");
                    return;
                }

                var revitPrint = new RevitPrint(repository);
                List<(string, int)> printParamValues = repository.GetPrintParamValues(printParamName);
                if(printParamValues.Count == 0) {
                    TaskDialog.Show("Пакетная печать.", "Не были обнаружены основные надписи.");
                    return;
                }

                var albums = printParamValues.Select(item => new PrintAlbumViewModel() { Name = item.Item1, Count = item.Item2 }).ToList();
                var window = new BatchPrintWindow() {
                    DataContext = new PrintAbumsViewModel() {
                        Albums = albums,
                        SelectedAlbum = albums.FirstOrDefault()
                    }
                };

                new WindowInteropHelper(window) { Owner = uiapp.MainWindowHandle };
                if(window.ShowDialog() == true) {
                    revitPrint.PrinterName = RevitRepository.DefaultPrinterName;
                    revitPrint.FilterParamName = printParamName;
                    revitPrint.FilterParamValue = ((PrintAbumsViewModel) window.DataContext).SelectedAlbum.Name;
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
