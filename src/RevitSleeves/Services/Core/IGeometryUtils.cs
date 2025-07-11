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
}
