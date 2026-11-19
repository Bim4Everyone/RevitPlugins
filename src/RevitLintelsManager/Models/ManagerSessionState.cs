using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using RevitLintelsManager.Models.Configs;

namespace RevitLintelsManager.Models;

/// <summary>Состояние запуска без объектов Revit API.</summary>
internal sealed class ManagerSessionState(ManagerConfigSnapshot config, string documentTitle,
    string initialViewUniqueId, IEnumerable<string> initialSelectionUniqueIds) {
    private long _generation;
    public Guid DocumentToken { get; } = Guid.NewGuid();
    public ManagerConfigSnapshot Config { get; } = config;
    public string DocumentTitle { get; } = documentTitle;
    public string InitialViewUniqueId { get; } = initialViewUniqueId;
    public IReadOnlyList<string> InitialSelectionUniqueIds { get; } = initialSelectionUniqueIds.ToList().AsReadOnly();
    public bool IsBlocked { get; internal set; }
    public bool IsClosing { get; internal set; }
    public long Generation => Interlocked.Read(ref _generation);

    /// <summary>Инвалидирует прежние запросы и возвращает следующее поколение данных.</summary>
    public long AdvanceGeneration() {
        return Interlocked.Increment(ref _generation);
    }
}
