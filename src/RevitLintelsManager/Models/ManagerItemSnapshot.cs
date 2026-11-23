using System;
using System.Collections.Generic;
using System.Linq;

namespace RevitLintelsManager.Models;

/// <summary>Шесть предметных действий; смешанное состояние группы не является действием.</summary>
internal enum ManagerAction { Install, Replace, Correct, Lock, Delete, Skip }

/// <summary>Основной статус элемента; причины представлены отдельными диагностиками.</summary>
internal enum ManagerElementStatus { Missing, Correct, Warning, Fixed, Grouped }

/// <summary>Составной ключ строки в исходном документе.</summary>
internal readonly struct ManagerItemKey(Guid documentToken, string openingUniqueId, string lintelUniqueId)
    : IEquatable<ManagerItemKey> {
    public Guid DocumentToken { get; } = documentToken;
    public string OpeningUniqueId { get; } = openingUniqueId;
    public string LintelUniqueId { get; } = lintelUniqueId;

    /// <inheritdoc />
    public bool Equals(ManagerItemKey other) {
        return DocumentToken == other.DocumentToken && OpeningUniqueId == other.OpeningUniqueId
            && LintelUniqueId == other.LintelUniqueId;
    }

    /// <inheritdoc />
    public override bool Equals(object obj) {
        return obj is ManagerItemKey other && Equals(other);
    }

    /// <inheritdoc />
    public override int GetHashCode() {
        return (DocumentToken, OpeningUniqueId, LintelUniqueId).GetHashCode();
    }
}

/// <summary>Ключ группировки по типам, статусам и фиксации перемычки.</summary>
internal readonly struct ManagerGroupKey(string openingTypeUniqueId, string lintelTypeUniqueId,
    ManagerElementStatus openingStatus, ManagerElementStatus lintelStatus, bool isLintelFixed) {
    public string OpeningTypeUniqueId { get; } = openingTypeUniqueId;
    public string LintelTypeUniqueId { get; } = lintelTypeUniqueId;
    public ManagerElementStatus OpeningStatus { get; } = openingStatus;
    public ManagerElementStatus LintelStatus { get; } = lintelStatus;
    public bool IsLintelFixed { get; } = isLintelFixed;
}

/// <summary>Локализуемая диагностическая причина без объектов API.</summary>
internal sealed class ManagerDiagnostic(string localizationKey, IEnumerable<string> arguments) {
    public string LocalizationKey { get; } = localizationKey;
    public IReadOnlyList<string> Arguments { get; } = arguments.ToList().AsReadOnly();
}

/// <summary>Контракт независимой строки; источник реальных строк появится в следующих задачах.</summary>
internal sealed class ManagerItemSnapshot(ManagerItemKey key, ManagerGroupKey groupKey,
    IEnumerable<ManagerDiagnostic> diagnostics, IEnumerable<ManagerAction> allowedActions, ManagerAction defaultAction) {
    public ManagerItemKey Key { get; } = key;
    public ManagerGroupKey GroupKey { get; } = groupKey;
    public IReadOnlyList<ManagerDiagnostic> Diagnostics { get; } = diagnostics.ToList().AsReadOnly();
    public IReadOnlyList<ManagerAction> AllowedActions { get; } = allowedActions.ToList().AsReadOnly();
    public ManagerAction DefaultAction { get; } = defaultAction;
}
