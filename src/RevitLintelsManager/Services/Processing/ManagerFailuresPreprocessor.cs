using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitLintelsManager.Services;

/// <summary>Собирает причины сбоев Revit и требует отката при неразрешённой ошибке.</summary>
internal sealed class ManagerFailuresPreprocessor : IFailuresPreprocessor, ITransactionFinalizer {
    public List<string> Diagnostics { get; } = [];
    public bool IsFatal { get; private set; }
    public string UserMessage { get; set; }
    public TransactionStatus? FinalStatus { get; private set; }
    private Action _afterFinalization;

    /// <summary>Передаёт аварийное завершение группы нативному finalizer ожидающей транзакции.</summary>
    public void AfterFinalization(Action finish) {
        if(FinalStatus.HasValue) {
            finish();
        } else {
            _afterFinalization = finish;
        }
    }

    /// <inheritdoc />
    public FailureProcessingResult PreprocessFailures(FailuresAccessor accessor) {
        var severity = accessor.GetSeverity();
        IsFatal |= severity >= FailureSeverity.DocumentCorruption;
        foreach(var failure in accessor.GetFailureMessages()) {
            if(failure.GetSeverity() >= FailureSeverity.Error) {
                UserMessage ??= failure.GetDescriptionText();
            }
            Diagnostics.Add($"{failure.GetSeverity()} {failure.GetFailureDefinitionId().Guid}: {failure.GetDescriptionText()}");
        }
        return severity >= FailureSeverity.Error
            ? FailureProcessingResult.ProceedWithRollBack : FailureProcessingResult.Continue;
    }

    /// <inheritdoc />
    public void OnCommitted(Document document, string name) => Finish(TransactionStatus.Committed);

    /// <inheritdoc />
    public void OnRolledBack(Document document, string name) => Finish(TransactionStatus.RolledBack);

    // Освобождает ссылку на владельца до вызова, исключая повторное завершение.
    private void Finish(TransactionStatus status) {
        FinalStatus = status;
        var finish = _afterFinalization;
        _afterFinalization = null;
        finish?.Invoke();
    }
}
