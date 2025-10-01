using System;
using System.IO;
using System.Linq;
using System.Threading;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI.Events;

using dosymep.Revit;
using dosymep.Revit.Geometry;
using dosymep.SimpleServices;

using RevitServerFolders.Models;

namespace RevitServerFolders.Services;
/// <summary>
/// Экспортирует rvt файлы в nwc в заданную директорию
/// </summary>
internal class NwcExportService : IModelsExportService<FileModelObjectExportSettings> {
    private const string _nwcSearchPattern = "*.nwc";
    private const string _navisworksViewName = "Navisworks";
    private readonly RevitRepository _revitRepository;
    private readonly ILoggerService _loggerService;
    private readonly ILocalizationService _localization;
    private readonly IErrorsService _errorsService;
    private FileModelObjectExportSettings _currentSettings;

    public NwcExportService(
        RevitRepository revitRepository,
        ILoggerService loggerService,
        ILocalizationService localization,
        IErrorsService errorsService) {
        _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
        _loggerService = loggerService ?? throw new ArgumentNullException(nameof(loggerService));
        _localization = localization ?? throw new ArgumentNullException(nameof(localization));
        _errorsService = errorsService ?? throw new ArgumentNullException(nameof(errorsService));
        _currentSettings = null;
    }


    public void ExportModelObjects(
        string[] modelFiles,
        FileModelObjectExportSettings settings,
        IProgress<int> progress = null,
        CancellationToken ct = default,
        int processStart = 0) {
        if(settings is null) {
            throw new ArgumentException(nameof(settings));
        }
        if(modelFiles is null) {
            throw new ArgumentNullException(nameof(modelFiles));
        }
        _currentSettings = settings;

        Directory.CreateDirectory(settings.TargetFolder);

        if(settings.ClearTargetFolder) {
            string[] navisFiles = Directory.GetFiles(settings.TargetFolder, _nwcSearchPattern);
            foreach(string navisFile in navisFiles) {
                File.SetAttributes(navisFile, FileAttributes.Normal);
                File.Delete(navisFile);
            }
        }

        foreach(string modelFile in modelFiles) {
            progress?.Report(processStart++);
            ct.ThrowIfCancellationRequested();

            try {
                ExportDocument(modelFile, settings);
            } catch(Autodesk.Revit.Exceptions.OperationCanceledException exCancel) {
                _loggerService.Warning(exCancel, "Отмена экспорта в nwc в файле: {@DocPath}", modelFile);
                _errorsService.AddError(modelFile,
                    _localization.GetLocalizedString("Exceptions.NwcExportCancel"),
                    settings);
            } catch(Exception ex) {
                _loggerService.Warning(ex, "Ошибка экспорта в nwc в файле: {@DocPath}", modelFile);
                _errorsService.AddError(modelFile,
                    _localization.GetLocalizedString("Exceptions.NwcExportError", ex.Message),
                    settings);
            }
        }
        _currentSettings = null;
    }


    private void ExportDocument(string fileName, FileModelObjectExportSettings settings) {
        _revitRepository.Application.FailuresProcessing += ApplicationOnFailuresProcessing;
        _revitRepository.UIApplication.DialogBoxShowing += UIApplicationOnDialogBoxShowing;

        string targetFolder = settings.TargetFolder;
        bool isExportRooms = settings.IsExportRooms;

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
                    _errorsService.AddError(fileName,
                        _localization.GetLocalizedString("Exceptions.ViewNotFound", _navisworksViewName),
                        settings);
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
                    _errorsService.AddError(fileName,
                        _localization.GetLocalizedString("Exceptions.ViewWithoutElements", navisView.Name),
                        settings);
                    return;
                }

                var projectLocations = document.ProjectLocations
                    .OfType<ProjectLocation>()
                    .ToArray();

                if(projectLocations.Length == 1) {
                    ExportDocument(fileName, navisView, document, targetFolder, isExportRooms);
                } else if(projectLocations.Length > 1) {
                    foreach(var projectLocation in projectLocations) {
                        using(var transaction = document.StartTransaction(
                            _localization.GetLocalizedString("Transaction.ChangeSite"))) {
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
                if(_currentSettings is not null) {
                    string[] ids = [.. elementIds.Select(id => id.ToString())];
                    _errorsService.AddError(accessor.GetDocument().Title,
                        _localization.GetLocalizedString(
                            "Exceptions.ElementsNotDeleted", string.Join(", ", ids), ex.Message),
                        _currentSettings);
                }
                return;
            }
            e.SetProcessingResult(FailureProcessingResult.ProceedWithCommit);
        } else {
            e.SetProcessingResult(FailureProcessingResult.Continue);
        }
    }
}
