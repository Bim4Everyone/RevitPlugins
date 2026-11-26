using System;
using System.Threading;

using Autodesk.Revit.DB;
using Autodesk.Revit.Exceptions;

using dosymep.SimpleServices;

using RevitLintelsManager.Models;

namespace RevitLintelsManager.Services;

/// <summary>Владеет транзакцией одного назначения до известного конечного статуса.</summary>
internal sealed class ProcessingTransaction(ProcessingOperation operation, Transaction transaction,
    ILocalizationService localization) {
    private readonly ManagerFailuresPreprocessor _failures = new();
    private ProcessingStatus _rollbackStatus = ProcessingStatus.Failed;
    private bool _rollbackRequested;

    /// <summary>Выполняет изменение, регенерацию и проверку до единственного Commit.</summary>
    public ProcessingResult Run(ProcessingTarget target, ProcessingAction action, CancellationToken token,
        Func<bool> isCurrent) {
        ProcessingResult result;
        try {
            var start = transaction.Start();
            if(start != TransactionStatus.Started) {
                result = End(start, ProcessingStatus.Fatal);
            } else {
                var options = transaction.GetFailureHandlingOptions();
                options.SetClearAfterRollback(true).SetForcedModalHandling(true)
                    .SetFailuresPreprocessor(_failures).SetTransactionFinalizer(_failures);
                transaction.SetFailureHandlingOptions(options);
                if(!target.Document.IsModifiable) {
                    throw new System.InvalidOperationException(localization.GetLocalizedString("Processing.DocumentReadOnly"));
                }
                token.ThrowIfCancellationRequested();
                if(!isCurrent()) {
                    throw new System.OperationCanceledException(localization.GetLocalizedString("Processing.StaleOrCanceled"));
                }
                action.Apply(target, token);
                token.ThrowIfCancellationRequested();
                target.Document.Regenerate();
                action.Verify(target);
                token.ThrowIfCancellationRequested();
                result = isCurrent() ? End(transaction.Commit(), ProcessingStatus.Failed)
                    : Abort(ProcessingStatus.Canceled, localization.GetLocalizedString("Processing.StaleOrCanceled"));
            }
        } catch(Exception error) {
            var status = error switch {
                RegenerationFailedException or InternalException or ModificationForbiddenException => ProcessingStatus.Fatal,
                System.OperationCanceledException or Autodesk.Revit.Exceptions.OperationCanceledException => ProcessingStatus.Canceled,
                _ => ProcessingStatus.Failed
            };
            result = Abort(status, error.ToString());
        }
        return Release(result);
    }

    /// <summary>Проверяет состояние после возврата finalizer в следующем разрешённом контексте.</summary>
    public ProcessingResult FinishPending() {
        if(!_failures.FinalStatus.HasValue) {
            return new ProcessingResult(operation, ProcessingStatus.Pending, _failures.Diagnostics);
        }
        try {
            return Release(End(_failures.FinalStatus.Value, _rollbackStatus));
        } catch(Exception error) {
            _failures.Diagnostics.Add(error.ToString());
            return Release(new ProcessingResult(operation, ProcessingStatus.Fatal, _failures.Diagnostics));
        }
    }

    // При неопределённом исходе освобождения закрывает дальнейшую запись этой сессии.
    private ProcessingResult Release(ProcessingResult result) {
        if(result.Status != ProcessingStatus.Pending) {
            try {
                transaction.Dispose();
            } catch(Exception error) {
                _failures.Diagnostics.Add(error.ToString());
                return new ProcessingResult(operation, ProcessingStatus.Fatal, _failures.Diagnostics);
            }
        }
        return result;
    }

    // Сохраняет обе причины, если после ошибки не удалось завершить и сам откат.
    private ProcessingResult Abort(ProcessingStatus status, string reason) {
        _rollbackRequested = true;
        _failures.Diagnostics.Add(reason);
        try {
            var current = transaction.GetStatus();
            if(current == TransactionStatus.Committed) {
                return End(current, ProcessingStatus.Fatal);
            }
            if(current == TransactionStatus.Started) {
                current = transaction.RollBack();
            }
            return End(current, status);
        } catch(Exception rollbackError) {
            _failures.Diagnostics.Add(rollbackError.ToString());
            return new ProcessingResult(operation, ProcessingStatus.Fatal, _failures.Diagnostics);
        }
    }

    // До терминального статуса ресурсы остаются у объекта операции. Pending останавливает весь запуск.
    private ProcessingResult End(TransactionStatus reported, ProcessingStatus rollbackStatus) {
        _rollbackStatus = rollbackStatus;
        var actual = transaction.GetStatus();
        ProcessingStatus status;
        if(reported == TransactionStatus.Pending || actual == TransactionStatus.Pending) {
            status = ProcessingStatus.Pending;
        } else if(_failures.IsFatal || rollbackStatus == ProcessingStatus.Fatal || actual != reported
            || (_rollbackRequested && actual == TransactionStatus.Committed)) {
            status = ProcessingStatus.Fatal;
        } else {
            status = actual switch {
                TransactionStatus.Committed => ProcessingStatus.Succeeded,
                TransactionStatus.RolledBack => rollbackStatus,
                _ => ProcessingStatus.Fatal
            };
        }
        if(status != ProcessingStatus.Succeeded) {
            _failures.Diagnostics.Add(localization.GetLocalizedString("Processing.TransactionStatus", reported, actual));
        }
        return new ProcessingResult(operation, status, _failures.Diagnostics);
    }
}
