using System;
using System.Collections.Generic;
using System.Linq;

namespace RevitLintelsManager.Models;

/// <summary>Назначение одной строки до подтверждения пользователем.</summary>
internal readonly struct ProcessingOperation(ManagerItemKey key, ManagerAction action, string openingTypeId,
    string lintelTypeId, string targetTypeId, ManagerCorrection corrections) {
    public ManagerItemKey Key { get; } = key;
    public ManagerAction Action { get; } = action;
    public string OpeningTypeId { get; } = openingTypeId;
    public string LintelTypeId { get; } = lintelTypeId;
    public string TargetTypeId { get; } = targetTypeId;
    public ManagerCorrection Corrections { get; } = corrections;
}

/// <summary>Неизменяемый состав операций одного анализа без ссылок на API и ViewModel.</summary>
internal sealed class ProcessingPlan {
    private ProcessingPlan(Guid documentToken, long generation, List<ProcessingOperation> operations) {
        DocumentToken = documentToken;
        Generation = generation;
        Operations = operations.AsReadOnly();
    }

    public Guid DocumentToken { get; }
    public long Generation { get; }
    public IReadOnlyList<ProcessingOperation> Operations { get; }

    /// <summary>Убирает одинаковые назначения и отклоняет противоречивые назначения одного элемента.</summary>
    public static ProcessingPlan Create(Guid documentToken, long generation, IEnumerable<ProcessingOperation> rows) {
        var ids = new HashSet<string>(StringComparer.Ordinal);
        var operations = new List<ProcessingOperation>();
        foreach(var operation in rows.Where(row => row.Action != ManagerAction.Skip).Distinct()) {
            var key = operation.Key;
            if(key.DocumentToken != documentToken || (key.OpeningUniqueId is null && key.LintelUniqueId is null)
                || (key.OpeningUniqueId is not null && !ids.Add(key.OpeningUniqueId))
                || (key.LintelUniqueId is not null && !ids.Add(key.LintelUniqueId))) {
                return null;
            }
            operations.Add(operation);
        }
        return new ProcessingPlan(documentToken, generation, operations);
    }
}

/// <summary>Однократное исполнение каждого назначения подтверждённого плана в API-контексте.</summary>
internal sealed class ApprovedProcessingPlan(ProcessingPlan plan) {
    private readonly HashSet<int> _started = [];
    internal BatchMatchChanges MatchChanges { get; } = new();
    public ProcessingPlan Plan { get; } = plan;
    public long Generation { get; internal set; } = plan.Generation;
    public bool IsStopped { get; internal set; }
    internal bool HasStarted => _started.Count != 0;

    /// <summary>Резервирует конкретное назначение, в том числе при повторной доставке запроса.</summary>
    internal bool TryStart(int index) => !IsStopped && _started.Add(index);
}
