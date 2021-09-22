using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitSuperfilter.Models {
    internal class ParamComparer : IEqualityComparer<Parameter> {
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

            return x.Definition.Name.Equals(y.Definition.Name);
        }

        public int GetHashCode(Parameter obj) {
            if(obj == null || obj.Definition == null) {
                return 0;
            }

            return obj.Definition.Name.GetHashCode();
        }
    }
}