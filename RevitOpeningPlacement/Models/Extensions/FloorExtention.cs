using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.Revit;

namespace RevitOpeningPlacement.Models.Extensions {
    internal static class FloorExtention {
        public static List<BuiltInParameter> _thicknessParameters = new List<BuiltInParameter> {
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
    }
}
