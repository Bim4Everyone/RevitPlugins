using System;
using System.Collections.Generic;

namespace RevitLintelsManager.Models;

/// <summary>Ожидаемый контекст Revit после навигации, без живых API-объектов.</summary>
internal sealed class ManagerNavigationStamp(Guid documentToken, long generation, string viewUniqueId,
    IEnumerable<long> selection) {
    private readonly ManagerScopeStamp _view = new(ManagerScope.View, viewUniqueId, Array.Empty<long>());
    private readonly ManagerScopeStamp _selection = new(ManagerScope.Selected, null, selection);

    /// <summary>Сверяет поколение и ожидаемое изменение источника выбранного диапазона.</summary>
    public bool Matches(ManagerSessionState state, ManagerScopeStamp current) {
        return state.DocumentToken == documentToken && state.Generation == generation
            && (current.Scope == ManagerScope.View ? _view.Matches(current) : _selection.Matches(current));
    }
}
