using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitMarkingElements.Models {
    public class CurveElementComparer : IEqualityComparer<CurveElement> {
        public bool Equals(CurveElement x, CurveElement y) {
            if(x == null || y == null)
                return false;
            return x.Id.Equals(y.Id);
        }

        public int GetHashCode(CurveElement obj) {
            if(obj == null)
                return 0;
            return obj.Id.GetHashCode();
        }
    }
}
