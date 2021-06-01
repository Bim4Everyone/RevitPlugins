#region Namespaces
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;

#endregion

namespace RevitServerFolders {
    public class ExportFilesToNavisworksCommand {
        public Application Application { get; set; }

        public string SourceFolderName { get; set; }
        public string TargetFolderName { get; set; }
        public bool CleanTargetFolder { get; set; }

        public void Execute() {
            if(Application == null) {
                throw new InvalidOperationException("Не было инициализировано свойство приложения Revit.");
            }

            if(string.IsNullOrEmpty(SourceFolderName)) {
                throw new InvalidOperationException("Перед использованием укажите папку с файлами Revit.");
            }

            if(string.IsNullOrEmpty(TargetFolderName)) {
                throw new InvalidOperationException("Перед использованием укажите папку сохранения 3D видов Navisworks.");
            }

            if(!OptionalFunctionalityUtils.IsNavisworksExporterAvailable()) {
                throw new Exception("Не установлено расширение экспорта в Navisworks.");
            }

            if(CleanTargetFolder) {
                if(Directory.Exists(TargetFolderName)) {
                    foreach(var navisFile in Directory.GetFiles(TargetFolderName, "*.nwc")) {
                        File.Delete(navisFile);
                    }
                }
            }

            new UnloadRevitLinksCommand() {
                Application = Application,
                SourceFolderName = SourceFolderName
            }.Execute();

            Directory.CreateDirectory(TargetFolderName);
            foreach(string fileName in Directory.EnumerateFiles(SourceFolderName, "*.rvt")) {
                var document = Application.OpenDocumentFile(fileName);
                try {
                    var exportView = new FilteredElementCollector(document)
                        .OfClass(typeof(View))
                        .ToElements()
                        .FirstOrDefault(item => item.Name.Equals("Navisworks"));

                    if(exportView == null) {
                        continue;
                    }

                    document.Export(TargetFolderName,
                        Path.GetFileNameWithoutExtension(fileName),
                        new NavisworksExportOptions {
                            ViewId = exportView.Id,
                            ExportScope = NavisworksExportScope.View,

                            ExportElementIds = false,
                            ConvertElementProperties = true,
                            Parameters = NavisworksParameters.All,
                            Coordinates = NavisworksCoordinates.Shared,
                            FacetingFactor = 1.0,
                            ExportUrls = false,
                            ConvertLights = false,
                            ExportRoomAsAttribute = false,
                            ConvertLinkedCADFormats = true,
                            ExportLinks = false,
                            ExportParts = false,
                            FindMissingMaterials = false,
                            DivideFileIntoLevels = true,
                            ExportRoomGeometry = false
                        });                    
                } finally {
                    document.Close(false);
                }
            }
        }
    }
}
