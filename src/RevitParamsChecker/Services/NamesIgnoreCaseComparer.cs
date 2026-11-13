using System;
using System.Collections.Generic;

using RevitParamsChecker.Models;

namespace RevitParamsChecker.Services;

internal class NamesIgnoreCaseComparer : IEqualityComparer<IName> {
    public bool Equals(IName x, IName y) {
        if(x is null
           || y is null
           || string.IsNullOrWhiteSpace(x.Name)
           || string.IsNullOrWhiteSpace(y.Name)) {
            return false;
        }

        if(ReferenceEquals(x, y)) {
            return true;
        }

        return x.Name.Equals(y.Name, StringComparison.CurrentCultureIgnoreCase);
    }

    public int GetHashCode(IName obj) {
        return EqualityComparer<string>.Default.GetHashCode(obj.Name?.ToLower());
    }
}
