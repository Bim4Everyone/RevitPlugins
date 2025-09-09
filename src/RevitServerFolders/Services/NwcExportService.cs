using System;
using System.IO;
using System.Linq;
using System.Threading;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI.Events;

using dosymep.Bim4Everyone.ProjectConfigs;
using dosymep.Revit;
using dosymep.Revit.Geometry;
using dosymep.SimpleServices;

using RevitServerFolders.Models;

namespace RevitServerFolders.Services;
/// <summary>
/// Экспортирует rvt файлы в nwc в заданную директорию
/// </summary>
internal class NwcExportService : IModelsExportService {
    private const string _nwcSearchPattern = "*.nwc";
    private const string _navisworksViewName = "Navisworks";
    private const string _transactionName = "Смена площадки";
    private readonly RevitRepository _revitRepository;
    private readonly ILoggerService _loggerService;
    private readonly IConfigSerializer _configSerializer;

    public NwcExportService(
        RevitRepository revitRepository,
        ILoggerService loggerService,
        IConfigSerializer configSerializer) {
        _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
        _loggerService = loggerService ?? throw new ArgumentNullException(nameof(loggerService));
        _configSerializer = configSerializer ?? throw new ArgumentNullException(nameof(configSerializer));
    }


    public void ExportModelObjects(
        string targetFolder,
        string[] modelFiles,
        bool clearTargetFolder = false,
        IProgress<int> progress = null,
        CancellationToken ct = default) {
        if(string.IsNullOrWhiteSpace(targetFolder)) {
            throw new ArgumentException(nameof(targetFolder));
        }
        if(modelFiles is null) {
            throw new ArgumentNullException(nameof(modelFiles));
        }

        Directory.CreateDirectory(targetFolder);

        if(clearTargetFolder) {
            string[] navisFiles = Directory.GetFiles(targetFolder, _nwcSearchPattern);
            foreach(string navisFile in navisFiles) {
                File.SetAttributes(navisFile, FileAttributes.Normal);
                File.Delete(navisFile);
            }
        }

        for(int i = 0; i < modelFiles.Length; i++) {
            progress?.Report(i);
            ct.ThrowIfCancellationRequested();

            var config = FileModelObjectConfig.GetPluginConfig(_configSerializer);
            try {
                ExportDocument(modelFiles[i], targetFolder, config.IsExportRooms);
            } catch(Exception ex) {
                _loggerService.Warning(ex, "Ошибка экспорта в nwc в файле: {@DocPath}", modelFiles[i]);
            }
        }
    }


    private void ExportDocument(string fileName, string targetFolder, bool isExportRooms) {
        _revitRepository.Application.FailuresProcessing += ApplicationOnFailuresProcessing;
        _revitRepository.UIApplication.DialogBoxShowing += UIApplicationOnDialogBoxShowing;

        try {
            DocumentExtensions.UnloadAllLinks(fileName);
            using var document = _revitRepository.OpenDocumentFile(fileName);
            try {
                var navisView = new FilteredElementCollector(document)
                    .OfClass(typeof(View3D))
                    .OfType<View3D>()
                    .Where(item => !item.IsTemplate)
                    .FirstOrDefault(item =>
                        item.Name.Equals(_navisworksViewName, StringComparison.OrdinalIgnoreCase));

                if(navisView == null) {
                    _loggerService.Warning(
                        "Файл {@FileName} не содержит вид {@NavisView}.",
                        fileName, _navisworksViewName);
                    return;
                }

                bool hasElements = new FilteredElementCollector(document, navisView.Id)
                        .WhereElementIsNotElementType()
#if REVIT_2021_OR_LESS
                    .Where(item =>
                        item.get_Geometry(new Options() {View = navisView})?.Any() == true)
#else
                        .WherePasses(new VisibleInViewFilter(document, navisView.Id))
#endif
                        .Any(e => e.Category != null && ElementHasGeometry(e)); // Ищем геометрию на виде

                if(!hasElements) {
                    _loggerService.Warning(
                        "Вид {@NavisView} в файле {@FileName} не содержит элементы.",
                         navisView.Name, fileName);
                    return;
                }

                var projectLocations = document.ProjectLocations
                    .OfType<ProjectLocation>()
                    .ToArray();

                if(projectLocations.Length == 1) {
                    ExportDocument(fileName, navisView, document, targetFolder, isExportRooms);
                } else if(projectLocations.Length > 1) {
                    foreach(var projectLocation in projectLocations) {
                        using(var transaction = document.StartTransaction(_transactionName)) {
                            document.ActiveProjectLocation = projectLocation;
                            transaction.Commit();
                        }

                        ExportDocument(fileName, navisView, document, targetFolder, isExportRooms, projectLocation);
                    }
                }
            } finally {
                document.Close(false);
            }
        } finally {
            _revitRepository.Application.FailuresProcessing -= ApplicationOnFailuresProcessing;
            _revitRepository.UIApplication.DialogBoxShowing -= UIApplicationOnDialogBoxShowing;
        }
    }

