using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.Revit;

namespace RevitOpeningPlacement.Models.Extensions {
    internal static class CeilingAndFloorExtension {
        private static List<BuiltInParameter> _thicknessParameters = new List<BuiltInParameter> {
            BuiltInParameter.FLOOR_ATTR_THICKNESS_PARAM,
            BuiltInParameter.FLOOR_ATTR_DEFAULT_THICKNESS_PARAM,
            BuiltInParameter.CEILING_THICKNESS,
        };

        public static double GetThickness(this Element element) {
            foreach(var param in _thicknessParameters) {
                if(element.IsExistsParam(param)) {
                    return element.GetParamValueOrDefault<double>(param);
                }
            }

            var type = element.GetTypeId().IsNotNull() ? element.Document.GetElement(element.GetTypeId()) : null;
            if(type != null) {
                return element.GetThickness();
            }

            return 0;
        }

        public static bool IsHorizontal(this CeilingAndFloor floor) {
            var normal = floor.GetTopFace().ComputeNormal(new UV(0, 0));
            return normal.IsPapallel(new XYZ(0, 0, 1));
        }

        public static Face GetTopFace(this CeilingAndFloor ceilingAndFloor) {
            var faceRefs = HostObjectUtils.GetTopFaces(ceilingAndFloor);
            return (Face) ceilingAndFloor.GetGeometryObjectFromReference(faceRefs[0]);
        }

        public static Face GetBottomFace(this CeilingAndFloor ceilingAndFloor) {
            var faceRefs = HostObjectUtils.GetBottomFaces(ceilingAndFloor);
            return (Face) ceilingAndFloor.GetGeometryObjectFromReference(faceRefs[0]);
        }
    }
}
