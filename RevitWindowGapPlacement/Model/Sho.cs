using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitWindowGapPlacement.Model {
    public class Sho : IEqualityComparer<HostObject> {
        public bool Equals(HostObject x, HostObject y)
        {
            if (ReferenceEquals(x, y)) {
                return true;
            }

            if (ReferenceEquals(x, null)) {
                return false;
            }

            if (ReferenceEquals(y, null)) {
                return false;
            }

            if (x.GetType() != y.GetType()) {
                return false;
            }

            return Equals(x.Id, y.Id);
        }

        public int GetHashCode(HostObject obj)
        {
            return (obj.Id != null ? obj.Id.GetHashCode() : 0);
        }
    }
}