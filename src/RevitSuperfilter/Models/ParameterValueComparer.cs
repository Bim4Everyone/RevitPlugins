using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.Revit;

namespace RevitSuperfilter.Models {
    internal class ParameterValueComparer : IEqualityComparer<Parameter> {
        public bool Equals(Parameter x, Parameter y) {
            if(x == null && y == null) {
                return true;
            }

            if(x == null || y == null) {
                return false;
            }

            if(x.Definition == null || y.Definition == null) {
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
                string xValue = x.AsValueString();
                string yValue = y.AsValueString();

                if(string.IsNullOrEmpty(xValue)) {
                    xValue = x.AsObject()?.ToString();
                }

                if(string.IsNullOrEmpty(yValue)) {
                    yValue = y.AsObject()?.ToString();
                }

                if(string.IsNullOrEmpty(xValue) && string.IsNullOrEmpty(yValue)) {
                    return true;
                }

                if(string.IsNullOrEmpty(xValue) || string.IsNullOrEmpty(yValue)) {
                    return false;
                }

                return xValue.Equals(yValue);
            } catch {
                // Есть некоторые параметры, которые отваливаются при попытке получить эти значения
            }

            return false;
        }

        public int GetHashCode(Parameter obj) {
            if(obj == null) {
                return 0;
            }

            if(obj.Definition == null) {
                return 0;
            }

            if(obj.HasValue) {
                string value = obj.AsValueString();
                if(string.IsNullOrEmpty(value)) {
                    value = obj.AsObject()?.ToString();
                }

                return value?.GetHashCode() ?? 0;
            }

            return 0;
        }
    }
}
