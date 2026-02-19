using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

using Autodesk.Revit.DB;

using dosymep.Revit.FileInfo;
using dosymep.SimpleServices;

using RevitBatchSpecExport.Models;

namespace RevitBatchSpecExport.Services;

internal class DocumentsProcessor : IDocumentsProcessor {
    private readonly RevitRepository _revitRepository;
    private readonly IConstantsProvider _constantsProvider;
    private readonly IErrorMessagesProvider _errorMessagesProvider;
    private readonly IDataExporter _dataExporter;
    private readonly ILocalizationService _localizationService;

    public DocumentsProcessor(
        RevitRepository revitRepository,
        IConstantsProvider constantsProvider,
        IErrorMessagesProvider errorMessagesProvider,
        IDataExporter dataExporter,
        ILocalizationService localizationService) {
        _revitRepository = revitRepository
                           ?? throw new ArgumentNullException(nameof(revitRepository));
        _constantsProvider = constantsProvider
                             ?? throw new ArgumentNullException(nameof(constantsProvider));
        _errorMessagesProvider = errorMessagesProvider
                                 ?? throw new ArgumentNullException(nameof(errorMessagesProvider));
        _dataExporter = dataExporter
                        ?? throw new ArgumentNullException(nameof(dataExporter));
        _localizationService = localizationService
                               ?? throw new ArgumentNullException(nameof(localizationService));
    }

    public string ProcessDocuments(
        DirectoryInfo rootExportDir,
        ICollection<IDocument> documents,
        PluginConfig config,
        IProgress<int> progress = null,
        CancellationToken ct = default) {
        List<string> errors = [];
        var notConflictedDocs = GetNotConflictedDocuments(documents, out string errorDocs);
        if(!string.IsNullOrWhiteSpace(errorDocs)) {
            errors.Add(errorDocs);
        }

        var options = GetOpenOptions();
        int i = 0;
        foreach(var documentToProcess in notConflictedDocs) {
            ct.ThrowIfCancellationRequested();
            progress?.Report(i++);
            var modelPath = ModelPathUtils.ConvertUserVisiblePathToModelPath(documentToProcess.Path);
            try {
                using var document = _revitRepository.Application.OpenDocumentFile(modelPath, options);
                var docDir = rootExportDir.CreateSubdirectory(document.Title);
                _dataExporter.ExportData(docDir, document, config);
                try {
                    document.Close(false);
                } catch(Autodesk.Revit.Exceptions.InvalidOperationException) {
                    continue;
                }
            } catch(Autodesk.Revit.Exceptions.CannotOpenBothCentralAndLocalException) {
                errors.Add(_errorMessagesProvider.GetFileAlreadyOpenedMessage(documentToProcess.Path));
            } catch(Autodesk.Revit.Exceptions.CorruptModelException) {
                string test = new RevitFileInfo(documentToProcess.Path).BasicFileInfo.AppInfo.Format;
                if(new RevitFileInfo(documentToProcess.Path).BasicFileInfo.AppInfo.Format
                   != _revitRepository.Application.VersionNumber) {
                    errors.Add(_errorMessagesProvider.GetFileVersionIsInvalidMessage(documentToProcess.Path));
                } else {
                    errors.Add(_errorMessagesProvider.GetFileDataCorruptionMessage(documentToProcess.Path));
                }
            } catch(Autodesk.Revit.Exceptions.FileAccessException) {
                errors.Add(_errorMessagesProvider.GetFileCannotBeProcessMessage(documentToProcess.Path));
            } catch(Autodesk.Revit.Exceptions.FileNotFoundException) {
                errors.Add(_errorMessagesProvider.GetFileRemovedMessage(documentToProcess.Path));
            } catch(Autodesk.Revit.Exceptions.InsufficientResourcesException) {
                throw new OperationCanceledException(_errorMessagesProvider.GetInsufficientResourcesMessage());
            } catch(Autodesk.Revit.Exceptions.InvalidOperationException) {
                errors.Add(_errorMessagesProvider.GetFileCannotBeProcessMessage(documentToProcess.Path));
            }
        }

        return errors.Count > 0 ? string.Join(Environment.NewLine, errors) : string.Empty;
    }

    private OpenOptions GetOpenOptions() {
        var options = new OpenOptions() { DetachFromCentralOption = DetachFromCentralOption.DetachAndPreserveWorksets };
        options.SetOpenWorksetsConfiguration(
            new WorksetConfiguration(WorksetConfigurationOption.CloseAllWorksets));
        return options;
    }

    /// <summary>
    /// Проверяет документы на конфликты имен и возвращает коллекцию документов из заданной коллекции,
    /// которые НЕ образуют конфликты и из имен документов можно сделать названия листов Excel.
    /// Правила именования листов Excel:
    /// https://support.microsoft.com/en-us/office/rename-a-worksheet-3f1f7148-ee83-404d-8ef0-9ff99fbad1f9
    /// </summary>
    /// <param name="data"></param>
    /// <param name="errorMessage">Сообщение об ошибке, или пустая строка, если ошибок нет</param>
    /// <returns></returns>
    private ICollection<IDocument> GetNotConflictedDocuments(
        ICollection<IDocument> data,
        out string errorMessage) {
        var docsWithNameConflicts = data
            .GroupBy(doc => string.Concat(doc.Name.Take(_constantsProvider.DocNameMaxLength)))
            .Where(group => group.Count() > 1)
            .SelectMany(group => group)
            .ToArray();
        errorMessage = docsWithNameConflicts.Length > 0
            ? _errorMessagesProvider
                .GetFileNamesConflictMessage(docsWithNameConflicts.Select(doc => doc.Name).ToArray())
            : string.Empty;
        return data.Except(docsWithNameConflicts).ToArray();
    }
}
