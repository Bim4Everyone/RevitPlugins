using System;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitOpeningPlacement.Models;
using RevitOpeningPlacement.Models.RealOpeningArPlacement;
using RevitOpeningPlacement.Models.RealOpeningKrPlacement;

namespace RevitOpeningPlacement.Services;

/// <summary>
/// Сервис построения геометрии экземпляров семейств, у которых нет собственной геометрии в модели.
/// <para>
/// Алгоритмы класса исходят из утверждений, что:<br/>
/// Прямоугольное отверстие в стене - это прямой параллелепипед,
/// точка вставки которого - центр нижней грани.<br/>
/// Круглое отверстие в стене - это горизонтальный прямой цилиндр,
/// точка вставки которого - геометрический центр цилиндра.<br/>
/// Прямоугольное отверстие в перекрытии - это прямой параллелепипед,
/// точка вставки которого - центр верхней грани.<br/>
/// Круглое отверстие в перекрытии - это вертикальный прямой цилиндр,
/// точка вставки которого - центр верхней грани.
/// </para>
/// </summary>
internal class FamilyGeometryProvider : IFamilyGeometryProvider {
    public FamilyGeometryProvider() {
    }

    public Solid GetSolid(FamilyInstance instance) {
        return GetSolid(instance, 0);
    }

    public Solid GetSolid(FamilyInstance instance, double inflation) {
        if(instance is null) {
            throw new ArgumentNullException(nameof(instance));
        }

        string familyName = instance.Symbol?.FamilyName;
        if(string.IsNullOrWhiteSpace(familyName)) {
            throw new InvalidOperationException(
                $"Не удалось определить название семейства элемента с Id {instance.Id}");
        }

        if(familyName.Equals(RevitRepository.OpeningRealArFamilyName[OpeningType.WallRectangle])) {
            return GetWallRectangleSolid(
                instance,
                RealOpeningArPlacer.RealOpeningArWidth,
                RealOpeningArPlacer.RealOpeningArHeight,
                RealOpeningArPlacer.RealOpeningArThickness,
                inflation);
        } else if(familyName.Equals(RevitRepository.OpeningRealArFamilyName[OpeningType.WallRound])) {
            return GetWallRoundSolid(
                instance,
                RealOpeningArPlacer.RealOpeningArDiameter,
                RealOpeningArPlacer.RealOpeningArThickness,
                inflation);
        } else if(familyName.Equals(RevitRepository.OpeningRealArFamilyName[OpeningType.FloorRectangle])) {
            return GetFloorRectangleSolid(
                instance,
                RealOpeningArPlacer.RealOpeningArWidth,
                RealOpeningArPlacer.RealOpeningArHeight,
                RealOpeningArPlacer.RealOpeningArThickness,
                inflation);
        } else if(familyName.Equals(RevitRepository.OpeningRealArFamilyName[OpeningType.FloorRound])) {
            return GetFloorRoundSolid(
                instance,
                RealOpeningArPlacer.RealOpeningArDiameter,
                RealOpeningArPlacer.RealOpeningArThickness,
                inflation);
        } else if(familyName.Equals(RevitRepository.OpeningRealKrFamilyName[OpeningType.WallRectangle])) {
            return GetWallRectangleSolid(
                instance,
                RealOpeningKrPlacer.RealOpeningKrInWallWidth,
                RealOpeningKrPlacer.RealOpeningKrInWallHeight,
                RealOpeningKrPlacer.RealOpeningKrThickness,
                inflation);
        } else if(familyName.Equals(RevitRepository.OpeningRealKrFamilyName[OpeningType.WallRound])) {
            return GetWallRoundSolid(
                instance,
                RealOpeningKrPlacer.RealOpeningKrDiameter,
                RealOpeningKrPlacer.RealOpeningKrThickness,
                inflation);
        } else if(familyName.Equals(RevitRepository.OpeningRealKrFamilyName[OpeningType.FloorRectangle])) {
            return GetFloorRectangleSolid(
                instance,
                RealOpeningKrPlacer.RealOpeningKrInFloorWidth,
                RealOpeningKrPlacer.RealOpeningKrInFloorHeight,
                RealOpeningKrPlacer.RealOpeningKrThickness,
                inflation);
        } else {
            throw new InvalidOperationException($"Семейство \"{familyName}\" не поддерживается");
        }
    }

