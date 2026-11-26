using System.Collections.Generic;
using System.Linq;

namespace RevitLintelsManager.Models;

internal enum ProcessingStatus { Succeeded, Skipped, Failed, Canceled, Pending, Fatal }

/// <summary>Фактический исход локальной операции и сообщения, пригодные после выхода из Revit.</summary>
internal sealed class ProcessingResult(ProcessingOperation operation, ProcessingStatus status,
    IEnumerable<string> diagnostics, string installedLintelUniqueId = null) {
    public ProcessingOperation Operation { get; } = operation;
    public ProcessingStatus Status { get; } = status;
    public string InstalledLintelUniqueId { get; } = installedLintelUniqueId;
    /// <summary>Ключ сохранённого проёма и нового экземпляра после успешного commit установки или замены.</summary>
    public ManagerItemKey? InstalledPairKey => InstalledLintelUniqueId is null ? null
        : new ManagerItemKey(Operation.Key.DocumentToken, Operation.Key.OpeningUniqueId, InstalledLintelUniqueId);
    public IReadOnlyList<string> Diagnostics { get; } = diagnostics.ToList().AsReadOnly();
    public bool MustStop => Status is ProcessingStatus.Pending or ProcessingStatus.Fatal or ProcessingStatus.Canceled;
}
