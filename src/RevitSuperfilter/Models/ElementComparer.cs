using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitSuperfilter.Models {
    internal class ElementComparer : IEqualityComparer<Element> {
        public bool Equals(Element x, Element y) {
            if(x == null && y == null) {
                return true;
            }

            if(x == null || y == null) {
                return false;
            }

            return x.Id == y.Id;
        }

        public int GetHashCode(Element obj) {
            return obj?.Id.GetHashCode() ?? 0;
        }
    }

    internal class ElementNameComparer : IEqualityComparer<Element> {
        public bool Equals(Element x, Element y) {
            if(x == null && y == null) {
                return true;
            }

            if(x == null || y == null) {
                return false;
            }

            return x.Name.Equals(y.Name);
        }

        public int GetHashCode(Element obj) {
            return obj?.Name.GetHashCode() ?? 0;
        }
    }
}
