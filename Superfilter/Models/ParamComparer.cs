using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace Superfilter.Models {
    internal class ParamComparer : IEqualityComparer<Parameter> {
        public bool Equals(Parameter x, Parameter y) {
            if(x == null && y == null) {
                return true;
            }

            if(x == null || y == null) {
                return false;
            }

            return x.Id == y.Id;
        }

        public int GetHashCode(Parameter obj) {
            return obj?.Id.GetHashCode() ?? 0;
        }
    }
}
