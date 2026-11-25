using System.Collections.Generic;

namespace RevitLintelsManager.Models;

/// <summary>Идентичность ссылки в пределах текущего чтения документа.</summary>
internal sealed class LintelLockReference(long? ownerId, string representation) {
    public long? OwnerId { get; } = ownerId;
    public string Representation { get; } = representation;
}

/// <summary>Поддерживаемый тип зависимости или техническое ограничение её чтения.</summary>
internal enum LintelLockDependencyKind { Direct, Other, Chain, Unsupported }

/// <summary>Только значения зависимости, необходимые для проверки конкретных ссылок.</summary>
internal sealed class LintelLockDependency(string id, LintelLockDependencyKind kind, bool? isLocked,
    bool referencesAvailable, IReadOnlyList<LintelLockReference> references) {
    public string Id { get; } = id;
    public LintelLockDependencyKind Kind { get; } = kind;
    public bool? IsLocked { get; } = isLocked;
    public bool ReferencesAvailable { get; } = referencesAvailable;
    public IReadOnlyList<LintelLockReference> References { get; } = references;
}
