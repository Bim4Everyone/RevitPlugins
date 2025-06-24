using System;

using Autodesk.Revit.DB;

using dosymep.Revit;

namespace RevitSleeves.Services.Core;
internal class OpeningGeometryProvider : IOpeningGeometryProvider {
    public const string FamilyNameOpeningArRectangleInFloor = "Окн_Отв_Прямоуг_Перекрытие";
    public const string FamilyNameOpeningArRectangleInWall = "Окн_Отв_Прямоуг_Стена";
    public const string FamilyNameOpeningArRoundInFloor = "Окн_Отв_Круг_Перекрытие";
    public const string FamilyNameOpeningArRoundInWall = "Окн_Отв_Круг_Стена";

    public const string ParameterOpeningArDiameter = "ФОП_РАЗМ_Диаметр";
    public const string ParameterOpeningArWidth = "ФОП_РАЗМ_Ширина проёма";
    public const string ParameterOpeningArHeight = "ФОП_РАЗМ_Высота проёма";
    public const string ParameterOpeningArThickness = "ФОП_РАЗМ_Глубина проёма";

    public const string FamilyNameOpeningKrRectangleInFloor = "ОбщМд_Отверстие_Перекрытие_Прямоугольное";
    public const string FamilyNameOpeningKrRectangleInWall = "ОбщМд_Отверстие_Стена_Прямоугольное";
    public const string FamilyNameOpeningKrRoundInWall = "ОбщМд_Отверстие_Стена_Круглое";

    public const string ParameterOpeningKrDiameter = "ФОП_РАЗМ_Диаметр";
    public const string ParameterOpeningKrInWallWidth = "ФОП_РАЗМ_Ширина";
    public const string ParameterOpeningKrInWallHeight = "ФОП_РАЗМ_Высота";
    public const string ParameterOpeningKrThickness = "ФОП_РАЗМ_Глубина";
    public const string ParameterOpeningKrInFloorHeight = "мод_ФОП_Габарит А";
    public const string ParameterOpeningKrInFloorWidth = "мод_ФОП_Габарит Б";

    /// <summary>
    /// Находит солид экземпляра семейства отверстия исходя из координат точки вставки, 
    /// формы семейства и значений параметров.
    /// </summary>
    public Solid GetSolid(FamilyInstance opening) {
        if(opening is null) {
            throw new ArgumentNullException(nameof(opening));
        }
        return opening.Symbol.FamilyName switch {
            FamilyNameOpeningArRectangleInWall => GetWallRectangleSolid(
                opening,
                ParameterOpeningArWidth,
                ParameterOpeningArHeight,
                ParameterOpeningArThickness),

            FamilyNameOpeningArRoundInWall => GetWallRoundSolid(
                opening,
                ParameterOpeningArDiameter,
                ParameterOpeningArThickness),

            FamilyNameOpeningArRectangleInFloor => GetFloorRectangleSolid(
                opening,
                ParameterOpeningArWidth,
                ParameterOpeningArHeight,
                ParameterOpeningArThickness),

            FamilyNameOpeningArRoundInFloor => GetFloorRoundSolid(
                opening,
                ParameterOpeningArDiameter,
                ParameterOpeningArThickness),

            FamilyNameOpeningKrRectangleInWall => GetWallRectangleSolid(
                opening,
                ParameterOpeningKrInWallWidth,
                ParameterOpeningKrInWallHeight,
                ParameterOpeningKrThickness),

            FamilyNameOpeningKrRoundInWall => GetWallRoundSolid(
                opening,
                ParameterOpeningKrDiameter,
                ParameterOpeningKrThickness),

            FamilyNameOpeningKrRectangleInFloor => GetFloorRectangleSolid(
                opening,
                ParameterOpeningKrInFloorWidth,
                ParameterOpeningKrInFloorHeight,
                ParameterOpeningKrThickness),

            _ => throw new InvalidOperationException()
        };
    }

    /// <summary>
    /// Находит солид прямоугольного отверстия в стене. 
    /// Точка вставки экземпляра семейства - центр нижней грани параллелепипеда отверстия.
    /// </summary>
    /// <returns>Параллелепипед, построенный в соответствии с семейством.</returns>
    private Solid GetWallRectangleSolid(FamilyInstance opening,
        string widthName,
        string heightName,
        string thicknessName) {

        if(opening.IsExistsSharedParam(widthName)
            && opening.IsExistsSharedParam(heightName)
            && opening.IsExistsSharedParam(thicknessName)) {

            (var frontNormal, var upDir, var leftDir) = GetOrientationVectors(opening);
            double width = opening.GetSharedParamValue<double>(widthName);
            double height = opening.GetSharedParamValue<double>(heightName);
            double thickness = opening.GetSharedParamValue<double>(thicknessName);
            var loopLeftUpperCorner = (opening.Location as LocationPoint).Point
                - frontNormal * thickness / 2
                + leftDir * width / 2
                + upDir * height;
            var loopRightUpperCorner = loopLeftUpperCorner - leftDir * width;
            var loopRightBottomCorner = loopRightUpperCorner - upDir * height;
            var loopLeftBottomCorner = loopRightBottomCorner + leftDir * width;

            var rectangle = CurveLoop.Create([
                    Line.CreateBound(loopLeftUpperCorner, loopRightUpperCorner),
                    Line.CreateBound(loopRightUpperCorner, loopRightBottomCorner),
                    Line.CreateBound(loopRightBottomCorner, loopLeftBottomCorner),
                    Line.CreateBound(loopLeftBottomCorner, loopLeftUpperCorner)
                ]);
            return GeometryCreationUtilities.CreateExtrusionGeometry([rectangle], frontNormal, thickness);
        } else {
            throw new InvalidOperationException();
        }
    }

