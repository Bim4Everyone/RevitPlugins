using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace Superfilter.Models {
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

            if(x.HasValue == y.HasValue) {
                return true;
            }

            if(x.HasValue || y.HasValue) {
                return false;
            }

            if(x.DisplayUnitType != y.DisplayUnitType) {
                return false;
            }

            if(x.StorageType != y.StorageType) {
                return false;
            }

            try {
                return x.AsValueString()?.Equals(y.AsValueString(), System.StringComparison.CurrentCultureIgnoreCase) == true;
            } catch {
                return false;
            }
        }

        public int GetHashCode(Parameter obj) {
            if(obj == null) {
                return 0;
            }

            if(obj.Definition == null) {
                return 0;
            }

            if(obj.HasValue) {
                return obj.AsValueString()?.GetHashCode() ?? 0;
            }

            return "Без значения".GetHashCode();
        }
    }
}