    /// <summary>
    /// Находит солид прямоугольного отверстия в стене.
    /// Точка вставки экземпляра семейства - центр нижней грани параллелепипеда отверстия.
    /// </summary>
    /// <returns>Параллелепипед, построенный в соответствии с семейством.</returns>
    private Solid GetWallRectangleSolid(
        FamilyInstance instance,
        string widthName,
        string heightName,
        string thicknessName,
        double inflation) {
        (var frontNormal, var upDir, var leftDir) = GetOrientationVectors(instance);
        double width = GetSharedParamValue(instance, widthName) + 2 * inflation;
        double height = GetSharedParamValue(instance, heightName) + 2 * inflation;
        double thickness = GetSharedParamValue(instance, thicknessName);

        var origin = GetLocationPoint(instance) - upDir * inflation;
        var loopLeftUpperCorner = origin
                                  - frontNormal * thickness / 2
                                  + leftDir * width / 2
                                  + upDir * height;
        var loopRightUpperCorner = loopLeftUpperCorner - leftDir * width;
        var loopRightBottomCorner = loopRightUpperCorner - upDir * height;
        var loopLeftBottomCorner = loopRightBottomCorner + leftDir * width;

        var rectangle = CurveLoop.Create(
        [
            Line.CreateBound(loopLeftUpperCorner, loopRightUpperCorner),
            Line.CreateBound(loopRightUpperCorner, loopRightBottomCorner),
            Line.CreateBound(loopRightBottomCorner, loopLeftBottomCorner),
            Line.CreateBound(loopLeftBottomCorner, loopLeftUpperCorner)
        ]);
        return GeometryCreationUtilities.CreateExtrusionGeometry(
            [rectangle],
            frontNormal,
            thickness);
    }

    /// <summary>
    /// Находит солид круглого отверстия в стене.
    /// Точка вставки экземпляра семейства - геометрический центр цилиндра.
    /// </summary>
    /// <returns>Горизонтальный цилиндр, построенный в соответствии с семейством.</returns>
    private Solid GetWallRoundSolid(
        FamilyInstance instance,
        string diameterName,
        string thicknessName,
        double inflation) {
        (var frontNormal, var upDir, var leftDir) = GetOrientationVectors(instance);
        double diameter = GetSharedParamValue(instance, diameterName) + 2 * inflation;
        double thickness = GetSharedParamValue(instance, thicknessName);

        var circleOrigin = GetLocationPoint(instance) - frontNormal * thickness / 2;
        var circle = CreateCircle(circleOrigin, leftDir, upDir, diameter);
        return GeometryCreationUtilities.CreateExtrusionGeometry(
            [circle],
            frontNormal,
            thickness);
    }

