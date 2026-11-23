using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Revit;

using RevitLintelsManager.Models.Configs;

namespace RevitLintelsManager.Models;

internal partial class RevitRepository {
    // Проверяет стартовый диапазон теми же фильтрами, что и последующее чтение.
    private bool HasScopeElements(ManagerScope scope, View view, ICollection<ElementId> selection,
        ManagerConfigSnapshot config, ElementId phaseId) {
        using var collector = CreateScopeCollector(scope, view, selection);
        return collector is not null && GetMatchingFamilies(collector, config, phaseId).Any();
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
