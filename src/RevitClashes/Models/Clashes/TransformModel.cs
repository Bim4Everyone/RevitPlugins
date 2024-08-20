using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using pyRevitLabs.Json;

namespace RevitClashDetective.Models.Clashes {
    internal class TransformModel : IEquatable<TransformModel> {
        [JsonConstructor]
        public TransformModel() { }

        public TransformModel(Transform transform) {
            if(transform is null) { throw new ArgumentNullException(nameof(transform)); }

            Origin = new XYZModel(transform.Origin);
            BasisX = new XYZModel(transform.BasisX);
            BasisY = new XYZModel(transform.BasisY);
            BasisZ = new XYZModel(transform.BasisZ);
        }


        public XYZModel BasisX { get; set; }

        public XYZModel BasisY { get; set; }

        public XYZModel BasisZ { get; set; }

        public XYZModel Origin { get; set; }


        public override bool Equals(object obj) {
            return Equals(obj as TransformModel);
        }

        public override int GetHashCode() {
            int hashCode = -354951233;
            hashCode = hashCode * -1521134295 + EqualityComparer<XYZModel>.Default.GetHashCode(BasisX);
            hashCode = hashCode * -1521134295 + EqualityComparer<XYZModel>.Default.GetHashCode(BasisY);
            hashCode = hashCode * -1521134295 + EqualityComparer<XYZModel>.Default.GetHashCode(BasisZ);
            hashCode = hashCode * -1521134295 + EqualityComparer<XYZModel>.Default.GetHashCode(Origin);
            return hashCode;
        }

        public bool Equals(TransformModel other) {
            if(ReferenceEquals(null, other)) { return false; }
            if(ReferenceEquals(this, other)) { return true; }

            return Equals(BasisX, other.BasisX)
                && Equals(BasisY, other.BasisY)
                && Equals(BasisZ, other.BasisZ)
                && Equals(Origin, other.Origin)
                ;
        }

        public bool IsAlmostEqualTo(Transform transform) {
            if(ReferenceEquals(null, transform)) { return false; }
            if(Origin is null || BasisX is null || BasisY is null || BasisZ is null) { return false; }

            return Origin.GetXYZ().IsAlmostEqualTo(transform.Origin)
                && BasisX.GetXYZ().IsAlmostEqualTo(transform.BasisX)
                && BasisY.GetXYZ().IsAlmostEqualTo(transform.BasisY)
                && BasisZ.GetXYZ().IsAlmostEqualTo(transform.BasisZ);
        }

        public Transform GetTransform() {
            Transform transform = Transform.Identity;
            transform.Origin = Origin.GetXYZ();
            transform.BasisX = BasisX.GetXYZ();
            transform.BasisY = BasisY.GetXYZ();
            transform.BasisZ = BasisZ.GetXYZ();
            return transform;
        }
    }
}
