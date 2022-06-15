using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

namespace RevitClashDetective.Models {
    internal static class TransformExtension {
        public static Transform GetTransitionMatrix(this Transform transform, Transform otherTransform) {
            var inversedTransform = transform.Inverse;
            return inversedTransform.Multiply(otherTransform);
        }
    }
}
