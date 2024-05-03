using Autodesk.Revit.DB;

namespace RevitOpeningPlacement.Models.Extensions {
    /// <summary>
    /// Методы расширения для классов - наследников <see cref="Autodesk.Revit.DB.RoofBase">RoofBase</see>
    /// </summary>
    internal static class RoofBaseExtension {
        /// <summary>
        /// Возвращает верхнюю поверхность крыши
        /// </summary>
        /// <param name="roofBase"></param>
        /// <returns></returns>
        public static Face GetTopFace(this RoofBase roofBase) {
            var faceRefs = HostObjectUtils.GetTopFaces(roofBase);
            return (Face) roofBase.GetGeometryObjectFromReference(faceRefs[0]);
        }

        /// <summary>
        /// Возвращает нижнюю повержность крыши
        /// </summary>
        /// <param name="roofBase"></param>
        /// <returns></returns>
        public static Face GetBottomFace(this RoofBase roofBase) {
            var faceRefs = HostObjectUtils.GetBottomFaces(roofBase);
            return (Face) roofBase.GetGeometryObjectFromReference(faceRefs[0]);
        }
    }
}
