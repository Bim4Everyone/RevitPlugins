using System;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SharedParams;
using dosymep.Revit;

namespace RevitSleeves.Services.Core;
internal class OpeningGeometryProvider : IOpeningGeometryProvider {
    /// <summary>
    /// Находит солид экземпляра семейства отверстия исходя из координат точки вставки, 
    /// формы семейства и значений параметров.
    /// </summary>
    public Solid GetSolid(FamilyInstance opening) {
        if(opening is null) {
            throw new ArgumentNullException(nameof(opening));
        }
        return opening.Symbol.FamilyName switch {
            NamesProvider.FamilyNameOpeningArRectangleInWall => GetWallRectangleSolid(
                opening,
                NamesProvider.ParameterOpeningArWidth,
                NamesProvider.ParameterOpeningArHeight,
                NamesProvider.ParameterOpeningArThickness),

            NamesProvider.FamilyNameOpeningArRoundInWall => GetWallRoundSolid(
                opening,
                NamesProvider.ParameterOpeningArDiameter,
                NamesProvider.ParameterOpeningArThickness),

            NamesProvider.FamilyNameOpeningArRectangleInFloor => GetFloorRectangleSolid(
                opening,
                NamesProvider.ParameterOpeningArWidth,
                NamesProvider.ParameterOpeningArHeight,
                NamesProvider.ParameterOpeningArThickness),

            NamesProvider.FamilyNameOpeningArRoundInFloor => GetFloorRoundSolid(
                opening,
                NamesProvider.ParameterOpeningArDiameter,
                NamesProvider.ParameterOpeningArThickness),

            NamesProvider.FamilyNameOpeningKrRectangleInWall => GetWallRectangleSolid(
                opening,
                NamesProvider.ParameterOpeningKrInWallWidth,
                NamesProvider.ParameterOpeningKrInWallHeight,
                NamesProvider.ParameterOpeningKrThickness),

            NamesProvider.FamilyNameOpeningKrRoundInWall => GetWallRoundSolid(
                opening,
                NamesProvider.ParameterOpeningKrDiameter,
                NamesProvider.ParameterOpeningKrThickness),

            NamesProvider.FamilyNameOpeningKrRectangleInFloor => GetFloorRectangleSolid(
                opening,
                NamesProvider.ParameterOpeningKrInFloorWidth,
                NamesProvider.ParameterOpeningKrInFloorHeight,
                NamesProvider.ParameterOpeningKrThickness),

            _ => throw new InvalidOperationException()
        };
    }

    /// <summary>
    /// Находит солид прямоугольного отверстия в стене. 
    /// Точка вставки экземпляра семейства - центр нижней грани параллелепипеда отверстия.
    /// </summary>
    /// <returns>Параллелепипед, построенный в соответствии с семейством.</returns>
    private Solid GetWallRectangleSolid(FamilyInstance opening,
        SharedParam widthParam,
        SharedParam heightParam,
        SharedParam thicknessParam) {

        if(opening.IsExistsParam(widthParam)
            && opening.IsExistsParam(heightParam)
            && opening.IsExistsParam(thicknessParam)) {

            (var frontNormal, var upDir, var leftDir) = GetOrientationVectors(opening);
            double width = opening.GetParamValue<double>(widthParam);
            double height = opening.GetParamValue<double>(heightParam);
            double thickness = opening.GetParamValue<double>(thicknessParam);
            var loopLeftUpperCorner = ((LocationPoint) opening.Location).Point
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
    private Solid GetWallRoundSolid(FamilyInstance opening, SharedParam diameterParam, SharedParam thicknessParam) {
        if(opening.IsExistsParam(diameterParam)
            && opening.IsExistsParam(thicknessParam)) {

            (var frontNormal, var upDir, var leftDir) = GetOrientationVectors(opening);
            double diameter = opening.GetParamValue<double>(diameterParam);
            double thickness = opening.GetParamValue<double>(thicknessParam);

            var circleOrigin = ((LocationPoint) opening.Location).Point - frontNormal * thickness / 2;
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
        SharedParam widthParam,
        SharedParam heightParam,
        SharedParam thicknessParam) {

        if(opening.IsExistsParam(widthParam)
            && opening.IsExistsParam(heightParam)
            && opening.IsExistsParam(thicknessParam)) {

            (var frontDir, var upDir, var leftDir) = GetOrientationVectors(opening);
            double width = opening.GetParamValue<double>(widthParam);
            double height = opening.GetParamValue<double>(heightParam);
            double thickness = opening.GetParamValue<double>(thicknessParam);
            var loopLeftUpperCorner = ((LocationPoint) opening.Location).Point
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
    private Solid GetFloorRoundSolid(FamilyInstance opening, SharedParam diameterParam, SharedParam thicknessParam) {
        if(opening.IsExistsParam(diameterParam)
            && opening.IsExistsParam(thicknessParam)) {

            (var frontDir, var upDir, var leftDir) = GetOrientationVectors(opening);
            double diameter = opening.GetParamValue<double>(diameterParam);
            double thickness = opening.GetParamValue<double>(thicknessParam);

            var circleOrigin = ((LocationPoint) opening.Location).Point;
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
