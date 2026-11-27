using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

using Autodesk.Revit.DB;
using Autodesk.Revit.Exceptions;

using dosymep.Revit;
using dosymep.SimpleServices;

using RevitLintelsManager.Models;
using RevitLintelsManager.Models.Configs;

namespace RevitLintelsManager.Services;

/// <summary>Связывает утверждённые назначения с исходным документом и одной локальной транзакцией.</summary>
internal sealed class ManagerProcessingService(RevitRepository source, ManagerSessionState state,
    IProcessingConfirmation confirmation,
    ILocalizationService localization, ILoggerService logger, Func<Document, IGeometryReadContext> openGeometry,
    ILintelLockReader lockReader, ILintelInstallationService installation, ILintelCorrectionService correction,
    IParamService parameters)
    : IManagerProcessingService {
    private volatile ApprovedProcessingPlan _approved;
    private volatile bool _faulted;
    private ProcessingTransaction _pending;
    private bool _pendingBatch;
    private readonly ConditionalWeakTable<ProcessingPlan, ApprovedProcessingPlan> _receipts = new();
    private int _busy;

    /// <inheritdoc />
    public ProcessingResult ExecuteAssigned(ApprovedProcessingPlan approved, int index, CancellationToken token) {
        if(approved is null || index < 0 || index >= approved.Plan.Operations.Count) {
            throw new System.ArgumentOutOfRangeException(nameof(index));
        }
        return Execute(approved, index, GetAction(approved.Plan.Operations[index].Action), token);
    }

    // И пакет, и отдельная операция используют одни и те же изменения и проверки постусловий.
    private ProcessingAction GetAction(ManagerAction assigned) =>
        assigned switch {
            ManagerAction.Lock => new ProcessingAction(ManagerAction.Lock, ApplyLock, VerifyLock),
            ManagerAction.Install => new ProcessingAction(ManagerAction.Install, ApplyInstall, installation.Verify),
            ManagerAction.Replace => new ProcessingAction(ManagerAction.Replace, ApplyReplace, VerifyReplace),
            ManagerAction.Delete => new ProcessingAction(ManagerAction.Delete, ApplyDelete, VerifyDelete),
            ManagerAction.SetFixation or ManagerAction.ClearFixation => CreateFixationAction(assigned),
            ManagerAction.Correct => new ProcessingAction(ManagerAction.Correct,
                (target, ct) => correction.Apply(target, state.Config, ct), target => correction.Verify(target, state.Config)),
            _ => null
        };

    // Меняет признак фиксации у участников строки и проверяет их после регенерации.
    private ProcessingAction CreateFixationAction(ManagerAction action) {
        string key = nameof(LintelManagerConfig.LintelFixParam);
        string name = state.Config.Parameters[key];
        bool isFixed = action == ManagerAction.SetFixation;
        var value = new LintelParameterValue(key, name, LintelParameterKind.YesNo, null, isFixed);
        return new ProcessingAction(action, (target, token) => {
            var instances = GetActionInstances(target, action).ToArray();
            if(instances.Length == 0) {
                throw new System.InvalidOperationException(localization.GetLocalizedString("Processing.Parameter", name));
            }
            foreach(var instance in instances) {
                token.ThrowIfCancellationRequested();
                if(parameters.ReadFixation(instance, name) == isFixed) {
                    continue;
                }
                try {
                    if(parameters.WriteInstanceValue(instance, value)) {
                        continue;
                    }
                } catch(Exception error) when(instance.GroupId != ElementId.InvalidElementId && error is
                    Autodesk.Revit.Exceptions.InvalidOperationException or System.InvalidOperationException
                    or Autodesk.Revit.Exceptions.ArgumentException) {
                    throw new System.InvalidOperationException(Text("FixationGroup"), error);
                }
                throw new System.InvalidOperationException(instance.GroupId != ElementId.InvalidElementId
                    ? Text("FixationGroup") : localization.GetLocalizedString("Processing.Parameter", name));
            }
        }, target => {
            foreach(var instance in GetActionInstances(target, action)) {
                if(parameters.ReadFixation(instance, name) != isFixed) {
                    throw new System.InvalidOperationException(Text(instance.GroupId != ElementId.InvalidElementId
                        ? "FixationGroup" : "FixationFailed"));
                }
            }
        });
    }

    // При изменении фиксации обрабатывает существующие элементы строки с подтверждённым параметром.
    private static IEnumerable<FamilyInstance> GetActionInstances(ProcessingTarget target, ManagerAction action) {
        bool changesFixation = action is ManagerAction.SetFixation or ManagerAction.ClearFixation;
        if(target.Opening is { } opening
            && (!changesFixation || target.Match.Opening.Element.Information.IsFixed.HasValue)) {
            yield return opening;
        }
        if(target.Lintel is { } lintel
            && (!changesFixation || target.Match.Lintel.Element.Information.IsFixed.HasValue)) {
            yield return lintel;
        }
    }

    /// <inheritdoc />
    public ProcessingBatchResult ExecuteBatch(ApprovedProcessingPlan approved, CancellationToken token, IProgress<int> progress) {
        if(approved is null || Interlocked.CompareExchange(ref _busy, 1, 0) != 0) {
            return new ProcessingBatchResult(ProcessingStatus.Skipped, [], Text("RequestUnavailable"));
        }
        bool ownsPlan = ReferenceEquals(_approved, approved);
        try {
            if(_faulted || approved.HasStarted || approved.IsStopped || !ownsPlan) {
                return new ProcessingBatchResult(ProcessingStatus.Skipped, [], Text("RequestUnavailable"));
            }
            if(token.IsCancellationRequested || !CanExecute(approved)) {
                return new ProcessingBatchResult(ProcessingStatus.Canceled, [], Text("StaleOrCanceled"));
            }
            if(approved.Plan.Operations.Count == 0) {
                return new ProcessingBatchResult(ProcessingStatus.Skipped, []);
            }
            var batch = new ProcessingBatch(source.Document, localization, logger);
            var result = batch.Run(approved.Plan.Operations, (index, op) =>
                ExecuteOperation(approved, index, op, GetAction(op.Action), token), token, progress,
                rollback => _pending.AfterFinalization(rollback));
            _faulted |= result.Status is ProcessingStatus.Fatal or ProcessingStatus.Pending;
            _pendingBatch = result.Status == ProcessingStatus.Pending;
            foreach(var row in result.Operations) {
                Report(row);
            }
            return result;
        } catch(Exception error) {
            _faulted = true;
            logger.Warning(error, Text("Batch"));
            return new ProcessingBatchResult(ProcessingStatus.Fatal, [], error.Message);
        } finally {
            if(ownsPlan) {
                approved.IsStopped = true;
            }
            Volatile.Write(ref _busy, 0);
        }
    }

    // Установка и создание зависимости используют ту же открытую транзакцию строки.
    private void ApplyInstall(ProcessingTarget target, CancellationToken token) {
        installation.Install(target, state.Config, token);
    }

    // Общая часть удаления и замены проверяет весь нативный каскад в транзакции строки.
    private void ApplyDelete(ProcessingTarget target, CancellationToken token) {
        source.DeleteLintel(target.LockGeometry, target.Lintel, target.Opening, localization);
    }

    // Обе части замены выполняются до единственного Commit, включая отказ сразу после удаления.
    private void ApplyReplace(ProcessingTarget target, CancellationToken token) {
        ApplyDelete(target, token);
        token.ThrowIfCancellationRequested();
        installation.Install(target, state.Config, token);
    }

    // Проверяет отсутствие исходного экземпляра и сохранность проёма, его хоста и стены выреза.
    private void VerifyDelete(ProcessingTarget target) {
        var original = target.Match;
        bool openingPreserved = original.Opening is not { } opening || (target.Opening is { IsValidObject: true }
            && target.Opening.UniqueId == opening.Element.UniqueId && target.Opening.Host?.UniqueId == opening.HostWallUniqueId
            && opening.CutWallIds.All(id => target.Document.GetElement(id) is Wall));
        if(target.Lintel.IsValidObject || target.Document.GetElement(original.Lintel.Element.UniqueId) is not null || !openingPreserved) {
            throw new System.InvalidOperationException(Text("DeletePair"));
        }
    }

    // Дополнительно проверяет смену экземпляра и все постусловия установки.
    private void VerifyReplace(ProcessingTarget target) {
        VerifyDelete(target);
        if(target.InstalledLintel.Id.GetIdValue() == target.Match.Lintel.Element.Information.ElementId) {
            throw new System.InvalidOperationException(Text("ReplacePair"));
        }
        installation.Verify(target);
    }

    // Перепроверка назначения уже установила допустимость ручного Lock, в том числе при блокирующем подборе.
    private void ApplyLock(ProcessingTarget target, CancellationToken token) {
        target.OriginalLockParameters = RevitRepository.ReadLockParameters(target.Opening, target.Lintel);
        target.CreatedLockId = source.CreateCentralLock(target.LockGeometry, target.Opening, target.Lintel,
            target.Match, localization);
    }

    // Читает фактические ссылки после регенерации и сверяет единственный замок с созданной зависимостью.
    private void VerifyLock(ProcessingTarget target) {
        if(!lockReader.ConfirmsCreatedLock(target.LockGeometry, target.Match, target.CreatedLockId)) {
            throw new System.InvalidOperationException(Text("LockFailed"));
        }
        if(target.OriginalLockParameters is not null && !target.OriginalLockParameters.SequenceEqual(
                RevitRepository.ReadLockParameters(target.Opening, target.Lintel))) {
            throw new System.InvalidOperationException(Text("LockParametersChanged"));
        }
    }

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
            return Report(ExecuteOperation(approved, index, operation, action, token));
        } finally {
            Volatile.Write(ref _busy, 0);
        }
    }

    // Пока выполняется операция, интерфейс не может выдать разрешение на другой план.
    private ProcessingResult ExecuteOperation(ApprovedProcessingPlan approved, int index, ProcessingOperation operation,
        ProcessingAction action, CancellationToken token) {
        if(_faulted || _pending is not null || !ReferenceEquals(_approved, approved) || !approved.TryStart(index)) {
            return new ProcessingResult(operation, ProcessingStatus.Skipped, [Text("RequestUnavailable")]);
        }
        ProcessingResult result;
        try {
            if(!CanExecute(approved) || token.IsCancellationRequested) {
                result = new ProcessingResult(operation, ProcessingStatus.Canceled, [Text("StaleOrCanceled")]);
            } else if(operation.Action == ManagerAction.Skip || action?.Action != operation.Action) {
                result = new ProcessingResult(operation, ProcessingStatus.Skipped, [Text("ActionUnavailable")]);
            } else {
                var target = Revalidate(approved, operation);
                using var geometry = action.ChangesFixation ? null : openGeometry(source.Document);
                target.LockGeometry = geometry;
                if(geometry is not null) {
                    geometry.ProcessingData = state.AnalysisContext.Project.ProcessingData;
                }
                token.ThrowIfCancellationRequested();
                var tx = new ProcessingTransaction(operation, new Transaction(source.Document, Text("Transaction")), localization);
                result = tx.Run(target, action, token, () => CanExecute(approved));
                if(result.Status == ProcessingStatus.Succeeded
                    && operation.Action is ManagerAction.Install or ManagerAction.Replace or ManagerAction.Delete or ManagerAction.Correct) {
                    approved.MatchChanges.Record(geometry.ProcessingData, target.Match.Lintel,
                        target.InstalledPair?.Lintel ?? target.CorrectedLintel, state.Config.MatchLimits);
                }
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
        return result;
    }

    /// <inheritdoc />
    public ProcessingResult FinishPending() {
        if(_pending is null) {
            return null;
        }
        var result = _pending.FinishPending();
        if(result.Status != ProcessingStatus.Pending) {
            _pending = null;
            _faulted = _pendingBatch || result.Status == ProcessingStatus.Fatal;
            if(_pendingBatch) {
                result = new ProcessingResult(result.Operation, ProcessingStatus.Fatal,
                    result.Diagnostics.Concat([Text("BatchAborted")]));
                _pendingBatch = false;
            }
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

    // Берёт расчёт подтверждённого диапазона и проверяет только действующих владельцев строки.
    private ProcessingTarget Revalidate(ApprovedProcessingPlan approved, ProcessingOperation operation) {
        var document = source.Document;
        if(document.IsModifiable) {
            throw new System.InvalidOperationException(Text("TransactionAlreadyOpen"));
        }
        var context = state.AnalysisContext;
        if(context is null || context.Generation != approved.Plan.Generation
            || !context.RowsByKey.TryGetValue(operation.Key, out var current)) {
            throw new System.InvalidOperationException(Text("PairChanged"));
        }
        var match = current.Match;
        var opening = Resolve(match.Opening?.Element, match.Opening?.Geometry);
        var lintel = Resolve(match.Lintel?.Element, match.Lintel?.Geometry);
        if(opening?.Symbol.UniqueId != operation.OpeningTypeId || lintel?.Symbol.UniqueId != operation.LintelTypeId
            || (opening is not null && !OpeningIsCurrent(opening, match.Opening))
            || !approved.MatchChanges.PreservesPair(context.Project.ProcessingData, match, state.Config.MatchLimits)) {
            throw new System.InvalidOperationException(Text("PairChanged"));
        }
        LintelCalculation calculation = null;
        if(opening is not null) {
            context.Project.Calculations.TryGetValue(opening.UniqueId, out calculation);
        }
        if(operation.Action is ManagerAction.Install or ManagerAction.Replace && document.IsWorkshared) {
            using var worksets = document.GetWorksetTable();
            if(calculation?.Context?.ActiveWorksetId != worksets.GetActiveWorksetId().IntegerValue) {
                throw new System.InvalidOperationException(Text("PairChanged"));
            }
        }
        if(!current.Snapshot.AllowedActions.Contains(operation.Action)
            || (operation.Action is ManagerAction.Install or ManagerAction.Replace
                && (calculation?.Type is null || calculation.Type.UniqueId != operation.TargetTypeId))
            || (operation.Action == ManagerAction.Correct && operation.Corrections != current.Snapshot.Corrections)) {
            var issues = current.Snapshot.Diagnostics.Concat(current.Opening?.Diagnostics ?? [])
                .Concat(current.Lintel?.Diagnostics ?? []).Select(issue =>
                    localization.GetLocalizedString(issue.LocalizationKey, issue.Arguments.Cast<object>().ToArray()));
            throw new System.InvalidOperationException(string.Join(" ", new[] { Text("ActionUnavailable") }.Concat(issues)));
        }
        var target = new ProcessingTarget(document, opening, lintel, match, calculation) {
            Comparison = current.Comparison
        };
        foreach(var instance in GetActionInstances(target, operation.Action)) {
            EnsureWritable(instance);
        }
        return target;
    }

    // Проверяет хост, стену выреза, уровень и размеры проёма без повторного измерения кладки.
    private bool OpeningIsCurrent(FamilyInstance opening, ManagerOpeningSnapshot snapshot) {
        var document = source.Document;
        var names = state.Config.Parameters;
        return opening.Host?.UniqueId == snapshot.HostWallUniqueId
            && snapshot.CutWallIds.All(id => document.GetElement(id) is Wall)
            && document.GetElement(opening.LevelId)?.UniqueId == snapshot.LevelUniqueId
            && parameters.ReadLengthMm(opening, opening.Symbol, names[nameof(OpeningFamilyConfig.LintelOpeningWidth)])
                == snapshot.WidthMm
            && parameters.ReadLengthMm(opening, opening.Symbol, names[nameof(OpeningFamilyConfig.LintelOpeningHeight)])
                == snapshot.HeightMm;
    }

    // Разрешает идентификатор только в исходном документе.
    private FamilyInstance Resolve(ManagerFamilySnapshot snapshot, GeometryFacts geometry) {
        if(snapshot is null) {
            return null;
        }
        var document = source.Document;
        if(document.GetElement(snapshot.UniqueId) is not FamilyInstance { IsValidObject: true } instance) {
            throw new System.InvalidOperationException(localization.GetLocalizedString("Processing.Missing", snapshot.UniqueId));
        }
        var info = snapshot.Information;
        if(!LintelLockReader.MatchesInstance(instance, snapshot, geometry)
            || parameters.ReadFixation(instance, state.Config.Parameters[nameof(LintelManagerConfig.LintelFixParam)]) != info.IsFixed
            || (instance.GroupId != ElementId.InvalidElementId) != info.IsInGroup
            || instance.CreatedPhaseId.GetIdValue() != info.Phases.CreatedId
            || instance.DemolishedPhaseId.GetIdValue() != info.Phases.DemolishedId) {
            throw new System.InvalidOperationException(Text("PairChanged"));
        }
        return instance;
    }

    // Проверяет запись после предметных ограничений, чтобы группа получила собственную причину отказа.
    private void EnsureWritable(FamilyInstance instance) {
        if(instance is null) {
            return;
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
