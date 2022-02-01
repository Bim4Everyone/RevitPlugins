﻿#region Namespaces
using System;
using System.Windows.Interop;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep;

using RevitServerFolders.Export;

#endregion

namespace RevitServerFolders {
    [Transaction(TransactionMode.Manual)]
    public class ExportNwcFileCommand : IExternalCommand {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements) {
            AppDomain.CurrentDomain.AssemblyResolve += AppDomainExtensions.CurrentDomain_AssemblyResolve;
            try {

                var uiApplication = commandData.Application;
                var application = uiApplication.Application;

                var exportNwcFileViewModel = new ExportNwcFileViewModel(ExportNwcFileConfig.GetExportNwcFileConfig());
                var exportWindow = new ExportNwcFileWindow { DataContext = exportNwcFileViewModel };
                new WindowInteropHelper(exportWindow) { Owner = uiApplication.MainWindowHandle };

                if(exportWindow.ShowDialog() == true) {
                    exportNwcFileViewModel = (ExportNwcFileViewModel) exportWindow.DataContext;

                    SaveExportNwcFileConfig(exportNwcFileViewModel);
                    UnloadAllLinks(exportNwcFileViewModel);
                    ExportFilesToNavisworks(application, exportNwcFileViewModel);

                    System.Windows.MessageBox.Show("Готово!", "Сообщение!", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                }
            } catch(Exception ex) {
#if D2020 || D2021 || D2022
                TaskDialog.Show("Экспорт NWC в RVT.", ex.ToString());
#else
                TaskDialog.Show("Экспорт NWC в RVT.", ex.Message);
#endif
            } finally {
                AppDomain.CurrentDomain.AssemblyResolve -= AppDomainExtensions.CurrentDomain_AssemblyResolve;
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
            exportConfig.WithSubFolders = exportNwcFileViewModel.WithSubFolders;
            
            exportConfig.SaveProjectConfig();
        }
    }
}
