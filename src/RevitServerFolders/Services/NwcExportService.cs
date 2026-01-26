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

        var viewSettings = settings.GetNwcExportViewSettings();
        Document viewTemplateDoc;
        try {
            viewTemplateDoc = _revitRepository.OpenDocumentFile(viewSettings.RvtFilePath);
        } catch(Autodesk.Revit.Exceptions.ApplicationException ex) {
            _loggerService.Warning(ex, "Не удалось открыть файл с шаблоном вида: {@Path}", viewSettings.RvtFilePath);
            _errorsService.AddError(viewSettings.RvtFilePath,
                _localization.GetLocalizedString("Exceptions.CannotOpenView3dTemplateFile",
                    viewSettings.RvtFilePath, ex.Message),
                settings);
            return;
        }
        View3D sourceViewTemplate;
        try {
            sourceViewTemplate = GetView3dTemplate(viewTemplateDoc, viewSettings.ViewTemplateName);
        } catch(InvalidOperationException ex) {
            _loggerService.Warning(ex, "Не найден шаблон вида. Настройки генерации 3D вида: {@Settings}.",
                viewSettings);
            _errorsService.AddError(viewSettings.RvtFilePath,
                _localization.GetLocalizedString("Exceptions.CannotFindView3dTemplate",
                viewSettings.RvtFilePath,
                viewSettings.ViewTemplateName),
                settings);
            return;
        }
        try {
            foreach(string modelFile in modelFiles) {
                progress?.Report(processStart++);
                ct.ThrowIfCancellationRequested();

                try {
                    ExportDocument(modelFile, settings, viewSettings, sourceViewTemplate);
                } catch(Autodesk.Revit.Exceptions.OperationCanceledException exCancel) {
                    _loggerService.Warning(exCancel, "Отмена экспорта в nwc в файле: {@DocPath}", modelFile);
                    _errorsService.AddError(modelFile,
                        _localization.GetLocalizedString("Exceptions.NwcExportCancel"),
                        settings);
                } catch(InvalidOperationException exInv) {
                    _loggerService.Warning(exInv, "Ошибка генерации 3D вида для экспорта в nwc в файле: {@DocPath}. " +
                        "Настройки генерации 3D вида: {@Settings}", modelFile, viewSettings);
                    _errorsService.AddError(modelFile,
                        _localization.GetLocalizedString("Exceptions.ExportViewError",
                        viewSettings.RvtFilePath,
                        viewSettings.ViewTemplateName,
                        viewSettings.WorksetHideTemplates,
                        exInv.Message),
                        settings);
                } catch(Exception ex) {
                    _loggerService.Warning(ex, "Ошибка экспорта в nwc в файле: {@DocPath}", modelFile);
                    _errorsService.AddError(modelFile,
                        _localization.GetLocalizedString("Exceptions.NwcExportError", ex.Message),
                        settings);
                }
            }
        } finally {
            viewTemplateDoc?.Close(false);
            viewTemplateDoc?.Dispose();
        }
        _currentSettings = null;
    }


    private void ExportDocument(string fileName,
        FileModelObjectExportSettings settings,
        NwcExportViewSettings exportViewSettings,
        View3D sourceTemplate) {
        _revitRepository.Application.FailuresProcessing += ApplicationOnFailuresProcessing;
        _revitRepository.UIApplication.DialogBoxShowing += UIApplicationOnDialogBoxShowing;

        string targetFolder = settings.TargetFolder;
        bool isExportRooms = settings.IsExportRooms;

        try {
            DocumentExtensions.UnloadAllLinks(fileName);
            using var document = _revitRepository.OpenDocumentFile(fileName);
            try {
                var exportView = CreateExportView3d(document, sourceTemplate, exportViewSettings.WorksetHideTemplates);
                using var t = document.StartTransaction("Regenerate");
                document.Regenerate();
                t.Commit();
                
                var elementsOnView = new FilteredElementCollector(document, exportView.Id)
                    .WhereElementIsNotElementType();
#if REVIT_2021_OR_LESS
                var visibleElements = elementsOnView.Where(item =>
                        item.get_Geometry(new Options() { View = exportView })?.Any() == true)
                    .ToArray();
#else
                var visibleElements = elementsOnView.WherePasses(new VisibleInViewFilter(document, exportView.Id));
#endif
                bool hasElements =
                    visibleElements.Any(e => e.Category != null && ElementHasGeometry(e)); // Ищем геометрию на виде

                if(!hasElements) {
                    _loggerService.Warning(
                        "Экспортируемый вид в файле {@FileName} не содержит элементы с геометрией. "
                        + "Элементов на виде: {@viewCount}, видимых элементов на виде: {@visibleElements} "
                        + "Настройки генерации 3D вида: {@Settings}",
                        fileName,
                        elementsOnView.Count(),
                        visibleElements.Count(),
                        exportViewSettings);
                    _errorsService.AddError(fileName,
                        _localization.GetLocalizedString("Exceptions.ViewWithoutElements",
                            exportViewSettings.RvtFilePath,
                            exportViewSettings.ViewTemplateName,
                            string.Join(", ", exportViewSettings.WorksetHideTemplates)),
                        settings);
                    return;
                }

                var projectLocations = document.ProjectLocations
                    .OfType<ProjectLocation>()
                    .ToArray();

                if(projectLocations.Length == 1) {
                    ExportDocument(fileName, exportView, document, targetFolder, isExportRooms);
                } else if(projectLocations.Length > 1) {
                    foreach(var projectLocation in projectLocations) {
                        using(var transaction = document.StartTransaction(
                            _localization.GetLocalizedString("Transaction.ChangeSite"))) {
                            document.ActiveProjectLocation = projectLocation;
                            transaction.Commit();
                        }

                        ExportDocument(fileName, exportView, document, targetFolder, isExportRooms, projectLocation);
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

    private View3D GetView3dTemplate(Document viewTemplateDoc, string templateName) {
        var view = new FilteredElementCollector(viewTemplateDoc)
            .WhereElementIsNotElementType()
            .OfClass(typeof(View3D))
            .OfType<View3D>()
            .FirstOrDefault(v => v.IsTemplate && v.Name.Equals(templateName, StringComparison.OrdinalIgnoreCase));
        if(view is null) {
            throw new InvalidOperationException(
                _localization.GetLocalizedString("Exceptions.CannotFindView3dTemplate"));
        }
        return view;
    }

    private View3D CopyView3dTemplate(Document destinationDocument, View3D sourceView3dTemplate) {
        var copyElements = ElementTransformUtils.CopyElements(
            sourceView3dTemplate.Document,
            [sourceView3dTemplate.Id],
            destinationDocument,
            Transform.Identity,
            new CopyPasteOptions());
        var copiedViewId = copyElements.FirstOrDefault();
        if(copiedViewId.IsNull()) {
            throw new InvalidOperationException(
                _localization.GetLocalizedString("Exceptions.CannotCopyView3dTemplate"));
        }
        return (View3D) destinationDocument.GetElement(copiedViewId);
    }

    private View3D CreateDefaultView3d(Document document) {
        var viewFamilyType = new FilteredElementCollector(document)
            .OfClass(typeof(ViewFamilyType))
            .OfType<ViewFamilyType>()
            .FirstOrDefault(x => x.ViewFamily == ViewFamily.ThreeDimensional);

        if(viewFamilyType is null) {
            throw new InvalidOperationException(
                _localization.GetLocalizedString("Exceptions.CannotFind3dFamilyType"));
        }

        return View3D.CreateIsometric(document, viewFamilyType.Id);
    }

    private View3D CreateExportView3d(Document destinationDocument, View3D sourceViewTemplate, string[] hideWorksets) {
        using var t = destinationDocument.StartTransaction(
            _localization.GetLocalizedString("Transaction.CreateExportView"));

        var view = CreateDefaultView3d(destinationDocument);
        var viewTemplate = CopyView3dTemplate(destinationDocument, sourceViewTemplate);
        view.ViewTemplateId = viewTemplate.Id;
        view.SetParamValue(BuiltInParameter.VIEW_PHASE, GetLatestPhaseId(destinationDocument));

        var worksets = new FilteredWorksetCollector(destinationDocument).OfKind(WorksetKind.UserWorkset).ToWorksets();
        foreach(var workset in worksets) {
            var visibility = hideWorksets.Any(p => workset.Name.IndexOf(p, StringComparison.OrdinalIgnoreCase) >= 0)
                ? WorksetVisibility.Hidden
                : WorksetVisibility.Visible;
            viewTemplate.SetWorksetVisibility(workset.Id, visibility);
        }

        t.Commit();
        return view;
    }

    private ElementId GetLatestPhaseId(Document document) {
        return new FilteredElementCollector(document)
            .OfCategory(BuiltInCategory.OST_Phases)
            .OfType<Phase>()
            .OrderByDescending(item => item.GetParamValueOrDefault<int>(BuiltInParameter.PHASE_SEQUENCE_NUMBER, 0))
            .FirstOrDefault()?.Id ?? ElementId.InvalidElementId;
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
