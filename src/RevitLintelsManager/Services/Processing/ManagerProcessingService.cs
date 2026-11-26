using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

using Autodesk.Revit.DB;
using Autodesk.Revit.Exceptions;

using dosymep.SimpleServices;

using RevitLintelsManager.Models;

namespace RevitLintelsManager.Services;

/// <summary>Связывает утверждённые назначения с исходным документом и одной локальной транзакцией.</summary>
internal sealed class ManagerProcessingService(RevitRepository source, ManagerSessionState state,
    IProjectSnapshotReader snapshots, IManagerAnalysisService analysis, IProcessingConfirmation confirmation,
    ILocalizationService localization, ILoggerService logger) : IManagerProcessingService {
    private volatile ApprovedProcessingPlan _approved;
    private volatile bool _faulted;
    private ProcessingTransaction _pending;
    private readonly ConditionalWeakTable<ProcessingPlan, ApprovedProcessingPlan> _receipts = new();
    private int _busy;

    /// <inheritdoc />
    public ApprovedProcessingPlan Confirm(ProcessingPlan plan) {
        if(plan is null || !IsCurrent(plan.DocumentToken, plan.Generation) || _faulted
            || Interlocked.CompareExchange(ref _busy, 1, 0) != 0) {
            return null;
        }
        try {
            if(_receipts.TryGetValue(plan, out var previous)) {
                return ReferenceEquals(previous, _approved) && !previous.IsStopped ? previous : null;
            }
            _approved = null;
            int deletes = plan.Operations.Count(item => item.Action == ManagerAction.Delete);
            int replacements = plan.Operations.Count(item => item.Action == ManagerAction.Replace);
            if((deletes + replacements > 0 && !confirmation.Show(deletes, replacements))
                || !IsCurrent(plan.DocumentToken, plan.Generation) || _faulted) {
                return null;
            }
            var approved = new ApprovedProcessingPlan(plan);
            _receipts.Add(plan, approved);
            return _approved = approved;
        } finally {
            Volatile.Write(ref _busy, 0);
        }
    }

    /// <inheritdoc />
    public ProcessingResult Execute(ApprovedProcessingPlan approved, int index, ProcessingAction action,
        CancellationToken token) {
        if(approved is null || index < 0 || index >= approved.Plan.Operations.Count) {
            throw new System.ArgumentOutOfRangeException(nameof(index));
        }
        var operation = approved.Plan.Operations[index];
        if(Interlocked.CompareExchange(ref _busy, 1, 0) != 0) {
            return Report(new ProcessingResult(operation, ProcessingStatus.Skipped, [Text("RequestUnavailable")]));
        }
        try {
            return ExecuteOperation(approved, index, operation, action, token);
        } finally {
            Volatile.Write(ref _busy, 0);
        }
    }

    // Пока выполняется операция, интерфейс не может выдать разрешение на другой план.
    private ProcessingResult ExecuteOperation(ApprovedProcessingPlan approved, int index, ProcessingOperation operation,
        ProcessingAction action, CancellationToken token) {
        if(_faulted || _pending is not null || !ReferenceEquals(_approved, approved) || !approved.TryStart(index)) {
            return Report(new ProcessingResult(operation, ProcessingStatus.Skipped, [Text("RequestUnavailable")]));
        }
        ProcessingResult result;
        try {
            if(!CanExecute(approved) || token.IsCancellationRequested) {
                result = new ProcessingResult(operation, ProcessingStatus.Canceled, [Text("StaleOrCanceled")]);
            } else if(operation.Action == ManagerAction.Skip || action?.Action != operation.Action) {
                result = new ProcessingResult(operation, ProcessingStatus.Skipped, [Text("ActionUnavailable")]);
            } else {
                var target = Revalidate(operation);
                token.ThrowIfCancellationRequested();
                var tx = new ProcessingTransaction(operation, new Transaction(source.Document, Text("Transaction")), localization);
                result = tx.Run(target, action, token, () => CanExecute(approved));
                if(result.Status == ProcessingStatus.Pending) {
                    _pending = tx;
                }
                // DocumentChanged собственной завершённой транзакции может продвинуть поколение в этом callback.
                if(result.Status is ProcessingStatus.Succeeded or ProcessingStatus.Failed) {
                    approved.Generation = state.Generation;
                }
            }
        } catch(Exception error) {
            var status = error switch {
                RegenerationFailedException or InternalException or ModificationForbiddenException => ProcessingStatus.Fatal,
                System.OperationCanceledException or Autodesk.Revit.Exceptions.OperationCanceledException => ProcessingStatus.Canceled,
                _ => ProcessingStatus.Failed
            };
            result = new ProcessingResult(operation, status, [error.Message]);
            logger.Warning(error, Text("ValidationFailed"));
        }
        approved.IsStopped |= result.MustStop;
        _faulted |= result.Status is ProcessingStatus.Fatal or ProcessingStatus.Pending;
        return Report(result);
    }

    /// <inheritdoc />
    public ProcessingResult FinishPending() {
        if(_pending is null) {
            return null;
        }
        var result = _pending.FinishPending();
        if(result.Status != ProcessingStatus.Pending) {
            _pending = null;
            _faulted = result.Status == ProcessingStatus.Fatal;
        }
        return Report(result);
    }

    // Сверяет поколение и действующий исходный документ до получения любых элементов для записи.
    private bool CanExecute(ApprovedProcessingPlan approved) =>
        ReferenceEquals(_approved, approved) && IsCurrent(approved.Plan.DocumentToken, approved.Generation)
        && source.IsSourceDocumentActive(source.UIApplication) && !source.Document.IsReadOnly
        && !source.Document.IsReadOnlyFile && !source.Document.IsFamilyDocument;

    // Проверяет состояние сессии, не обращаясь к документу из интерфейса.
    private bool IsCurrent(Guid token, long generation) =>
        token == state.DocumentToken && generation == state.Generation && !state.IsClosing && !state.IsBlocked;

    // Читает пару по всему проекту с ограничением выдачи по ID. EditFamily выполняется до Transaction.Start.
    private ProcessingTarget Revalidate(ProcessingOperation operation) {
        var document = source.Document;
        if(document.IsModifiable) {
            throw new System.InvalidOperationException(Text("TransactionAlreadyOpen"));
        }
        var opening = Resolve(operation.Key.OpeningUniqueId);
        var lintel = Resolve(operation.Key.LintelUniqueId);
        var ids = new List<ElementId>();
        if(opening is not null) {
            ids.Add(opening.Id);
        }
        if(lintel is not null) {
            ids.Add(lintel.Id);
        }
        using var collector = new FilteredElementCollector(document, ids);
        var project = snapshots.ReadScopeSnapshot(state.Config, source.GetReadPhaseId(state.Config),
            ManagerScope.Selected, collector);
        var match = project.GetRows().SingleOrDefault(row => row.Opening?.Element.UniqueId == operation.Key.OpeningUniqueId
            && row.Lintel?.Element.UniqueId == operation.Key.LintelUniqueId);
        if(match is null || match.Opening?.Element.TypeUniqueId != operation.OpeningTypeId
            || match.Lintel?.Element.TypeUniqueId != operation.LintelTypeId) {
            throw new System.InvalidOperationException(Text("PairChanged"));
        }
        LintelCalculation calculation = null;
        if(opening is not null) {
            project.Calculations.TryGetValue(opening.UniqueId, out calculation);
        }
        var current = analysis.AnalyzeForProcessing(state.DocumentToken, match, calculation, state.Config, out var comparison);
        if(!current.Snapshot.AllowedActions.Contains(operation.Action)
            || (operation.Action is ManagerAction.Install or ManagerAction.Replace
                && (calculation?.Type is null || calculation.Type.UniqueId != operation.TargetTypeId))
            || (operation.Action == ManagerAction.Correct && operation.Corrections != current.Snapshot.Corrections)) {
            var issues = current.Snapshot.Diagnostics.Concat(current.Opening?.Diagnostics ?? [])
                .Concat(current.Lintel?.Diagnostics ?? []).Select(issue =>
                    localization.GetLocalizedString(issue.LocalizationKey, issue.Arguments.Cast<object>().ToArray()));
            throw new System.InvalidOperationException(string.Join(" ", new[] { Text("ActionUnavailable") }.Concat(issues)));
        }
        EnsureWritable(opening);
        EnsureWritable(lintel);
        if(operation.Action == ManagerAction.Correct) {
            foreach(string name in comparison.Differences.Where(diff => diff.Correction != ManagerCorrection.None
                    && diff.Parameter is not null).Select(diff => diff.Parameter).Distinct()) {
                var parameters = lintel.GetParameters(name);
                if(parameters.Count != 1 || parameters[0].IsReadOnly) {
                    throw new System.InvalidOperationException(localization.GetLocalizedString("Processing.Parameter", name));
                }
            }
        }
        return new ProcessingTarget(document, opening, lintel, match, calculation);
    }

    // Разрешает идентификатор только в исходном документе.
    private FamilyInstance Resolve(string uniqueId) {
        if(uniqueId is null) {
            return null;
        }
        var document = source.Document;
        if(document.GetElement(uniqueId) is not FamilyInstance { IsValidObject: true } instance) {
            throw new System.InvalidOperationException(localization.GetLocalizedString("Processing.Missing", uniqueId));
        }
        return instance;
    }

    // Проверяет запись после предметных ограничений, чтобы группа получила собственную причину отказа.
    private void EnsureWritable(FamilyInstance instance) {
        if(instance is null) {
            return;
        }
        if(instance.Pinned) {
            throw new System.InvalidOperationException(localization.GetLocalizedString("Processing.Pinned", instance.UniqueId));
        }
        var document = source.Document;
        if(document.IsWorkshared && (WorksharingUtils.GetCheckoutStatus(document, instance.Id) == CheckoutStatus.OwnedByOtherUser
            || WorksharingUtils.GetModelUpdatesStatus(document, instance.Id) != ModelUpdatesStatus.CurrentWithCentral)) {
            throw new System.InvalidOperationException(localization.GetLocalizedString("Processing.Ownership", instance.UniqueId));
        }
    }

    // Возвращает сообщение на языке текущей сессии.
    private string Text(string key) => localization.GetLocalizedString("Processing." + key);

    // Журнал содержит идентичность строки и все собранные причины, включая предупреждения при Commit.
    private ProcessingResult Report(ProcessingResult result) {
        if(result.Diagnostics.Count > 0) {
            logger.Warning(localization.GetLocalizedString("Processing.OperationLog", result.Operation.Action,
                result.Operation.Key.OpeningUniqueId, result.Operation.Key.LintelUniqueId, result.Status,
                string.Join(" | ", result.Diagnostics)));
        }
        return result;
    }
}
