using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitSuperfilter.Models {
    internal class DefinitionComparer : IEqualityComparer<Definition> {
        public bool Equals(Definition x, Definition y) {
            if(x == null && y == null) {
                return true;
            }

            if(x == null || y == null) {
                return false;
            }

            return x.Name.Equals(y.Name);
        }

        public int GetHashCode(Definition obj) {
            return obj?.Name.GetHashCode() ?? 0;
        }
    }
}
