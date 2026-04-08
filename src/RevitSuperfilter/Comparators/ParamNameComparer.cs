using System;
using System.Collections.Generic;
using System.Globalization;

using Autodesk.Revit.DB;

namespace RevitSuperfilter.Comparators;

internal sealed class ParamNameComparer : IComparer<Parameter>, IEqualityComparer<Parameter> {
    public static readonly ParamNameComparer Instance = new();
    
    public bool Equals(Parameter x, Parameter y) {
        if(ReferenceEquals(x, y)) {
            return true;
        }

        if(x is null) {
            return false;
        }

        if(y is null) {
            return false;
        }

        if(x.GetType() != y.GetType()) {
            return false;
        }

        return Equals(x.Definition?.Name, y.Definition?.Name);
    }

    public int GetHashCode(Parameter obj) {
        return (obj.Definition?.Name != null ? obj.Definition.Name.GetHashCode() : 0);
    }

    public int Compare(Parameter x, Parameter y) {
        if(ReferenceEquals(x, y)) {
            return 0;
        }

        if(y is null) {
            return 1;
        }

        if(x is null) {
            return -1;
        }

        return StringComparer.CurrentCulture.Compare(x.Definition?.Name, y.Definition?.Name);
    }
}
