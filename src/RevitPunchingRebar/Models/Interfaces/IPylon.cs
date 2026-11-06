using Autodesk.Revit.DB;

namespace RevitPunchingRebar.Models.Interfaces;
internal interface IPylon {
    Element PylonInstance { get; }

    // вектор нормали к одной из коротких сторон пилона
    XYZ FacingOrientation { get; }

    double Length { get; }
    double Width { get; }
    double Height { get; }

    /// <summary>
    /// Возвращает нижнюю центральную точку
    /// </summary>
    /// <returns></returns>
    XYZ GetLocation();
}
