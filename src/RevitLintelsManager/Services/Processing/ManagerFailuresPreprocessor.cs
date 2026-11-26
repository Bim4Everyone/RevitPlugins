using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitLintelsManager.Services;

/// <summary>Собирает причины сбоев Revit и требует отката при неразрешённой ошибке.</summary>
internal sealed class ManagerFailuresPreprocessor : IFailuresPreprocessor, ITransactionFinalizer {
    public List<string> Diagnostics { get; } = [];
    public bool IsFatal { get; private set; }
    public TransactionStatus? FinalStatus { get; private set; }

    /// <inheritdoc />
    public FailureProcessingResult PreprocessFailures(FailuresAccessor accessor) {
        var severity = accessor.GetSeverity();
        IsFatal |= severity >= FailureSeverity.DocumentCorruption;
        foreach(var failure in accessor.GetFailureMessages()) {
            Diagnostics.Add($"{failure.GetSeverity()} {failure.GetFailureDefinitionId().Guid}: {failure.GetDescriptionText()}");
        }
        return severity >= FailureSeverity.Error
            ? FailureProcessingResult.ProceedWithRollBack : FailureProcessingResult.Continue;
    }

    /// <inheritdoc />
    public void OnCommitted(Document document, string name) => FinalStatus = TransactionStatus.Committed;

    /// <inheritdoc />
    public void OnRolledBack(Document document, string name) => FinalStatus = TransactionStatus.RolledBack;
}
