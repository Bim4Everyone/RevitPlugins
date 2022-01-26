using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

namespace RevitSuperfilter.Models {
    internal class ElementTypeNameComparer : IEqualityComparer<ElementType> {
        public bool Equals(ElementType x, ElementType y) {
            if(x == null && y == null) {
                return true;
            }

            if(x == null || y == null) {
                return false;
            }

            return x.Name.Equals(y.Name);
        }

        public int GetHashCode(ElementType obj) {
            return obj?.Name.GetHashCode() ?? 0;
        }
    }
}
