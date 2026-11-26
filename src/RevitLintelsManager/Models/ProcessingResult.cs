using System.Collections.Generic;
using System.Linq;

namespace RevitLintelsManager.Models;

internal enum ProcessingStatus { Succeeded, Skipped, Failed, Canceled, Pending, Fatal }

/// <summary>Фактический исход локальной операции и сообщения, пригодные после выхода из Revit.</summary>
internal sealed class ProcessingResult(ProcessingOperation operation, ProcessingStatus status,
    IEnumerable<string> diagnostics) {
    public ProcessingOperation Operation { get; } = operation;
    public ProcessingStatus Status { get; } = status;
    public IReadOnlyList<string> Diagnostics { get; } = diagnostics.ToList().AsReadOnly();
    public bool MustStop => Status is ProcessingStatus.Pending or ProcessingStatus.Fatal or ProcessingStatus.Canceled;
}
