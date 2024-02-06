
using Autodesk.Revit.DB;

namespace RevitOpeningPlacement.Models.Interfaces {
    internal interface ISolidProvider {
        Solid GetSolid();

        BoundingBoxXYZ GetTransformedBBoxXYZ();
    }
}
