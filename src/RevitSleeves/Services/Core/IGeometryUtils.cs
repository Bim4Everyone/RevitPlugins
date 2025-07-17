using Autodesk.Revit.DB;

namespace RevitSleeves.Services.Core;
internal interface IGeometryUtils {
    bool IsHorizontal(Floor floor);

    double GetFloorThickness(Floor structureElement);

    bool IsVertical(MEPCurve curve);

    bool IsHorizontal(MEPCurve curve);

    /// <summary>
    /// Создает солид стены без отверстий
    /// </summary>
    /// <param name="wall">Стена</param>
    /// <param name="transform">Трансформация солида</param>
    /// <returns>Солид стены без отверстий с заданной трансформацией</returns>
    Solid CreateWallSolid(Wall wall, Transform transform = null);

    /// <summary>
    /// Создает прямой цилиндр по центру нижнего основания, вектору нормали, радиусу и высоте
    /// </summary>
    /// <param name="bottomPoint">Центр нижнего основания</param>
    /// <param name="topDir">Вектор нормали, направленный от нижнего основания к верхнему</param>
    /// <param name="radius">Радиус цилиндра в единицах Revit</param>
    /// <param name="height">Высота цилиндра в единицах Revit</param>
    /// <returns>Солид</returns>
    Solid CreateCylinder(XYZ bottomPoint, XYZ topDir, double radius, double height);
}
