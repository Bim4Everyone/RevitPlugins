using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitValueModifier.Models {
    internal class RevitParameterComparerById : IEqualityComparer<Parameter> {
        public bool Equals(Parameter param1, Parameter param2) {
            if(param1.Id == param2.Id) {
                //TaskDialog.Show("RevitParameterComparerById", "RevitParameterComparerById");
                return true;

            } else
                return false;
        }

        public int GetHashCode(Parameter obj) {
            return obj.Id.GetHashCode();
        }
    }
}
