using System;
using System.Collections.Generic;
using System.Linq;

namespace RevitLintelsManager.Models;

/// <summary>Степень подтверждения геометрического замка для конкретной пары.</summary>
internal enum LintelLockState {
    Confirmed, Absent, UnsuitableDependency, ReferencesUnavailable, Unreadable, Ambiguous, Stale
}

/// <summary>Причина результата чтения, которую может использовать последующий анализ.</summary>
internal enum LintelLockReason {
    None, SourceDocumentUnavailable, PairChanged, OpeningReferenceUnavailable, LintelReferenceUnavailable,
    MultipleCentralReferences, DependencyUnreadable, UnsupportedDependency, IndirectDependency
}

/// <summary>Результат одного чтения. Идентификаторы зависимостей относятся к исходному документу.</summary>
internal sealed class LintelLockFacts(LintelLockState state, LintelLockReason reason = LintelLockReason.None,
    IEnumerable<string> dependencyIds = null) {
    public LintelLockState State { get; } = state;
    public LintelLockReason Reason { get; } = reason;
    public IReadOnlyList<string> DependencyIds { get; } = dependencyIds is null
        ? Array.Empty<string>() : dependencyIds.Where(id => id is not null).Distinct()
            .OrderBy(id => id, StringComparer.Ordinal).ToList().AsReadOnly();

    public bool? IsLocked => State switch {
        LintelLockState.Confirmed => true,
        LintelLockState.Absent or LintelLockState.UnsuitableDependency => false,
        _ => null
    };
}