    /// <summary>
    /// Находит солид прямоугольного отверстия в перекрытии.
    /// Точка вставки экземпляра семейства - центр верхней грани параллелепипеда.
    /// </summary>
    /// <returns>Параллелепипед, построенный в соответствии с семейством.</returns>
    private Solid GetFloorRectangleSolid(
        FamilyInstance instance,
        string widthName,
        string heightName,
        string thicknessName,
        double inflation) {
        (var frontDir, var upDir, var leftDir) = GetOrientationVectors(instance);
        double width = GetSharedParamValue(instance, widthName) + 2 * inflation;
        double height = GetSharedParamValue(instance, heightName) + 2 * inflation;
        double thickness = GetSharedParamValue(instance, thicknessName);

        var origin = GetLocationPoint(instance);
        var loopLeftUpperCorner = origin
                                  + leftDir * width / 2
                                  + frontDir * height / 2;
        var loopRightUpperCorner = loopLeftUpperCorner - leftDir * width;
        var loopRightBottomCorner = loopRightUpperCorner - frontDir * height;
        var loopLeftBottomCorner = loopRightBottomCorner + leftDir * width;

        var rectangle = CurveLoop.Create(
        [
            Line.CreateBound(loopLeftUpperCorner, loopRightUpperCorner),
            Line.CreateBound(loopRightUpperCorner, loopRightBottomCorner),
            Line.CreateBound(loopRightBottomCorner, loopLeftBottomCorner),
            Line.CreateBound(loopLeftBottomCorner, loopLeftUpperCorner)
        ]);
        return GeometryCreationUtilities.CreateExtrusionGeometry(
            [rectangle],
            -upDir,
            thickness);
    }

    /// <summary>
    /// Находит солид круглого отверстия в перекрытии.
    /// Точка вставки экземпляра семейства - центр верхней грани цилиндра.
    /// </summary>
    /// <returns>Вертикальный цилиндр, построенный в соответствии с семейством.</returns>
    private Solid GetFloorRoundSolid(
        FamilyInstance instance,
        string diameterName,
        string thicknessName,
        double inflation) {
        (var frontDir, var upDir, var leftDir) = GetOrientationVectors(instance);
        double diameter = GetSharedParamValue(instance, diameterName) + 2 * inflation;
        double thickness = GetSharedParamValue(instance, thicknessName);

        var circle = CreateCircle(GetLocationPoint(instance), leftDir, frontDir, diameter);
        return GeometryCreationUtilities.CreateExtrusionGeometry(
            [circle],
            -upDir,
            thickness);
    }

    /// <summary>
    /// Создает окружность заданного диаметра в плоскости заданных векторов.
    /// </summary>
    /// <param name="origin">Центр окружности.</param>
    /// <param name="firstDir">Первый вектор плоскости окружности.</param>
    /// <param name="secondDir">Второй вектор плоскости окружности.</param>
    /// <param name="diameter">Диаметр окружности.</param>
    private CurveLoop CreateCircle(XYZ origin, XYZ firstDir, XYZ secondDir, double diameter) {
        var leftPoint = origin + firstDir * diameter / 2;
        var topPoint = origin + secondDir * diameter / 2;
        var rightPoint = origin - firstDir * diameter / 2;
        var bottomPoint = origin - secondDir * diameter / 2;

        return CurveLoop.Create(
            [Arc.Create(leftPoint, rightPoint, topPoint), Arc.Create(rightPoint, leftPoint, bottomPoint)]);
    }

    /// <summary>
    /// Возвращает нормализованные векторы ориентации экземпляра семейства.
    /// </summary>
    /// <returns>Вперед, вверх, влево.</returns>
    private (XYZ frontDir, XYZ upDir, XYZ leftDir) GetOrientationVectors(FamilyInstance instance) {
        var frontNormal = instance.FacingOrientation;
        var upDir = XYZ.BasisZ;
        var leftDir = upDir.CrossProduct(frontNormal).Normalize();
        return (frontNormal, upDir, leftDir);
    }

    /// <summary>
    /// Возвращает точку вставки экземпляра семейства.
    /// </summary>
    private XYZ GetLocationPoint(FamilyInstance instance) {
        return ((LocationPoint) instance.Location).Point;
    }

    /// <summary>
    /// Возвращает значение общего параметра экземпляра семейства в единицах длины Revit.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Исключение, если у экземпляра семейства нет заданного общего параметра.</exception>
    private double GetSharedParamValue(Element element, string paramName) {
        return element.IsExistsSharedParam(paramName)
            ? element.GetSharedParamValue<double>(paramName)
            : throw new InvalidOperationException(
                $"У элемента с Id {element.Id} из файла {element.Document.Title} отсутствует общий параметр \"{paramName}\"");
    }
}
