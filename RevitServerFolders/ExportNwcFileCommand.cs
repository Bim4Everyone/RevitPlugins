#region Namespaces
using System;
using System.Windows.Interop;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using RevitServerFolders.Export;

#endregion

namespace RevitServerFolders {
    [Transaction(TransactionMode.Manual)]
    public class ExportNwcFileCommand : IExternalCommand {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements) {
            try {
                var uiApplication = commandData.Application;
                var application = uiApplication.Application;

                var exportNwcFileViewModel = new ExportNwcFileViewModel(ExportNwcFileConfig.GetExportNwcFileConfig());
                var exportWindow = new ExportNwcFileWindow { DataContext = exportNwcFileViewModel };
                new WindowInteropHelper(exportWindow) { Owner = uiApplication.MainWindowHandle };

                if(exportWindow.ShowDialog() == true) {
                    exportNwcFileViewModel = (ExportNwcFileViewModel) exportWindow.DataContext;

                    SaveExportNwcFileConfig(exportNwcFileViewModel);
                    UnloadRevitLinks(exportNwcFileViewModel);
                    ExportFilesToNavisworks(application, exportNwcFileViewModel);

                    System.Windows.MessageBox.Show("Готово!", "Сообщение!", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                }
            } catch(Exception ex) {
#if DEBUG
                System.Windows.MessageBox.Show(ex.ToString(), "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
#else
                System.Windows.MessageBox.Show(ex.Message, "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
#endif
            }

            return Result.Succeeded;
        }

        private static void ExportFilesToNavisworks(Autodesk.Revit.ApplicationServices.Application application, ExportNwcFileViewModel exportNwcFileViewModel) {
            new ExportFilesToNavisworksCommand() {
                Application = application,
                SourceFolderName = exportNwcFileViewModel.SourceNwcFolder,
                TargetFolderName = exportNwcFileViewModel.TargetNwcFolder,

                WithRooms = exportNwcFileViewModel.WithRooms,
                CleanTargetFolder = exportNwcFileViewModel.CleanTargetNwcFolder
            }.Execute();
        }

        private static void UnloadRevitLinks(ExportNwcFileViewModel exportNwcFileViewModel) {
            new UnloadRevitLinksCommand() {
                SourceFolderName = exportNwcFileViewModel.SourceNwcFolder
            }.Execute();
        }

        private static void SaveExportNwcFileConfig(ExportNwcFileViewModel exportNwcFileViewModel) {
            ExportNwcFileConfig.SaveExportNwcFileConfig(new ExportNwcFileConfig() {
                CleanTargetNwcFolder = exportNwcFileViewModel.CleanTargetNwcFolder,
                SourceNwcFolder = exportNwcFileViewModel.SourceNwcFolder,
                TargetNwcFolder = exportNwcFileViewModel.TargetNwcFolder,
                WithRooms = exportNwcFileViewModel.WithRooms,
                WithSubFolders = exportNwcFileViewModel.WithSubFolders
            });
        }
    }
}
