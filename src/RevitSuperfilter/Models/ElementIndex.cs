using System;

using Autodesk.Revit.DB;

using RevitSuperfilter.Comparators;

namespace RevitSuperfilter.Models;

internal sealed class ElementIndex : IEquatable<ElementIndex> {
    public ElementIndex(Element element) {
        Id = element.Id;
        TypeId = element.GetTypeId();
        Category = element.Category?.Name ?? "<null>";
        
        Element = element;
    }

    public ElementId Id { get; }
    public ElementId TypeId { get; }
    public string Category { get; }
    public Element Element { get; }

    #region IEquatable<ElementIndex>

    public bool Equals(ElementIndex other) {
        if(other is null) {
            return false;
        }

        if(ReferenceEquals(this, other)) {
            return true;
        }

        return Equals(Id, other.Id) && Equals(TypeId, other.TypeId) && Category == other.Category;
    }

    public override bool Equals(object obj) {
        return ReferenceEquals(this, obj) || obj is ElementIndex other && Equals(other);
    }

    public override int GetHashCode() {
        unchecked {
            int hashCode = (Id != null ? Id.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (TypeId != null ? TypeId.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (Category != null ? Category.GetHashCode() : 0);
            return hashCode;
        }
    }

    public static bool operator==(ElementIndex left, ElementIndex right) {
        return Equals(left, right);
    }

    public static bool operator!=(ElementIndex left, ElementIndex right) {
        return !Equals(left, right);
    }

    #endregion
}
