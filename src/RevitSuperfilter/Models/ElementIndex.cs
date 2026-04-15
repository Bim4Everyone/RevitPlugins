using System;

using Autodesk.Revit.DB;

using RevitSuperfilter.Comparators;

namespace RevitSuperfilter.Models;

internal sealed class ElementIndex : IEquatable<ElementIndex> {
    public ElementIndex(Category category, Definition definition, string paramValue) {
        Category = category;
        Definition = definition;
        ParamValue = paramValue;
    }

    public Category Category { get; }
    public Definition Definition { get; }
    public string ParamValue { get; }

    #region IEquatable<ElementIndex>

    public bool Equals(ElementIndex other) {
        if(other is null) {
            return false;
        }

        if(ReferenceEquals(this, other)) {
            return true;
        }

        return CategoryNameComparer.Instance.Equals(Category, other.Category)
               && DefinitionNameComparer.Instance.Equals(Definition, other.Definition)
               && ParamValue.Equals(other.ParamValue);
    }

    public override bool Equals(object obj) {
        return ReferenceEquals(this, obj) || obj is ElementIndex other && Equals(other);
    }

    public override int GetHashCode() {
        unchecked {
            int hashCode =
                (Category != null ? CategoryNameComparer.Instance.GetHashCode(Category) : 0);

            hashCode =
                (hashCode * 397) ^ (Definition != null ? DefinitionNameComparer.Instance.GetHashCode(Definition) : 0);

            hashCode =
                (hashCode * 397) ^ (ParamValue != null ? ParamValue.GetHashCode() : 0);

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
