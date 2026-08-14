using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

namespace RevitPunchingRebar.Models.Interfaces;
internal interface IPylon {
    Element PylonInstance { get; set; }

    // вектор нормали к одной из коротких сторон пилона
    XYZ FacingOrientation { get; set; }

    double Length { get; set; }
    double Width { get; set; }
    double Height { get; set; }

    /// <summary>
    /// Возвращает нижнюю центральную точку
    /// </summary>
    /// <returns></returns>
    XYZ GetLocation();
}
