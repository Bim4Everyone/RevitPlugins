using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitSuperfilter.Models {
    internal class CategoryComparer : IEqualityComparer<Category> {
        public bool Equals(Category x, Category y) {
            if(x == null && y == null) {
                return true;
            }

            if(x == null || y == null) {
                return false;
            }

            return x.Id == y.Id;
        }

        public int GetHashCode(Category obj) {
            return obj?.Id.GetHashCode() ?? 0;
        }
    }
}
