using System.Collections.Generic;

namespace RevitLintelsManager.Models;

/// <summary>Исходные данные диапазона для сравнения с текущим контекстом Revit.</summary>
internal sealed class ManagerScopeStamp(ManagerScope scope, string viewUniqueId, IEnumerable<long> selection) {
    private readonly HashSet<long> _selection = new(selection);
    private readonly string _viewUniqueId = viewUniqueId;

    public ManagerScope Scope { get; } = scope;

    /// <summary>Проверяет совпадение вида или состава выделения без учёта порядка элементов.</summary>
    public bool Matches(ManagerScopeStamp other) {
        return Scope == other.Scope && _viewUniqueId == other._viewUniqueId && _selection.SetEquals(other._selection);
    }
}
