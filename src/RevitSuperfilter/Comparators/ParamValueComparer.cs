using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.Revit;

namespace RevitSuperfilter.Comparators;

internal sealed class ParamValueComparer : IComparer<Parameter>, IEqualityComparer<Parameter> {
    public static readonly ParamValueComparer Instance = new();
    
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

        if(x.Definition is null) {
            return false;
        }

        if(y.Definition is null) {
            return false;
        }

        if(x.HasValue != y.HasValue) {
            return false;
        }

        try {
            if(x.StorageType != y.StorageType) {
                return false;
            }
        } catch {
            // Не все параметры имеют тип возвращаемого значения
        }

        try {
#if REVIT_2020_OR_LESS
            if(x.DisplayUnitType != y.DisplayUnitType) {
                return false;
            }
#else
            if(x.GetUnitTypeId() != y.GetUnitTypeId()) {
                return false;
            }
#endif
        } catch {
            // Не все параметры имеют отображаемую единицу измерения
        }

        try {
            return x.StorageType switch {
                StorageType.None => true,
                StorageType.Integer => x.AsInteger() == y.AsInteger(),
                StorageType.Double => Math.Abs(x.AsDouble() - y.AsDouble()) < 0.001,
                StorageType.String => x.AsString() == y.AsString(),
                StorageType.ElementId => x.AsElementId() == y.AsElementId(),
                _ => throw new ArgumentOutOfRangeException()
            };
        } catch {
            // Есть некоторые параметры, которые отваливаются при попытке получить эти значения
        }

        return false;
    }

    public int GetHashCode(Parameter obj) {
        if(obj.Definition == null) {
            return 0;
        }

        if(obj.HasValue) {
            return obj.StorageType switch {
                StorageType.None => 0,
                StorageType.Integer => obj.AsInteger().GetHashCode(),
                StorageType.Double => obj.AsDouble().GetHashCode(),
                StorageType.String => obj.AsString().GetHashCode(),
                StorageType.ElementId => obj.AsElementId().GetHashCode(),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        return 0;
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

        if(y.Definition is null) {
            return 1;
        }

        if(x.Definition is null) {
            return -1;
        }

        if(!y.HasValue) {
            return 1;
        }

        if(!x.HasValue) {
            return -1;
        }

        if(x.StorageType != y.StorageType) {
            return x.StorageType.CompareTo(y.StorageType);
        }

        return x.StorageType switch {
            StorageType.None => 0,
            StorageType.Integer => x.AsInteger().CompareTo(y.AsInteger()),
            StorageType.Double => x.AsDouble().CompareTo(y.AsDouble()),
            StorageType.String => StringComparer.CurrentCulture.Compare(x.AsString(), y.AsString()),
            StorageType.ElementId => x.AsElementId().Compare(y.AsElementId()),
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}
