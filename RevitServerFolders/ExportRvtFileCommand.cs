#region Namespaces
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Interop;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

using RevitServerFolders.Export;

#endregion

namespace RevitServerFolders {
    [Transaction(TransactionMode.Manual)]
    public class ExportRvtFileCommand : IExternalCommand {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements) {
            try {
                var uiApplication = commandData.Application;
                var application = uiApplication.Application;

                var exportRvtFileViewModel = new ExportRvtFileViewModel(application.VersionNumber, ExportRvtFileConfig.GetExportRvtFileConfig());

                var exportWindow = new ExportRvtFileWindow { DataContext = exportRvtFileViewModel };
                new WindowInteropHelper(exportWindow) { Owner = uiApplication.MainWindowHandle };

                exportRvtFileViewModel.Owner = exportWindow;
                if(exportWindow.ShowDialog() == true) {
                    exportRvtFileViewModel = (ExportRvtFileViewModel) exportWindow.DataContext;

                    SaveExportRvtFileConfig(exportRvtFileViewModel);
                    DetachRevitFiles(exportRvtFileViewModel);
                    UnloadAllLinks(exportRvtFileViewModel);
                    ExportFilesToNavisworks(application, exportRvtFileViewModel);

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

        private static void ExportFilesToNavisworks(Application application, ExportRvtFileViewModel exportRvtFileViewModel) {
            if(exportRvtFileViewModel.WithNwcFiles) {
                new ExportFilesToNavisworksCommand() {
                    Application = application,
                    SourceFolderName = exportRvtFileViewModel.TargetRvtFolder,
                    TargetFolderName = exportRvtFileViewModel.TargetNwcFolder,

                    WithRooms = exportRvtFileViewModel.WithRooms,
                    CleanTargetFolder = exportRvtFileViewModel.CleanTargetNwcFolder
                }.Execute();
            }
        }

        private static void UnloadAllLinks(ExportRvtFileViewModel exportRvtFileViewModel) {
            new UnloadAllLinksCommand() {
                SourceFolderName = exportRvtFileViewModel.TargetRvtFolder
            }.Execute();
        }

        private static void DetachRevitFiles(ExportRvtFileViewModel exportRvtFileViewModel) {
            new DetachRevitFilesCommand() {
                ServerName = exportRvtFileViewModel.ServerName,
                RevitVersion = exportRvtFileViewModel.RevitVersion,
                WithSubFolders = exportRvtFileViewModel.WithSubFolders,
                FolderName = exportRvtFileViewModel.SourceRvtFolder,
                TargetFolderName = exportRvtFileViewModel.TargetRvtFolder,
                CleanTargetFolder = exportRvtFileViewModel.CleanTargetRvtFolder
            }.Execute();
        }

        private static void SaveExportRvtFileConfig(ExportRvtFileViewModel exportRvtFileViewModel) {
            ExportRvtFileConfig.SaveExportRvtFileConfig(new ExportRvtFileConfig() {
                ServerName = exportRvtFileViewModel.ServerName,
                WithRooms = exportRvtFileViewModel.WithRooms,
                WithNwcFiles = exportRvtFileViewModel.WithNwcFiles,
                SourceRvtFolder = exportRvtFileViewModel.SourceRvtFolder,
                TargetNwcFolder = exportRvtFileViewModel.TargetNwcFolder,
                TargetRvtFolder = exportRvtFileViewModel.TargetRvtFolder,
                WithSubFolders = exportRvtFileViewModel.WithSubFolders,
                CleanTargetNwcFolder = exportRvtFileViewModel.CleanTargetNwcFolder,
                CleanTargetRvtFolder = exportRvtFileViewModel.CleanTargetRvtFolder
            });
        }
    }
}
