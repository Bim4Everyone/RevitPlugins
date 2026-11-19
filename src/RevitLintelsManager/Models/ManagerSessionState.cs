using System;
using System.Collections.Generic;
using System.Linq;

using RevitLintelsManager.Models.Configs;

namespace RevitLintelsManager.Models;

/// <summary>Состояние запуска без объектов Revit API.</summary>
internal sealed class ManagerSessionState(ManagerConfigSnapshot config, string documentTitle,
    string initialViewUniqueId, IEnumerable<string> initialSelectionUniqueIds) {
    public Guid DocumentToken { get; } = Guid.NewGuid();
    public ManagerConfigSnapshot Config { get; } = config;
    public string DocumentTitle { get; } = documentTitle;
    public string InitialViewUniqueId { get; } = initialViewUniqueId;
    public IReadOnlyList<string> InitialSelectionUniqueIds { get; } = initialSelectionUniqueIds.ToList().AsReadOnly();
    public bool IsBlocked { get; internal set; }
    public bool IsClosing { get; internal set; }
}
