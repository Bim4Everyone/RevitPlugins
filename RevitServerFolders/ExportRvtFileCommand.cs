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

#endregion

namespace RevitServerFolders {
    [Transaction(TransactionMode.Manual)]
    public class ExportRvtFileCommand : IExternalCommand {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements) {
            new ExportRvtFileCommandPyRevitCommand().Execute(commandData.Application);
            return Result.Succeeded;
        }
    }

    [Transaction(TransactionMode.Manual)]
    public class ExportNwcFileCommand : IExternalCommand {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements) {
            new ExportNwcFileCommandPyRevitCommand().Execute(commandData.Application);
            return Result.Succeeded;
        }
    }

    public class ExportRvtFileCommandPyRevitCommand {
        public void Execute(UIApplication uiapp) {
            try {
                var exportRvtFileViewModel = new ExportRvtFileViewModel(uiapp.Application.VersionNumber, ExportRvtFileConfig.GetExportRvtFileConfig());

                var exportWindow = new ExportRvtFileWindow { DataContext = exportRvtFileViewModel };
                new WindowInteropHelper(exportWindow) { Owner = uiapp.MainWindowHandle };

                exportRvtFileViewModel.Owner = exportWindow;
                if(exportWindow.ShowDialog() == true) {
                    exportRvtFileViewModel = (ExportRvtFileViewModel) exportWindow.DataContext;
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

                    new DetachRevitFilesCommand() {
                        ServerName = exportRvtFileViewModel.ServerName,
                        RevitVersion = exportRvtFileViewModel.RevitVersion,
                        WithSubFolders = exportRvtFileViewModel.WithSubFolders,
                        FolderName = exportRvtFileViewModel.SourceRvtFolder,
                        TargetFolderName = exportRvtFileViewModel.TargetRvtFolder,
                        CleanTargetFolder = exportRvtFileViewModel.CleanTargetRvtFolder
                    }.Execute();

                    if(exportRvtFileViewModel.WithNwcFiles) {
                        new ExportFilesToNavisworksCommand() {
                            Application = uiapp.Application,
                            SourceFolderName = exportRvtFileViewModel.TargetRvtFolder,
                            TargetFolderName = exportRvtFileViewModel.TargetNwcFolder,
                            CleanTargetFolder = exportRvtFileViewModel.CleanTargetNwcFolder
                        }.Execute();
                    }

                    if(exportRvtFileViewModel.WithRooms) {
                        new ExportRoomFilesToNavisworksCommand() {
                            Application = uiapp.Application,
                            SourceFolderName = exportRvtFileViewModel.TargetRvtFolder,
                            TargetFolderName = exportRvtFileViewModel.TargetNwcFolder,
                            CleanTargetFolder = exportRvtFileViewModel.CleanTargetNwcFolder
                        }.Execute();
                    }

                    System.Windows.MessageBox.Show("Готово!", "Сообщение!", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                }
            } catch(Exception ex) {
#if DEBUG
                System.Windows.MessageBox.Show(ex.ToString(), "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
#else
                System.Windows.MessageBox.Show(ex.Message, "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
#endif
            }
        }
    }

    public class ExportNwcFileCommandPyRevitCommand {
        public void Execute(UIApplication uiapp) {
            try {
                var exportNwcFileViewModel = new ExportNwcFileViewModel(ExportNwcFileConfig.GetExportNwcFileConfig());
                var exportWindow = new ExportNwcFileWindow { DataContext = exportNwcFileViewModel };
                new WindowInteropHelper(exportWindow) { Owner = uiapp.MainWindowHandle };

                if(exportWindow.ShowDialog() == true) {
                    exportNwcFileViewModel = (ExportNwcFileViewModel) exportWindow.DataContext;
                    ExportNwcFileConfig.SaveExportNwcFileConfig(new ExportNwcFileConfig() {
                        CleanTargetNwcFolder = exportNwcFileViewModel.CleanTargetNwcFolder,
                        SourceNwcFolder = exportNwcFileViewModel.SourceNwcFolder,
                        TargetNwcFolder = exportNwcFileViewModel.TargetNwcFolder,
                        WithRooms = exportNwcFileViewModel.WithRooms,
                        WithSubFolders = exportNwcFileViewModel.WithSubFolders
                    });

                    new ExportFilesToNavisworksCommand() {
                        Application = uiapp.Application,
                        SourceFolderName = exportNwcFileViewModel.SourceNwcFolder,
                        TargetFolderName = exportNwcFileViewModel.TargetNwcFolder,
                        CleanTargetFolder = exportNwcFileViewModel.CleanTargetNwcFolder
                    }.Execute();

                    if(exportNwcFileViewModel.WithRooms) {
                        new ExportRoomFilesToNavisworksCommand() {
                            Application = uiapp.Application,
                            SourceFolderName = exportNwcFileViewModel.SourceNwcFolder,
                            TargetFolderName = exportNwcFileViewModel.TargetNwcFolder,
                            CleanTargetFolder = exportNwcFileViewModel.CleanTargetNwcFolder
                        }.Execute();
                    }

                    System.Windows.MessageBox.Show("Готово!", "Сообщение!", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                }
            } catch(Exception ex) {
#if DEBUG
                System.Windows.MessageBox.Show(ex.ToString(), "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
#else
                System.Windows.MessageBox.Show(ex.Message, "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
#endif
            }
        }
    }
}
