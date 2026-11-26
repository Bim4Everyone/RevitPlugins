using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Revit;

using RevitLintelsManager.Models.Configs;
using RevitLintelsManager.Services;

using static RevitLintelsManager.Models.RevitElementRead;

namespace RevitLintelsManager.Models;

/// <summary>Захватывает диапазон сессии и читает его снимок в допустимом контексте Revit.</summary>
internal sealed class ManagerScopeReader(RevitRepository source, IProjectSnapshotReader snapshots,
    IManagerAnalysisService analysis) : IManagerScopeReader {
    private Document Document => source.Document;

    /// <summary>Запоминает исходные вид и выделение и определяет диапазон по подходящим экземплярам.</summary>
    public ManagerSessionState CaptureInitialState(ManagerConfigSnapshot config) {
        var uiDocument = source.ActiveUIDocument;
        var view = uiDocument.ActiveView;
        var selection = uiDocument.Selection.GetElementIds();
        var phaseId = source.GetReadPhaseId(config);
        var scope = HasScopeElements(ManagerScope.Selected, view, selection, config, phaseId) ? ManagerScope.Selected
            : HasScopeElements(ManagerScope.View, view, selection, config, phaseId) ? ManagerScope.View
            : ManagerScope.Project;
        return new ManagerSessionState(config, Document.Title, view?.UniqueId, GetSelectionUniqueIds(selection), scope);
    }

    /// <summary>Читает контекст действующего диапазона из проверки исходного документа на Idling.</summary>
    public ManagerScopeStamp CaptureScopeStamp(ManagerScope scope) {
        var uiDocument = source.ActiveUIDocument;
        var selection = scope == ManagerScope.Selected ? uiDocument.Selection.GetElementIds()
            : Array.Empty<ElementId>();
        string viewUniqueId = scope == ManagerScope.View ? uiDocument.ActiveView?.UniqueId : null;
        return CreateScopeStamp(scope, viewUniqueId, selection);
    }

    /// <summary>Читает снимки выбранного диапазона только из Execute обработчика ExternalEvent.</summary>
    /// <param name="app">Приложение текущего API-вызова.</param>
    /// <param name="request">Неизменяемая идентичность запроса.</param>
    /// <param name="state">Зафиксированные настройки, исходные вид и выделение сессии.</param>
    /// <returns>Независимый контекст или null при чужом/недоступном документе.</returns>
    public ManagerDocumentContext ReadDocumentContext(UIApplication app, ManagerRequest request,
        ManagerSessionState state) {
        var activeDocument = app.ActiveUIDocument;
        if(!Document.IsValidObject || activeDocument?.Document?.IsValidObject != true
            || !Document.Equals(activeDocument.Document) || Document.IsReadOnly || Document.IsModifiable) {
            return null;
        }
        var phaseId = source.GetReadPhaseId(state.Config);
        var view = activeDocument.ActiveView;
        if(request.UseInitialContext && request.Scope == ManagerScope.View) {
            view = state.InitialViewUniqueId is string initialView
                ? ReadOptional(() => Document.GetElement(initialView) as View) : null;
        }
        var selection = GetScopeSelection(activeDocument, request, state);
        using var collector = CreateScopeCollector(request.Scope, view, selection);
        var project = snapshots.ReadScopeSnapshot(state.Config, phaseId, request.Scope, collector, out _);
        var rows = project.GetRows().Select(row => {
            LintelCalculation calculation = null;
            if(row.Opening is not null) {
                project.Calculations.TryGetValue(row.Opening.Element.UniqueId, out calculation);
            }
            return analysis.Analyze(request.DocumentToken, row, calculation, state.Config);
        });
        string viewUniqueId = view?.UniqueId;
        return new ManagerDocumentContext(request.DocumentToken, request.Generation, Document.Title,
            viewUniqueId, view?.Name, GetSelectionUniqueIds(selection), project,
            CreateScopeStamp(request.Scope, viewUniqueId, selection), GetRemovedKeys(request), rows);
    }

    // Уточняет исчезновение по UniqueId, чтобы сохранить состояние после Undo до обновления.
    private IEnumerable<ManagerItemKey> GetRemovedKeys(ManagerRequest request) {
        return request.DeletionCandidates.Where(key => key.DocumentToken == request.DocumentToken
            && (IsMissing(key.OpeningUniqueId) || IsMissing(key.LintelUniqueId)));
    }

    // Проверка выполняется только в исходном документе и в том же запросе, что и чтение диапазона.
    private bool IsMissing(string uniqueId) {
        return uniqueId is not null && Document.GetElement(uniqueId)?.IsValidObject != true;
    }

    // Проверяет стартовый диапазон теми же фильтрами, что и последующее чтение.
    private bool HasScopeElements(ManagerScope scope, View view, ICollection<ElementId> selection,
        ManagerConfigSnapshot config, ElementId phaseId) {
        using var collector = CreateScopeCollector(scope, view, selection);
        return collector is not null && snapshots.HasMatchingFamilies(collector, config, phaseId);
    }

    // Собирает элементы вида по правилам Revit и проверяет актуальность ID выделения в исходном документе.
    private FilteredElementCollector CreateScopeCollector(ManagerScope scope, View view,
        ICollection<ElementId> selection) {
        switch(scope) {
            case ManagerScope.Selected:
                var ids = new HashSet<ElementId>(selection.Where(id => id is not null
                    && id != ElementId.InvalidElementId
                    && ReadOptional(() => Document.GetElement(id)?.IsValidObject == true)));
                return ids.Count == 0 ? null : new FilteredElementCollector(Document, ids);
            case ManagerScope.View:
                return view?.IsValidObject == true && FilteredElementCollector.IsViewValidForElementIteration(
                    Document, view.Id) ? new FilteredElementCollector(Document, view.Id) : null;
            case ManagerScope.Project:
                return new FilteredElementCollector(Document);
            default:
                throw new ArgumentOutOfRangeException(nameof(scope));
        }
    }

    // Восстанавливает исходные ключи только для первого запроса, затем использует текущее выделение.
    private ICollection<ElementId> GetScopeSelection(UIDocument uiDocument, ManagerRequest request,
        ManagerSessionState state) {
        if(request.Scope != ManagerScope.Selected) {
            return Array.Empty<ElementId>();
        }
        if(!request.UseInitialContext) {
            return uiDocument.Selection.GetElementIds();
        }
        return state.InitialSelectionUniqueIds.Select(key => ReadOptional(() => Document.GetElement(key)?.Id))
            .Where(id => id is not null).ToArray();
    }

    // Сохраняет ключи доступных элементов выделения для чтения стартового диапазона.
    private string[] GetSelectionUniqueIds(IEnumerable<ElementId> selection) {
        return selection.Where(id => id is not null && id != ElementId.InvalidElementId)
            .Select(id => ReadOptional(() => Document.GetElement(id)?.UniqueId))
            .Where(key => key is not null).Distinct().ToArray();
    }

    // Сохраняет только те признаки контекста, изменение которых влияет на данный диапазон.
    private static ManagerScopeStamp CreateScopeStamp(ManagerScope scope, string viewUniqueId,
        IEnumerable<ElementId> selection) {
        return new ManagerScopeStamp(scope, scope == ManagerScope.View ? viewUniqueId : null,
            scope == ManagerScope.Selected ? selection.Where(id => id is not null).Select(id => (long) id.GetIdValue())
                : Array.Empty<long>());
    }
}
