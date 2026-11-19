using System;
using System.Collections.Generic;
using System.Linq;

namespace RevitLintelsManager.Models;

/// <summary>Результат чтения контекста; не содержит объектов Revit API.</summary>
internal sealed class ManagerDocumentContext(Guid documentToken, long generation, string documentTitle,
    string viewUniqueId, string viewName, IEnumerable<string> selectionUniqueIds) {
    public Guid DocumentToken { get; } = documentToken;
    public long Generation { get; } = generation;
    public string DocumentTitle { get; } = documentTitle;
    public string ViewUniqueId { get; } = viewUniqueId;
    public string ViewName { get; } = viewName;
    public IReadOnlyList<string> SelectionUniqueIds { get; } = selectionUniqueIds.ToList().AsReadOnly();
}
