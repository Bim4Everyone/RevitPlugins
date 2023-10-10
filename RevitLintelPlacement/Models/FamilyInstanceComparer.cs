using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

namespace RevitLintelPlacement.Models {
    internal class FamilyInstanceComparer : IEqualityComparer<FamilyInstance> {
        public bool Equals(FamilyInstance x, FamilyInstance y) {
            return x?.Id == y?.Id;
        }

        public int GetHashCode(FamilyInstance obj) {
            return obj.Id.GetHashCode();
        }
    }
}
