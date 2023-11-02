using System;

using Autodesk.Revit.DB;

using pyRevitLabs.Json;

namespace RevitClashDetective.Models.Clashes {
    internal class XYZModel : IEquatable<XYZModel> {
        [JsonConstructor]
        public XYZModel() { }

        public XYZModel(XYZ xyz) {
            if(xyz is null) { throw new ArgumentNullException(nameof(xyz)); }

            X = xyz.X;
            Y = xyz.Y;
            Z = xyz.Z;
        }

        public double X { get; set; } = 0;
        public double Y { get; set; } = 0;
        public double Z { get; set; } = 0;

        public override bool Equals(object obj) {
            return Equals(obj as XYZModel);
        }

        public override int GetHashCode() {
            int hashCode = -307843816;
            hashCode = hashCode * -1521134295 + X.GetHashCode();
            hashCode = hashCode * -1521134295 + Y.GetHashCode();
            hashCode = hashCode * -1521134295 + Z.GetHashCode();
            return hashCode;
        }

        public bool Equals(XYZModel other) {
            if(ReferenceEquals(null, other)) { return false; }
            if(ReferenceEquals(this, other)) { return true; }

            return X == other.X
                && Y == other.Y
                && Z == other.Z;
        }

        public XYZ GetXYZ() {
            return new XYZ(X, Y, Z);
        }
    }
}
