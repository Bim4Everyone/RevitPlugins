using System;

using pyRevitLabs.Json;

namespace RevitServerFolders.Models;

internal class ExcludedObjectPattern : IEquatable<ExcludedObjectPattern> {
    public ExcludedObjectPattern(string value) {
        if(string.IsNullOrWhiteSpace(value)) {
            throw new ArgumentException(nameof(value));
        }

        Id = Guid.NewGuid();
        Value = value;
    }

    /// <summary>
    /// Идентификатор подстроки.
    /// </summary>
    [JsonProperty(nameof(Id))]
    public Guid Id { get; }

    /// <summary>
    /// Подстрока для скрытия файлов из списка моделей набора.
    /// </summary>
    [JsonProperty(nameof(Value))]
    public string Value { get; }

    public bool Equals(ExcludedObjectPattern other) {
        if(other is null) {
            return false;
        }

        if(ReferenceEquals(this, other)) {
            return true;
        }

        return Id.Equals(other.Id);
    }

    public override bool Equals(object obj) {
        return Equals(obj as ExcludedObjectPattern);
    }

    public override int GetHashCode() {
        return Id.GetHashCode();
    }
}
