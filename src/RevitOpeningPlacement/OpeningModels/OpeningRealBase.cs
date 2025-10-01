using System;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.Revit.Geometry;

using RevitClashDetective.Models.Extensions;

using RevitOpeningPlacement.Models;
using RevitOpeningPlacement.Models.Extensions;
using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.RealOpeningArPlacement;
using RevitOpeningPlacement.Models.RealOpeningKrPlacement;

namespace RevitOpeningPlacement.OpeningModels;
/// <summary>
/// Базовый класс полого экземпляра семейства
/// </summary>
internal abstract class OpeningRealBase : IOpeningReal {
    /// <summary>
    /// Экземпляр семейства чистового отверстия
    /// </summary>
    private protected readonly FamilyInstance _familyInstance;

    /// <summary>
    /// Закэшированный BBox
    /// </summary>
    private protected BoundingBoxXYZ _boundingBox;

    /// <summary>
    /// Закэшированный солид в координатах файла
    /// </summary>
    private Solid _solid;

    /// <summary>
    /// Базовый конструктор, устанавливающий <see cref="_familyInstance"/>, <see cref="_boundingBox"/>
    /// </summary>
    /// <param name="openingReal">Экземпляр семейства проема в стене или перекрытии</param>
    /// <exception cref="ArgumentNullException">Исключение, если обязательный параметр является null</exception>
    /// <exception cref="ArgumentException">Исключение, если экземпляр семейства не имеет хоста</exception>
    protected OpeningRealBase(FamilyInstance openingReal) {
        if(openingReal is null) { throw new ArgumentNullException(nameof(openingReal)); }
        if(openingReal.Host is null) {
            throw new ArgumentException(
                $"{nameof(openingReal)} с Id {openingReal.Id} не содержит ссылки на хост элемент");
        }
        _familyInstance = openingReal;
        Id = _familyInstance.Id;

        SetTransformedBBoxXYZ();
    }

    /// <summary>
    /// Id экземпляра семейства чистового проема
    /// </summary>
    public ElementId Id { get; }


    public abstract Solid GetSolid();

    public abstract BoundingBoxXYZ GetTransformedBBoxXYZ();


    /// <summary>
    /// Возвращает хост экземпляра семейства отверстия
    /// </summary>
    public Element GetHost() {
        return _familyInstance.Host;
    }

    public FamilyInstance GetFamilyInstance() {
        return _familyInstance;
    }


    /// <summary>
    /// Возвращает значение параметра, или пустую строку, если параметра у семейства нет. 
    /// Значения параметров с типом данных "длина" конвертируются в мм и округляются до 1 мм.
    /// </summary>
    /// <exception cref="ArgumentNullException">Исключение, если обязательный параметр null</exception>
    private protected string GetFamilyInstanceStringParamValueOrEmpty(string paramName) {
        if(_familyInstance is null) {
            throw new ArgumentNullException(nameof(_familyInstance));
        }
        string value = string.Empty;
        if(_familyInstance.GetParameters(paramName).FirstOrDefault(item => item.IsShared) != null) {
#if REVIT_2022_OR_GREATER
            if(_familyInstance.GetSharedParam(paramName).Definition.GetDataType() == SpecTypeId.Length) {
                return Math.Round(
                    UnitUtils.ConvertFromInternalUnits(
                        GetFamilyInstanceDoubleParamValueOrZero(paramName), UnitTypeId.Millimeters))
                    .ToString();
            }
#elif REVIT_2021
            if(_familyInstance.GetSharedParam(paramName).Definition.ParameterType == ParameterType.Length) {
                return Math.Round(
                    UnitUtils.ConvertFromInternalUnits(
                        GetFamilyInstanceDoubleParamValueOrZero(paramName), UnitTypeId.Millimeters))
                    .ToString();
            }
#else
            if(_familyInstance.GetSharedParam(paramName).Definition.UnitType == UnitType.UT_Length) {
                return Math.Round(
                    UnitUtils.ConvertFromInternalUnits(
                        GetFamilyInstanceDoubleParamValueOrZero(paramName), DisplayUnitType.DUT_MILLIMETERS))
                    .ToString();
            }
#endif
            object paramValue = _familyInstance.GetParamValue(paramName);
            if(paramValue is not null) {
                value = paramValue is double doubleValue ? Math.Round(doubleValue).ToString() : paramValue.ToString();
            }
        }
        return value;
    }