    /// <summary>
    /// Находит солид круглого отверстия в стене.
    /// Точка вставки экземпляра семейства - геометрический центр цилиндра.
    /// </summary>
    /// <returns>Горизонтальный цилиндр, построенный в соответствии с семейством.</returns>
    private Solid GetWallRoundSolid(FamilyInstance opening, string diameterName, string thicknessName) {
        if(opening.IsExistsSharedParam(diameterName)
            && opening.IsExistsSharedParam(thicknessName)) {

            (var frontNormal, var upDir, var leftDir) = GetOrientationVectors(opening);
            double diameter = opening.GetSharedParamValue<double>(diameterName);
            double thickness = opening.GetSharedParamValue<double>(thicknessName);

            var circleOrigin = (opening.Location as LocationPoint).Point - frontNormal * thickness / 2;
            var leftPoint = circleOrigin + leftDir * diameter / 2;
            var topPoint = circleOrigin + upDir * diameter / 2;
            var rightPoint = circleOrigin - leftDir * diameter / 2;
            var bottomPoint = circleOrigin - upDir * diameter / 2;

            var circle = CurveLoop.Create([
                    Arc.Create(leftPoint, rightPoint, topPoint),
                    Arc.Create(rightPoint, leftPoint, bottomPoint)
                ]);
            return GeometryCreationUtilities.CreateExtrusionGeometry([circle], frontNormal, thickness);
        } else {
            throw new InvalidOperationException();
        }
    }

    /// <summary>
    /// Находит солид прямоугольного отверстия в перекрытии.
    /// Точка вставки экземпляра семейства - центр верхней грани параллелепипеда.
    /// </summary>
    /// <returns>Параллелепипед, построенный в соответствии с семейством.</returns>
    private Solid GetFloorRectangleSolid(
        FamilyInstance opening,
        string widthName,
        string heightName,
        string thicknessName) {

        if(opening.IsExistsSharedParam(widthName)
            && opening.IsExistsSharedParam(heightName)
            && opening.IsExistsSharedParam(thicknessName)) {

            (var frontDir, var upDir, var leftDir) = GetOrientationVectors(opening);
            double width = opening.GetSharedParamValue<double>(widthName);
            double height = opening.GetSharedParamValue<double>(heightName);
            double thickness = opening.GetSharedParamValue<double>(thicknessName);
            var loopLeftUpperCorner = (opening.Location as LocationPoint).Point
                + leftDir * width / 2
                + frontDir * height / 2;
            var loopRightUpperCorner = loopLeftUpperCorner - leftDir * width;
            var loopRightBottomCorner = loopRightUpperCorner - frontDir * height;
            var loopLeftBottomCorner = loopRightBottomCorner + leftDir * width;

            var rectangle = CurveLoop.Create([
                    Line.CreateBound(loopLeftUpperCorner, loopRightUpperCorner),
                    Line.CreateBound(loopRightUpperCorner, loopRightBottomCorner),
                    Line.CreateBound(loopRightBottomCorner, loopLeftBottomCorner),
                    Line.CreateBound(loopLeftBottomCorner, loopLeftUpperCorner)
                ]);
            return GeometryCreationUtilities.CreateExtrusionGeometry([rectangle], -upDir, thickness);
        } else {
            throw new InvalidOperationException();
        }
    }

    /// <summary>
    /// Находит солид круглого отверстия в перекрытии.
    /// Точка вставки экземпляра семейства - центр верхней грани цилиндра.
    /// </summary>
    /// <returns>Вертикальный цилиндр, построенный в соответствии с семейством.</returns>
    private Solid GetFloorRoundSolid(FamilyInstance opening, string diameterName, string thicknessName) {
        if(opening.IsExistsSharedParam(diameterName)
            && opening.IsExistsSharedParam(thicknessName)) {

            (var frontDir, var upDir, var leftDir) = GetOrientationVectors(opening);
            double diameter = opening.GetSharedParamValue<double>(diameterName);
            double thickness = opening.GetSharedParamValue<double>(thicknessName);

            var circleOrigin = (opening.Location as LocationPoint).Point;
            var leftPoint = circleOrigin + leftDir * diameter / 2;
            var topPoint = circleOrigin + frontDir * diameter / 2;
            var rightPoint = circleOrigin - leftDir * diameter / 2;
            var bottomPoint = circleOrigin - frontDir * diameter / 2;

            var circle = CurveLoop.Create([
                    Arc.Create(leftPoint, rightPoint, topPoint),
                    Arc.Create(rightPoint, leftPoint, bottomPoint)
                ]);
            return GeometryCreationUtilities.CreateExtrusionGeometry([circle], -upDir, thickness);
        } else {
            throw new InvalidOperationException();
        }
    }

    /// <summary>
    /// Возвращает нормализованные векторы ориентации экземпляра семейства.
    /// </summary>
    /// <returns>Вперед, вверх, влево.</returns>
    private (XYZ frontDir, XYZ upDir, XYZ leftDir) GetOrientationVectors(FamilyInstance opening) {
        var frontNormal = opening.FacingOrientation;
        var upDir = XYZ.BasisZ;
        var leftDir = upDir.CrossProduct(frontNormal).Normalize();
        return (frontNormal, upDir, leftDir);
    }
}