    private bool ElementHasGeometry(Element element) {
        try {
            return element.GetSolids().Any(s => s?.Volume > 0);
        } catch(Exception ex) when(
        ex is System.ArgumentNullException
        or System.NullReferenceException
        or Autodesk.Revit.Exceptions.ApplicationException) {
            return false;
        }
    }

    private void ExportDocument(string fileName,
        View3D navisView,
        Document document,
        string targetFolder,
        bool isExportRooms,
        ProjectLocation location = null) {
        string exportFileName = isExportRooms
            ? _revitRepository.GetRoomsFileName(fileName)
            : _revitRepository.GetFileName(fileName);

        var exportOptions = isExportRooms
            ? _revitRepository.GetRoomsExportOptions(navisView)
            : _revitRepository.GetExportOptions(navisView);

        exportFileName += location == null
            ? null
            : "_" + location.Name;

        document.Export(targetFolder, exportFileName, exportOptions);
    }

    private void UIApplicationOnDialogBoxShowing(object sender, DialogBoxShowingEventArgs e) {
        _loggerService.Information(
            "Event handler: {@EventHandler}; DialogId: {@DialogId}",
            nameof(UIApplicationOnDialogBoxShowing), e.DialogId);
        e.OverrideResult(1);
    }

    private void ApplicationOnFailuresProcessing(object sender, FailuresProcessingEventArgs e) {
        var accessor = e.GetFailuresAccessor();
        var failureMessages = accessor.GetFailureMessages()
            .GroupBy(m => m.GetDescriptionText())
            .Select(g => new {
                Description = g.Key,
                Count = g.Count(),
                Severity = g.FirstOrDefault()?.GetSeverity().ToString()
            })
            .ToArray();

        if(failureMessages.Length == 0) {
            return;
        }
        _loggerService.Information(
            "Event handler: {@EventHandler}; title: {@DocTitle}; Failures: {@Failures}",
            nameof(ApplicationOnFailuresProcessing), accessor.GetDocument().Title, failureMessages);

        accessor.DeleteAllWarnings();
        accessor.ResolveFailures(accessor.GetFailureMessages());

        var elementIds = accessor.GetFailureMessages()
            .SelectMany(item => item.GetFailingElementIds())
            .ToArray();

        if(elementIds.Length > 0) {
            try {
                accessor.DeleteElements(elementIds);
            } catch(Exception ex) {
                _loggerService.Warning(ex, $"Не удалось удалить элементы, вызывающие ошибки");
                e.SetProcessingResult(FailureProcessingResult.ProceedWithRollBack);
                return;
            }
            e.SetProcessingResult(FailureProcessingResult.ProceedWithCommit);
        } else {
            e.SetProcessingResult(FailureProcessingResult.Continue);
        }
    }
}