    /// <summary>
    /// Возвращает солид отверстия в координатах собственного файла
    /// </summary>
    private protected Solid GetOpeningSolid() {
        if(_solid != null) {
            return _solid;
        }
        Solid solid;
        try {
            solid = GetSolidByFamily(_familyInstance.Symbol.FamilyName);
        } catch(Exception ex) when(
        ex is NullReferenceException
        or ArgumentNullException
        or ArgumentException
        or InvalidOperationException
        or Autodesk.Revit.Exceptions.ApplicationException) {
            solid = GetSolidByCut();
        }
        _solid = solid;
        return _solid;
    }

    /// <summary>
    /// Возвращает значение double параметра экземпляра семейства задания на отверстие в единицах ревита, 
    /// или 0, если параметр отсутствует
    /// </summary>
    /// <param name="paramName">Название параметра</param>
    private protected double GetFamilyInstanceDoubleParamValueOrZero(string paramName) {
        return _familyInstance.GetParameters(paramName).FirstOrDefault(item => item.IsShared) != null
            ? _familyInstance.GetSharedParamValue<double>(paramName)
            : 0;
    }


    private Solid CreateRawSolid(BoundingBoxXYZ bbox) {
        return bbox.CreateSolid();
    }

    /// <summary>
    /// Устанавливает значение полю <see cref="_boundingBox"/>
    /// </summary>
    private void SetTransformedBBoxXYZ() {
        _boundingBox = _familyInstance.GetBoundingBox();
    }

    /// <summary>
    /// Создает солид по форме, которую вырезает текущее отверстие из хоста.
    /// </summary>
    private Solid GetSolidByCut() {
        var box = _familyInstance.GetBoundingBox();
        var openingLocation = (box.Max + box.Min) / 2;
        var hostElement = GetHost();
        var hostSolidCut = hostElement.GetSolid();
        try {
            var hostSolidOriginal = (hostElement as HostObject).GetHostElementOriginalSolid();
            var openings = SolidUtils.SplitVolumes(
                BooleanOperationsUtils.ExecuteBooleanOperation(
                    hostSolidOriginal,
                    hostSolidCut,
                    BooleanOperationsType.Difference));
            var thisOpeningSolid = openings.OrderBy(
                solidOpening => (solidOpening.ComputeCentroid() - openingLocation).GetLength()).FirstOrDefault();
            return thisOpeningSolid ?? CreateRawSolid(box);
        } catch(Autodesk.Revit.Exceptions.InvalidOperationException) {
            return CreateRawSolid(box);
        } catch(Autodesk.Revit.Exceptions.ArgumentNullException) {
            return CreateRawSolid(box);
        } catch(Autodesk.Revit.Exceptions.ArgumentOutOfRangeException) {
            return CreateRawSolid(box);
        } catch(Autodesk.Revit.Exceptions.ArgumentException) {
            return CreateRawSolid(box);
        } catch(InvalidOperationException) {
            return CreateRawSolid(box);
        } catch(ArgumentException) {
            return CreateRawSolid(box);
        }
    }

