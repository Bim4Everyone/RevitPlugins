using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;

using RevitLintelsManager.Models;

namespace RevitLintelsManager.Services;

/// <summary>Владеет одной группой и удерживает результаты дочерних транзакций до её завершения.</summary>
internal sealed class ProcessingBatch(Document document, ILocalizationService localization, ILoggerService logger) {
    /// <summary>Вызывает обработчик каждого назначения в текущем API-контексте.</summary>
    public ProcessingBatchResult Run(IReadOnlyList<ProcessingOperation> operations,
        Func<int, ProcessingOperation, ProcessingResult> execute, CancellationToken token, IProgress<int> progress,
        Action<Action> afterPending) {
        var rows = new List<ProcessingResult>(operations.Count);
        var group = new TransactionGroup(document, localization.GetLocalizedString("Processing.Batch"));
        bool transferred = false;
        try {
            if(group.Start() != TransactionStatus.Started) {
                throw new InvalidOperationException(StatusMessage(group.GetStatus()));
            }
            for(int index = 0; index < operations.Count && !token.IsCancellationRequested; index++) {
                var result = execute(index, operations[index]);
                rows.Add(result);
                if(result.Status == ProcessingStatus.Pending) {
                    transferred = true;
                    afterPending(() => RollBackPending(group));
                    return Discard(rows, ProcessingStatus.Pending, StatusMessage(TransactionStatus.Pending));
                }
                if(result.Status == ProcessingStatus.Fatal) {
                    throw new InvalidOperationException(string.Join(Environment.NewLine, result.Diagnostics));
                }
                progress?.Report(rows.Count);
                if(result.MustStop) {
                    break;
                }
            }
            bool changed = rows.Any(row => row.Status == ProcessingStatus.Succeeded);
            var expected = changed ? TransactionStatus.Committed : TransactionStatus.RolledBack;
            var reported = changed ? group.Assimilate() : group.RollBack();
            if(reported != expected || group.GetStatus() != expected) {
                throw new InvalidOperationException(StatusMessage(reported));
            }
            var status = rows.Count < operations.Count || rows.Any(row => row.Status == ProcessingStatus.Canceled)
                ? ProcessingStatus.Canceled : rows.Any(row => row.Status == ProcessingStatus.Failed)
                    ? ProcessingStatus.Failed : changed ? ProcessingStatus.Succeeded : ProcessingStatus.Skipped;
            return new ProcessingBatchResult(status, rows);
        } catch(Exception error) {
            logger.Warning(error, localization.GetLocalizedString("Processing.Batch"));
            string diagnostic = error.Message;
            try {
                RollBack(group);
            } catch(Exception rollbackError) {
                logger.Warning(rollbackError, localization.GetLocalizedString("Processing.Batch"));
                diagnostic += Environment.NewLine + rollbackError.Message;
            }
            return Discard(rows, ProcessingStatus.Fatal, diagnostic);
        } finally {
            if(!transferred) {
                group.Dispose();
            }
        }
    }

    // Не публикует успешные ID, когда группа откатилась или её конечный исход неизвестен.
    private static ProcessingBatchResult Discard(IEnumerable<ProcessingResult> rows, ProcessingStatus status,
        string diagnostic) {
        return new ProcessingBatchResult(status, rows.Select(row => row.Status == ProcessingStatus.Succeeded
            ? new ProcessingResult(row.Operation, status, row.Diagnostics.Concat([diagnostic]), userMessage: diagnostic)
            : row), diagnostic);
    }

    // В режиме failures группа принадлежит finalizer. Этот путь не начинает следующий API-запрос.
    private void RollBackPending(TransactionGroup group) {
        try {
            RollBack(group);
        } catch(Exception error) {
            logger.Warning(error, localization.GetLocalizedString("Processing.Batch"));
        } finally {
            try {
                group.Dispose();
            } catch(Exception error) {
                logger.Warning(error, localization.GetLocalizedString("Processing.Batch"));
            }
        }
    }

    // Сверяет фактический статус группы после аварийного отката.
    private void RollBack(TransactionGroup group) {
        if(group.GetStatus() == TransactionStatus.Started
            && (group.RollBack() != TransactionStatus.RolledBack || group.GetStatus() != TransactionStatus.RolledBack)) {
            throw new InvalidOperationException(StatusMessage(group.GetStatus()));
        }
    }

    // Групповой исход отличим от статуса дочерней транзакции в диагностике.
    private string StatusMessage(TransactionStatus status) =>
        localization.GetLocalizedString("Processing.BatchStatus", status);
}
