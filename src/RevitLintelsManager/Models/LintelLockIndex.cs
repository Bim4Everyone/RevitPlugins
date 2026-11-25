using System.Collections.Generic;
using System.Linq;

namespace RevitLintelsManager.Models;

/// <summary>Индекс ограничений одного чтения без зависимости от Revit API.</summary>
internal sealed class LintelLockIndex(IReadOnlyCollection<LintelLockDependency> dependencies) {
    private readonly ILookup<long, LintelLockDependency> _byOwner = dependencies
        .Where(d => d.References is not null)
        .SelectMany(d => d.References.Where(r => r.OwnerId.HasValue)
            .Select(r => r.OwnerId.Value).Distinct().Select(id => (id, d)))
        .ToLookup(item => item.id, item => item.d);

    private readonly LintelLockDependency[] _unattributed = dependencies.Where(d => d.IsLocked != false
        && d.Kind != LintelLockDependencyKind.Other
        && (d.References is null || d.References.Any(r => !r.OwnerId.HasValue))).ToArray();

    /// <summary>Проверяет зависимости двух центральных ссылок с учётом неполных результатов чтения.</summary>
    public LintelLockFacts Assess(LintelLockReference opening, LintelLockReference lintel) {
        var candidates = _byOwner[opening.OwnerId.Value].Concat(_byOwner[lintel.OwnerId.Value])
            .Concat(_unattributed).Distinct().ToList();
        var results = candidates.Select(candidate => (candidate.Id, Facts: AssessDependency(candidate, opening, lintel)))
            .Where(item => item.Facts.HasValue).ToList();
        foreach(var state in new[] {
            LintelLockState.Confirmed, LintelLockState.Unreadable, LintelLockState.Ambiguous,
            LintelLockState.UnsuitableDependency
        }) {
            var matches = results.Where(item => item.Facts.Value.State == state).ToList();
            if(matches.Count > 0) {
                var reason = state == LintelLockState.Unreadable
                    && matches.Any(item => item.Facts.Value.Reason == LintelLockReason.UnsupportedDependency)
                    ? LintelLockReason.UnsupportedDependency : matches[0].Facts.Value.Reason;
                return new LintelLockFacts(state, reason, matches.Select(item => item.Id));
            }
        }
        return new LintelLockFacts(LintelLockState.Absent);
    }

    // Сначала исключает другие элементы и ссылки. При пробеле проверяет, мог ли он изменить итог для пары.
    private static (LintelLockState State, LintelLockReason Reason)? AssessDependency(LintelLockDependency dependency,
        LintelLockReference opening, LintelLockReference lintel) {
        var references = dependency.References;
        if(references is null) {
            return (LintelLockState.Unreadable, dependency.Kind == LintelLockDependencyKind.Unsupported
                ? LintelLockReason.UnsupportedDependency : LintelLockReason.DependencyUnreadable);
        }
        var openingRefs = references.Where(r => r.OwnerId == opening.OwnerId).ToArray();
        var lintelRefs = references.Where(r => r.OwnerId == lintel.OwnerId).ToArray();
        bool related = openingRefs.Length > 0 || lintelRefs.Length > 0;
        int unknownOwners = references.Count(r => !r.OwnerId.HasValue);
        bool couldBind = MissingOwnerSlots(openingRefs, opening) + MissingOwnerSlots(lintelRefs, lintel)
            <= unknownOwners;
        if(!couldBind || dependency.IsLocked == false || dependency.Kind == LintelLockDependencyKind.Other) {
            return related ? (LintelLockState.UnsuitableDependency, LintelLockReason.None) : null;
        }
        if(dependency.Kind == LintelLockDependencyKind.Unsupported) {
            return (LintelLockState.Unreadable, LintelLockReason.UnsupportedDependency);
        }
        if(dependency.IsLocked is null || !dependency.ReferencesAvailable
            || !openingRefs.Any(r => r.Representation == opening.Representation)
            || !lintelRefs.Any(r => r.Representation == lintel.Representation)) {
            return (LintelLockState.Unreadable, LintelLockReason.DependencyUnreadable);
        }
        return dependency.Kind == LintelLockDependencyKind.Direct
            ? (LintelLockState.Confirmed, LintelLockReason.None)
            : (LintelLockState.Ambiguous, LintelLockReason.IndirectDependency);
    }

    // Отсутствие доказательства несовпадения требует проверить неразрешённые ссылки.
    private static int MissingOwnerSlots(LintelLockReference[] refs, LintelLockReference expected) {
        return refs.Any(r => r.Representation is null || r.Representation == expected.Representation) ? 0 : 1;
    }
}
