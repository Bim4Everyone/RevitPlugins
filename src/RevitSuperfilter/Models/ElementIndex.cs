using System;

namespace RevitSuperfilter.Models;

internal sealed class ElementIndex : IEquatable<ElementIndex> {
    public ElementIndex(string categoryName, string paramName, string paramValue) {
        CategoryName = categoryName;
        ParamName = paramName;
        ParamValue = paramValue;
    }

    public string CategoryName { get; }
    public string ParamName { get; }
    public string ParamValue { get; }

    #region IEquatable<ElementIndex>

    public bool Equals(ElementIndex other) {
        if(other is null) {
            return false;
        }

        if(ReferenceEquals(this, other)) {
            return true;
        }

        return CategoryName.Equals(other.CategoryName)
               && ParamName.Equals(other.ParamName)
               && ParamValue.Equals(other.ParamValue);
    }

    public override bool Equals(object obj) {
        return ReferenceEquals(this, obj) || obj is ElementIndex other && Equals(other);
    }

    public override int GetHashCode() {
        unchecked {
            int hashCode = (CategoryName != null ? CategoryName.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (ParamName != null ? ParamName.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (ParamValue != null ? ParamValue.GetHashCode() : 0);
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