    /// <summary>
    /// Находит солид экземпляра семейства отверстия исходя из координат точки вставки, 
    /// формы семейства и значений параметров.
    /// </summary>
    /// <param name="familyName">Название семейства.</param>
    /// <returns>Солид экземпляра семейства.</returns>
    private Solid GetSolidByFamily(string familyName) {
        if(familyName is null) {
            throw new ArgumentNullException(nameof(familyName));
        }
        if(string.IsNullOrWhiteSpace(familyName)) {
            throw new ArgumentException(nameof(familyName));
        }
        if(familyName.Equals(RevitRepository.OpeningRealArFamilyName[OpeningType.WallRectangle])) {
            return GetWallRectangleSolid(
                RealOpeningArPlacer.RealOpeningArWidth,
                RealOpeningArPlacer.RealOpeningArHeight,
                RealOpeningArPlacer.RealOpeningArThickness);
        } else if(familyName.Equals(RevitRepository.OpeningRealArFamilyName[OpeningType.WallRound])) {
            return GetWallRoundSolid(
                RealOpeningArPlacer.RealOpeningArDiameter,
                RealOpeningArPlacer.RealOpeningArThickness);
        } else if(familyName.Equals(RevitRepository.OpeningRealArFamilyName[OpeningType.FloorRectangle])) {
            return GetFloorRectangleSolid(
                RealOpeningArPlacer.RealOpeningArWidth,
                RealOpeningArPlacer.RealOpeningArHeight,
                RealOpeningArPlacer.RealOpeningArThickness);
        } else if(familyName.Equals(RevitRepository.OpeningRealArFamilyName[OpeningType.FloorRound])) {
            return GetFloorRoundSolid(
                RealOpeningArPlacer.RealOpeningArDiameter,
                RealOpeningArPlacer.RealOpeningArThickness);
        } else if(familyName.Equals(RevitRepository.OpeningRealKrFamilyName[OpeningType.WallRectangle])) {
            return GetWallRectangleSolid(
                RealOpeningKrPlacer.RealOpeningKrInWallWidth,
                RealOpeningKrPlacer.RealOpeningKrInWallHeight,
                RealOpeningKrPlacer.RealOpeningKrThickness);
        } else {
            return familyName.Equals(RevitRepository.OpeningRealKrFamilyName[OpeningType.WallRound])
                ? GetWallRoundSolid(
                            RealOpeningKrPlacer.RealOpeningKrDiameter,
                            RealOpeningKrPlacer.RealOpeningKrThickness)
                : familyName.Equals(RevitRepository.OpeningRealKrFamilyName[OpeningType.FloorRectangle])
                            ? GetFloorRectangleSolid(
                                            RealOpeningKrPlacer.RealOpeningKrInFloorWidth,
                                            RealOpeningKrPlacer.RealOpeningKrInFloorHeight,
                                            RealOpeningKrPlacer.RealOpeningKrThickness)
                            : throw new InvalidOperationException();
        }
    }

    /// <summary>
    /// Находит солид прямоугольного отверстия в стене. 
    /// Точка вставки экземпляра семейства - центр нижней грани параллелепипеда отверстия.
    /// </summary>
    /// <returns>Параллелепипед, построенный в соответствии с семейством.</returns>
    private Solid GetWallRectangleSolid(string widthName, string heightName, string thicknessName) {
        if(_familyInstance.IsExistsSharedParam(widthName)
            && _familyInstance.IsExistsSharedParam(heightName)
            && _familyInstance.IsExistsSharedParam(thicknessName)) {

            (var frontNormal, var upDir, var leftDir) = GetOrientationVectors();
            double width = _familyInstance.GetSharedParamValue<double>(widthName);
            double height = _familyInstance.GetSharedParamValue<double>(heightName);
            double thickness = _familyInstance.GetSharedParamValue<double>(thicknessName);
            var loopLeftUpperCorner = (_familyInstance.Location as LocationPoint).Point
                - frontNormal * thickness / 2
                + leftDir * width / 2
                + upDir * height;
            var loopRightUpperCorner = loopLeftUpperCorner - leftDir * width;
            var loopRightBottomCorner = loopRightUpperCorner - upDir * height;
            var loopLeftBottomCorner = loopRightBottomCorner + leftDir * width;

            var rectangle = CurveLoop.Create(new Line[] {
                Line.CreateBound(loopLeftUpperCorner, loopRightUpperCorner),
                Line.CreateBound(loopRightUpperCorner, loopRightBottomCorner),
                Line.CreateBound(loopRightBottomCorner, loopLeftBottomCorner),
                Line.CreateBound(loopLeftBottomCorner, loopLeftUpperCorner)
            });
            return GeometryCreationUtilities.CreateExtrusionGeometry(
                new CurveLoop[] { rectangle }, frontNormal, thickness);
        } else {
            throw new InvalidOperationException();
        }
    }

    /// <summary>
    /// Находит солид круглого отверстия в стене.
    /// Точка вставки экземпляра семейства - геометрический центр цилиндра.
    /// </summary>
    /// <returns>Горизонтальный цилиндр, построенный в соответствии с семейством.</returns>
    private Solid GetWallRoundSolid(string diameterName, string thicknessName) {
        if(_familyInstance.IsExistsSharedParam(diameterName)
            && _familyInstance.IsExistsSharedParam(thicknessName)) {

            (var frontNormal, var upDir, var leftDir) = GetOrientationVectors();
            double diameter = _familyInstance.GetSharedParamValue<double>(diameterName);
            double thickness = _familyInstance.GetSharedParamValue<double>(thicknessName);

            var circleOrigin = (_familyInstance.Location as LocationPoint).Point - frontNormal * thickness / 2;
            var leftPoint = circleOrigin + leftDir * diameter / 2;
            var topPoint = circleOrigin + upDir * diameter / 2;
            var rightPoint = circleOrigin - leftDir * diameter / 2;
            var bottomPoint = circleOrigin - upDir * diameter / 2;

            var circle = CurveLoop.Create(new Arc[] {
                Arc.Create(leftPoint, rightPoint, topPoint),
                Arc.Create(rightPoint, leftPoint, bottomPoint)
            });
            return GeometryCreationUtilities.CreateExtrusionGeometry(
                new CurveLoop[] { circle }, frontNormal, thickness);
        } else {
            throw new InvalidOperationException();
        }
    }

