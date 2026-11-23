using System;
using System.Collections.Generic;
using System.Linq;

namespace RevitLintelsManager.Models;

/// <summary>Неизменяемый запрос снимка выбранного диапазона исходного документа.</summary>
internal sealed class ManagerRequest(Guid documentToken, long generation, ManagerScope scope = ManagerScope.Project,
    bool useInitialContext = false, IEnumerable<ManagerItemKey> deletionCandidates = null) {
    public Guid DocumentToken { get; } = documentToken;
    public long Generation { get; } = generation;
    public ManagerScope Scope { get; } = scope;
    public bool UseInitialContext { get; } = useInitialContext;
    public IReadOnlyList<ManagerItemKey> DeletionCandidates { get; } = deletionCandidates is null
        ? Array.Empty<ManagerItemKey>() : deletionCandidates.ToList().AsReadOnly();
}
