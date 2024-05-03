using Autodesk.Revit.DB;

namespace RevitClashDetective.Models.Extensions {
    internal static class TransformExtension {
        public static Transform GetTransitionMatrix(this Transform transform, Transform otherTransform) {
            var inversedTransform = transform.Inverse;
            return inversedTransform.Multiply(otherTransform);
        }
    }
}
