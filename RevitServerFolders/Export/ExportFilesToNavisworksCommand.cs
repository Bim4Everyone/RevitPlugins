#region Namespaces
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;

#endregion

namespace RevitServerFolders.Export {
    public class ExportFilesToNavisworksCommand {
        public Application Application { get; set; }

        public string SourceFolderName { get; set; }
        public string TargetFolderName { get; set; }

        public bool WithRooms { get; set; }
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
                throw new InvalidOperationException("Не установлено расширение экспорта в Navisworks.");
            }

            if(CleanTargetFolder) {
                if(Directory.Exists(TargetFolderName)) {
                    foreach(var navisFile in Directory.GetFiles(TargetFolderName, "*.nwc")) {
                        File.Delete(navisFile);
                    }
                }
            }

            Directory.CreateDirectory(TargetFolderName);
            foreach(string fileName in Directory.EnumerateFiles(SourceFolderName, "*.rvt")) {
                var document = Application.OpenDocumentFile(fileName);
                try {
                    var exportView = new FilteredElementCollector(document)
                        .OfClass(typeof(View3D))
                        .OfType<View3D>()
                        .Where(item => !item.IsTemplate)
                        .FirstOrDefault(item => item.Name.Equals("Navisworks", StringComparison.CurrentCultureIgnoreCase));

                    if(exportView == null) {
                        continue;
                    }

                    document.Export(TargetFolderName, GetFileName(fileName), GetExportOptions(exportView));
                    if(WithRooms) {
                        document.Export(TargetFolderName, GetRoomsFileName(fileName), GetRoomsExportOptions(exportView));
                    }
                } finally {
                    document.Close(false);
                }
            }
        }

        private string GetFileName(string fileName) {
            return Path.GetFileNameWithoutExtension(fileName);
        }

        private NavisworksExportOptions GetExportOptions(Element exportView) {
            return new NavisworksExportOptions {
                ViewId = exportView.Id,
                ExportScope = NavisworksExportScope.View,

                ExportElementIds = true,
                ConvertElementProperties = true,
                Parameters = NavisworksParameters.All,
                Coordinates = NavisworksCoordinates.Shared,
                FacetingFactor = 5.0,
                ExportUrls = false,
                ConvertLights = false,
                ExportRoomAsAttribute = false,
                ConvertLinkedCADFormats = false,
                ExportLinks = false,
                ExportParts = false,
                FindMissingMaterials = false,
                DivideFileIntoLevels = true,
                ExportRoomGeometry = false
            };
        }

        private string GetRoomsFileName(string fileName) {
            return GetFileName(fileName) + "_ROOMS";
        }

        private NavisworksExportOptions GetRoomsExportOptions(Element exportView) {
            var exportOptions = GetExportOptions(exportView);
            exportOptions.ExportRoomGeometry = true;
            exportOptions.ExportRoomAsAttribute = true;

            return exportOptions;
        }
    }
}
