#region Namespaces
using System;
using System.Windows.Interop;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep;
using dosymep.Bim4Everyone;

using RevitServerFolders.Export;

#endregion

namespace RevitServerFolders {
    [Transaction(TransactionMode.Manual)]
    public class ExportNwcFileCommand : BasePluginCommand {
        public ExportNwcFileCommand() {
            PluginName = "Экспорт NWC в RVT";
        }

        protected override void Execute(UIApplication uiApplication) {
            var application = uiApplication.Application;

            var exportNwcFileViewModel = new ExportNwcFileViewModel(ExportNwcFileConfig.GetExportNwcFileConfig());
            var exportWindow = new ExportNwcFileWindow {DataContext = exportNwcFileViewModel};

            if(exportWindow.ShowDialog() == true) {
                exportNwcFileViewModel = (ExportNwcFileViewModel) exportWindow.DataContext;

                SaveExportNwcFileConfig(exportNwcFileViewModel);
                UnloadAllLinks(exportNwcFileViewModel);
                ExportFilesToNavisworks(uiApplication, exportNwcFileViewModel);

                TaskDialog.Show("Сообщение!", "Готово!");
            }
        }

        private static void ExportFilesToNavisworks(UIApplication uiApplication,
            ExportNwcFileViewModel exportNwcFileViewModel) {
            new ExportFilesToNavisworksCommand() {
                Application = uiApplication.Application,
                UIApplication = uiApplication,
                SourceFolderName = exportNwcFileViewModel.SourceNwcFolder,
                TargetFolderName = exportNwcFileViewModel.TargetNwcFolder,
                WithRooms = exportNwcFileViewModel.WithRooms,
                WithLinkedFiles = exportNwcFileViewModel.WithLinkedFiles,
                CleanTargetFolder = exportNwcFileViewModel.CleanTargetNwcFolder
            }.Execute();
        }

        private static void UnloadAllLinks(ExportNwcFileViewModel exportNwcFileViewModel) {
            new UnloadAllLinksCommand() {
                SourceFolderName = exportNwcFileViewModel.SourceNwcFolder
            }.Execute();
        }

        private static void SaveExportNwcFileConfig(ExportNwcFileViewModel exportNwcFileViewModel) {
            ExportNwcFileConfig exportConfig = ExportNwcFileConfig.GetExportNwcFileConfig();

            exportConfig.CleanTargetNwcFolder = exportNwcFileViewModel.CleanTargetNwcFolder;
            exportConfig.SourceNwcFolder = exportNwcFileViewModel.SourceNwcFolder;
            exportConfig.TargetNwcFolder = exportNwcFileViewModel.TargetNwcFolder;
            exportConfig.WithRooms = exportNwcFileViewModel.WithRooms;
            exportConfig.WithLinkedFiles = exportNwcFileViewModel.WithLinkedFiles;
            exportConfig.WithSubFolders = exportNwcFileViewModel.WithSubFolders;
            
            exportConfig.SaveProjectConfig();
        }
    }
}
