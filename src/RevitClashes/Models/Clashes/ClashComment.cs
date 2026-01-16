using System;

using pyRevitLabs.Json;

namespace RevitClashDetective.Models.Clashes;

internal class ClashComment : IEquatable<ClashComment> {
    [JsonConstructor]
    public ClashComment() {
    }

    public int Id { get; set; }

    public string Body { get; set; } = string.Empty;

    public string Author { get; set; } = string.Empty;

    public DateTime CreatedDate { get; set; } = DateTime.Now;

    public bool Equals(ClashComment other) {
        if(other is null) {
            return false;
        }

        if(ReferenceEquals(this, other)) {
            return true;
        }

        return Id == other.Id;
    }

    public override bool Equals(object obj) {
        if(obj is null) {
            return false;
        }

        if(ReferenceEquals(this, obj)) {
            return true;
        }

        if(obj.GetType() != GetType()) {
            return false;
        }

        return Equals((ClashComment) obj);
    }

    public override int GetHashCode() {
        return Id;
    }
}
