using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;

using RevitServerFolders.Models;

namespace RevitServerFolders.Services.FailureHandlers;
/// <summary>
/// Подавляет предупреждения и устраняет ошибки удалением элементов.
/// Элементы предупреждений, описание которых подходит под
/// <see cref="NwcExportViewSettings.WarningKeepElementsTemplates"/>, не удаляются:
/// их удаление приводит к отмене открытия документа.
/// </summary>
internal class NwcFailuresPreprocessor : IFailuresPreprocessor {
    private readonly string[] _keepElementsTemplates;
    private readonly string[] _deleteElementsTemplates;
    private readonly ILoggerService _loggerService;

    public NwcFailuresPreprocessor(NwcExportViewSettings viewSettings, ILoggerService loggerService) {
        if(viewSettings is null) {
            throw new ArgumentNullException(nameof(viewSettings));
        }

        _loggerService = loggerService ?? throw new ArgumentNullException(nameof(loggerService));
        _keepElementsTemplates = viewSettings.WarningKeepElementsTemplates ?? [];
        _deleteElementsTemplates = viewSettings.WarningDeleteElementsTemplates ?? [];
    }


    public FailureProcessingResult PreprocessFailures(FailuresAccessor failuresAccessor) {
        int resolvedErrors = 0;
        foreach(var failure in failuresAccessor.GetFailureMessages()) {
            var severity = failure.GetSeverity();
            string description = failure.GetDescriptionText();

            if(severity == FailureSeverity.Warning) {
                if(ShouldDeleteElements(description)) {
                    TryDeleteElements(failuresAccessor, GetFailureElementIds(failure));
                }

                TryDeleteWarning(failuresAccessor, failure);
                continue;
            }

            if(severity == FailureSeverity.Error
               && TryResolveError(failuresAccessor, failure)) {
                resolvedErrors++;
            }
        }

        // после устранения ошибок Continue снова покажет диалог, ProceedWithCommit - нет
        return resolvedErrors > 0
            ? FailureProcessingResult.ProceedWithCommit
            : FailureProcessingResult.Continue;
    }

    private bool ShouldDeleteElements(string description) {
        if(string.IsNullOrEmpty(description)) {
            return false;
        }

        if(_keepElementsTemplates.Any(t => ContainsTemplate(description, t))) {
            return false;
        }

        return _deleteElementsTemplates.Any(t => ContainsTemplate(description, t));
    }

    private IList<ElementId> GetFailureElementIds(FailureMessageAccessor failure) {
        return [
            .. failure.GetFailingElementIds()
                .Union(failure.GetAdditionalElementIds())
        ];
    }

    private void TryDeleteWarning(FailuresAccessor failuresAccessor, FailureMessageAccessor failure) {
        try {
            failuresAccessor.DeleteWarning(failure);
        } catch(Autodesk.Revit.Exceptions.ApplicationException ex) {
            _loggerService.Warning(
                ex,
                "Не удалось подавить предупреждение: {@Description}",
                failure.GetDescriptionText());
        }
    }

    private bool TryDeleteElements(FailuresAccessor failuresAccessor, IList<ElementId> elementIds) {
        try {
            if(elementIds.Count > 0
               && failuresAccessor.IsElementsDeletionPermitted(elementIds)) {
                failuresAccessor.DeleteElements(elementIds);
                return true;
            }
        } catch(Autodesk.Revit.Exceptions.ApplicationException ex) {
            _loggerService.Warning(
                ex,
                "Не удалось удалить элементы, вызывающие ошибки: {@ElementIds}",
                elementIds.Select(id => id.ToString()));
        }

        return false;
    }

    /// <summary>
    /// Устраняет ошибку удалением элементов, а если это невозможно - любым доступным способом
    /// </summary>
    private bool TryResolveError(FailuresAccessor failuresAccessor, FailureMessageAccessor failure) {
        try {
            if(failure.HasResolutionOfType(FailureResolutionType.DeleteElements)
               && failuresAccessor.IsFailureResolutionPermitted(failure, FailureResolutionType.DeleteElements)) {
                failure.SetCurrentResolutionType(FailureResolutionType.DeleteElements);
                failuresAccessor.ResolveFailure(failure);
                return true;
            }
        } catch(Autodesk.Revit.Exceptions.ApplicationException ex) {
            _loggerService.Warning(
                ex,
                "Не удалось устранить ошибку удалением элементов: {@Description}",
                failure.GetDescriptionText());
        }

        if(TryDeleteElements(failuresAccessor, GetFailureElementIds(failure))) {
            return true;
        }

        try {
            if(failure.HasResolutions()) {
                failuresAccessor.ResolveFailure(failure);
                return true;
            }
        } catch(Autodesk.Revit.Exceptions.ApplicationException ex) {
            _loggerService.Warning(ex, "Не удалось устранить ошибку: {@Description}", failure.GetDescriptionText());
        }

        return false;
    }

    private bool ContainsTemplate(string value, string template) {
        return value.IndexOf(template, StringComparison.OrdinalIgnoreCase) >= 0;
    }
}