    /// <summary>
    /// Находит солид прямоугольного отверстия в перекрытии.
    /// Точка вставки экземпляра семейства - центр верхней грани параллелепипеда.
    /// </summary>
    /// <returns>Параллелепипед, построенный в соответствии с семейством.</returns>
    private Solid GetFloorRectangleSolid(string widthName, string heightName, string thicknessName) {
        if(_familyInstance.IsExistsSharedParam(widthName)
            && _familyInstance.IsExistsSharedParam(heightName)
            && _familyInstance.IsExistsSharedParam(thicknessName)) {

            (var frontDir, var upDir, var leftDir) = GetOrientationVectors();
            double width = _familyInstance.GetSharedParamValue<double>(widthName);
            double height = _familyInstance.GetSharedParamValue<double>(heightName);
            double thickness = _familyInstance.GetSharedParamValue<double>(thicknessName);
            var loopLeftUpperCorner = (_familyInstance.Location as LocationPoint).Point
                + leftDir * width / 2
                + frontDir * height / 2;
            var loopRightUpperCorner = loopLeftUpperCorner - leftDir * width;
            var loopRightBottomCorner = loopRightUpperCorner - frontDir * height;
            var loopLeftBottomCorner = loopRightBottomCorner + leftDir * width;

            var rectangle = CurveLoop.Create(new Line[] {
                Line.CreateBound(loopLeftUpperCorner, loopRightUpperCorner),
                Line.CreateBound(loopRightUpperCorner, loopRightBottomCorner),
                Line.CreateBound(loopRightBottomCorner, loopLeftBottomCorner),
                Line.CreateBound(loopLeftBottomCorner, loopLeftUpperCorner)
            });
            return GeometryCreationUtilities.CreateExtrusionGeometry(
                new CurveLoop[] { rectangle }, -upDir, thickness);
        } else {
            throw new InvalidOperationException();
        }
    }

    /// <summary>
    /// Находит солид круглого отверстия в перекрытии.
    /// Точка вставки экземпляра семейства - центр верхней грани цилиндра.
    /// </summary>
    /// <returns>Вертикальный цилиндр, построенный в соответствии с семейством.</returns>
    private Solid GetFloorRoundSolid(string diameterName, string thicknessName) {
        if(_familyInstance.IsExistsSharedParam(diameterName)
            && _familyInstance.IsExistsSharedParam(thicknessName)) {

            (var frontDir, var upDir, var leftDir) = GetOrientationVectors();
            double diameter = _familyInstance.GetSharedParamValue<double>(diameterName);
            double thickness = _familyInstance.GetSharedParamValue<double>(thicknessName);

            var circleOrigin = (_familyInstance.Location as LocationPoint).Point;
            var leftPoint = circleOrigin + leftDir * diameter / 2;
            var topPoint = circleOrigin + frontDir * diameter / 2;
            var rightPoint = circleOrigin - leftDir * diameter / 2;
            var bottomPoint = circleOrigin - frontDir * diameter / 2;

            var circle = CurveLoop.Create(new Arc[] {
                Arc.Create(leftPoint, rightPoint, topPoint),
                Arc.Create(rightPoint, leftPoint, bottomPoint)
            });
            return GeometryCreationUtilities.CreateExtrusionGeometry(
                new CurveLoop[] { circle }, -upDir, thickness);
        } else {
            throw new InvalidOperationException();
        }
    }

    /// <summary>
    /// Возвращает нормализованные векторы ориентации экземпляра семейства.
    /// </summary>
    /// <returns>Вперед, вверх, влево.</returns>
    private (XYZ frontDir, XYZ upDir, XYZ leftDir) GetOrientationVectors() {
        var frontNormal = _familyInstance.FacingOrientation;
        var upDir = XYZ.BasisZ;
        var leftDir = upDir.CrossProduct(frontNormal).Normalize();
        return (frontNormal, upDir, leftDir);
    }
}
