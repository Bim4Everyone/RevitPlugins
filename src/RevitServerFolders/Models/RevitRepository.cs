using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Revit.FileInfo;

namespace RevitServerFolders.Models;
internal class RevitRepository {
    private const string _roomsSuffix = "_ROOMS";

    public RevitRepository(UIApplication uiApplication) {
        UIApplication = uiApplication;
    }

    public UIApplication UIApplication { get; }
    public UIDocument ActiveUIDocument => UIApplication.ActiveUIDocument;

    public Application Application => UIApplication.Application;
    public Document Document => ActiveUIDocument.Document;

    /// <summary>
    /// Открывает документ отсоединенным от центральной модели
    /// </summary>
    /// <param name="fileName">Полный путь к документу</param>
    /// <param name="worksetHideTemplates">
    /// Рабочие наборы, в названиях которых содержится подстрока из коллекции, не будут открыты
    /// </param>
    public Document OpenDocumentFile(string fileName, string[] worksetHideTemplates = null) {
        var modelPath = ModelPathUtils.ConvertUserVisiblePathToModelPath(fileName);
        var opts = new OpenOptions() {
            AllowOpeningLocalByWrongUser = true,
            OpenForeignOption = OpenForeignOption.Open,
            Audit = false
        };
        if(modelPath.ServerPath
           || IsWorkshared(fileName)) {
            opts.DetachFromCentralOption = DetachFromCentralOption.DetachAndPreserveWorksets;
            opts.SetOpenWorksetsConfiguration(GetWorksetConfiguration(modelPath, worksetHideTemplates));
        }
        return Application.OpenDocumentFile(
            modelPath,
            opts);
    }

    public string GetFileName(string fileName) {
        return Path.GetFileNameWithoutExtension(fileName);
    }

    public string GetRoomsFileName(string fileName) {
        return Path.GetFileNameWithoutExtension(fileName) + _roomsSuffix;
    }

    public NavisworksExportOptions GetExportOptions(View exportView) {
        return new NavisworksExportOptions {
            ViewId = exportView.Id,
            ExportScope = NavisworksExportScope.View,
            Parameters = NavisworksParameters.All,
            Coordinates = NavisworksCoordinates.Shared,
            FacetingFactor = 5.0,
            ExportElementIds = true,
            ConvertElementProperties = true,
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

    public NavisworksExportOptions GetRoomsExportOptions(View exportView) {
        var exportOptions = GetExportOptions(exportView);
        exportOptions.ExportRoomGeometry = true;
        exportOptions.ExportRoomAsAttribute = true;

        return exportOptions;
    }

    /// <summary>
    /// Проверяет, является ли документ совместным.
    /// </summary>
    private bool IsWorkshared(string fileName) {
        try {
            return BasicFileInfo.Extract(fileName).IsWorkshared;
        } catch(Autodesk.Revit.Exceptions.ApplicationException) {
            try {
                return new RevitFileInfo(fileName).BasicFileInfo.IsWorkshared;
            } catch(ArgumentException) {
                return false;
            }
        }
    }

    /// <summary>
    /// Возвращает конфигурацию, в которой открыты все рабочие наборы,
    /// кроме наборов с подстрокой из <paramref name="worksetHideTemplates"/> в названии
    /// </summary>
    private WorksetConfiguration GetWorksetConfiguration(ModelPath modelPath, string[] worksetHideTemplates) {
        if(worksetHideTemplates is null
           || worksetHideTemplates.Length == 0) {
            return new WorksetConfiguration(WorksetConfigurationOption.OpenAllWorksets);
        }

        try {
            IList<WorksetId> worksetsToOpen = WorksharingUtils.GetUserWorksetInfo(modelPath)
                .Where(workset => !worksetHideTemplates.Any(template =>
                    workset.Name.IndexOf(template, StringComparison.OrdinalIgnoreCase) >= 0))
                .Select(workset => workset.Id)
                .ToList();

            var config = new WorksetConfiguration(WorksetConfigurationOption.CloseAllWorksets);
            config.Open(worksetsToOpen);
            return config;
        } catch(Autodesk.Revit.Exceptions.ApplicationException) {
            return new WorksetConfiguration(WorksetConfigurationOption.OpenAllWorksets);
        }
    }
}
