using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitOpeningPlacement.Models.Interfaces {
    internal interface IDirectionsGetter {
        IEnumerable<XYZ> GetDirectionsOnPlane(Plane plane);
    }
}
