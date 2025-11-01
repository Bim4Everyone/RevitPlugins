using Autodesk.Revit.DB;

namespace RevitSetCoordParams.Models.Interfaces;
internal interface ISphereProvider {
    /// <summary>
    /// Метод получения Solid по координате и диаметру
    /// </summary>
    Solid GetSphere(XYZ location, double diameter);
}
