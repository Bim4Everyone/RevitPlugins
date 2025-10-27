using Autodesk.Revit.DB;

namespace RevitSetCoordParams.Models.Interfaces;
internal interface ISphereProvider {
    Solid GetSphere(XYZ location, double diameter);
}
