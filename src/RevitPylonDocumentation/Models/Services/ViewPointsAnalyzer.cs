using System;
using System.Collections.Generic;
using System.Windows.Media;

using Autodesk.Revit.DB;

using RevitPylonDocumentation.Models.PylonSheetNView;

namespace RevitPylonDocumentation.Models.Services;
public class ViewPointsAnalyzer {
    private readonly PylonView _viewOfPylon;
    private bool _checked;
    private XYZ _cornerRightTopGlobalPlus;
    private XYZ _cornerLeftBottomGlobalPlus;
    private XYZ _cornerLeftTopGlobalPlus;
    private XYZ _cornerRightBottomGlobalPlus;
    private XYZ _center;
    private XYZ _quadrantTopGlobalPlus;
    private XYZ _quadrantRightGlobalPlus;
    private XYZ _quadrantBottomGlobalPlus;
    private XYZ _quadrantLeftGlobalPlus;

    public ViewPointsAnalyzer(PylonView pylonView) {
        _viewOfPylon = pylonView;
        _checked = false;
    }

    private void AnalyzeViewControlPoints() {
        // Получаем CropBox вида
        var cropBox = _viewOfPylon.ViewElement.CropBox;

        // Получаем минимальную точку подрезки (нижний левый угол) в локальной системе координат вида
        var cornerRightTopLocal = cropBox.Max;
        var cornerLeftBottomLocal = cropBox.Min;
        var cornerLeftTopLocal = new XYZ(cornerLeftBottomLocal.X, cornerRightTopLocal.Y, 0);
        var cornerRightBottomLocal = new XYZ(cornerRightTopLocal.X, cornerLeftBottomLocal.Y, 0);

        var center = (cornerRightTopLocal + cornerLeftBottomLocal) / 2;

        var quadrantTopLocal = (cornerLeftTopLocal + cornerRightTopLocal) / 2;
        var quadrantRightLocal = (cornerRightTopLocal + cornerRightBottomLocal) / 2;
        var quadrantBottomLocal = (cornerLeftBottomLocal + cornerRightBottomLocal) / 2;
        var quadrantLeftLocal = (cornerLeftBottomLocal + cornerLeftTopLocal) / 2;

        // Преобразуем точку в глобальные координаты
        var transform = cropBox.Transform;  // Преобразование из локальной системы вида в глобальную
        _center = ProjectPointToViewFront(transform.OfPoint(center));

        _cornerRightTopGlobalPlus = ProjectPointToViewFront(transform.OfPoint(cornerRightTopLocal));
        _cornerLeftBottomGlobalPlus = ProjectPointToViewFront(transform.OfPoint(cornerLeftBottomLocal));
        _cornerLeftTopGlobalPlus = ProjectPointToViewFront(transform.OfPoint(cornerLeftTopLocal));
        _cornerRightBottomGlobalPlus = ProjectPointToViewFront(transform.OfPoint(cornerRightBottomLocal));

        _quadrantTopGlobalPlus = ProjectPointToViewFront(transform.OfPoint(quadrantTopLocal));
        _quadrantRightGlobalPlus = ProjectPointToViewFront(transform.OfPoint(quadrantRightLocal));
        _quadrantBottomGlobalPlus = ProjectPointToViewFront(transform.OfPoint(quadrantBottomLocal));
        _quadrantLeftGlobalPlus = ProjectPointToViewFront(transform.OfPoint(quadrantLeftLocal));

        // Отдаляем точку от центра, чтобы аналитика работала точнее
        _cornerRightTopGlobalPlus = _cornerRightTopGlobalPlus * 2 - _center;
        _cornerLeftBottomGlobalPlus = _cornerLeftBottomGlobalPlus * 2 - _center;
        _cornerLeftTopGlobalPlus = _cornerLeftTopGlobalPlus * 2 - _center;
        _cornerRightBottomGlobalPlus = _cornerRightBottomGlobalPlus * 2 - _center;

        _quadrantTopGlobalPlus = _quadrantTopGlobalPlus * 2 - _center;
        _quadrantRightGlobalPlus = _quadrantRightGlobalPlus * 2 - _center;
        _quadrantBottomGlobalPlus = _quadrantBottomGlobalPlus * 2 - _center;
        _quadrantLeftGlobalPlus = _quadrantLeftGlobalPlus * 2 - _center;

        _checked = true;
    }

    public Element GetElementByDirection(List<Element> elements, DirectionType directionType, bool getByBoundingBox) {
        if(!_checked) {
            AnalyzeViewControlPoints();
        }

        XYZ pointForCompare;
        switch(directionType) {
            case DirectionType.Top:
                pointForCompare = _quadrantTopGlobalPlus;
                break;
            case DirectionType.Right:
                pointForCompare = _quadrantRightGlobalPlus;
                break;
            case DirectionType.Bottom:
                pointForCompare = _quadrantBottomGlobalPlus;
                break;
            case DirectionType.Left:
                pointForCompare = _quadrantLeftGlobalPlus;
                break;

            case DirectionType.RightTop:
                pointForCompare = _cornerRightTopGlobalPlus;
                break;
            case DirectionType.RightBottom:
                pointForCompare = _cornerRightBottomGlobalPlus;
                break;
            case DirectionType.LeftBottom:
                pointForCompare = _cornerLeftBottomGlobalPlus;
                break;
            case DirectionType.LeftTop:
                pointForCompare = _cornerLeftTopGlobalPlus;
                break;
            default:
                return null;
        }

        // Находим элемент, ближайший к этой точке
        Element neededElement = null;
        double minDistance = double.MaxValue;
        foreach(var element in elements) {
            if(getByBoundingBox) {
                // Получаем середину BoundingBox объекта
                var boundingBox = element.get_BoundingBox(_viewOfPylon.ViewElement);
                var midleOfBoundingBox = ProjectPointToViewFront((boundingBox.Max + boundingBox.Min) / 2);

                // Получаем и сравниваем расстояние от точки для сравнения до центра BoundingBox
                double distance = midleOfBoundingBox.DistanceTo(pointForCompare);
                if(distance < minDistance) {
                    minDistance = distance;
                    neededElement = element;
                }
            } else {
                var elementPoint = ProjectPointToViewFront((element.Location as LocationPoint).Point);
                double distance = elementPoint.DistanceTo(pointForCompare);
                if(distance < minDistance) {
                    minDistance = distance;
                    neededElement = element;
                }
            }
        }
        return neededElement;
    }


    private (XYZ xOffset, XYZ yOffset) GetOffsetsByDirection(DirectionType cornerType,
                                                             double xOffsetCoef, double yOffsetCoef) {
        var view = _viewOfPylon.ViewElement;
        XYZ xOffset = default;
        XYZ yOffset = default;
        // Получаем вектора смещения в зависимости от выбранного направления
        switch(cornerType) {
            case DirectionType.Top:
                xOffset = new XYZ();
                yOffset = view.UpDirection.Normalize();
                break;
            case DirectionType.Right:
                xOffset = view.RightDirection.Normalize();
                yOffset = new XYZ();
                break;
            case DirectionType.Bottom:
                xOffset = new XYZ();
                yOffset = view.UpDirection.Normalize().Negate();
                break;
            case DirectionType.Left:
                xOffset = view.RightDirection.Normalize().Negate();
                yOffset = new XYZ();
                break;

            case DirectionType.RightTop:
                xOffset = view.RightDirection.Normalize();
                yOffset = view.UpDirection.Normalize();
                break;
            case DirectionType.RightBottom:
                xOffset = view.RightDirection.Normalize();
                yOffset = view.UpDirection.Normalize().Negate();
                break;
            case DirectionType.LeftBottom:
                xOffset = view.RightDirection.Normalize().Negate();
                yOffset = view.UpDirection.Normalize().Negate();
                break;
            case DirectionType.LeftTop:
                xOffset = view.RightDirection.Normalize().Negate();
                yOffset = view.UpDirection.Normalize();
                break;
            default:
                break;
        }
        xOffset = xOffset.Multiply(xOffsetCoef);
        yOffset = yOffset.Multiply(yOffsetCoef);
        return (xOffset, yOffset);
    }


    public XYZ GetPointByDirection(Element element, DirectionType directionType,
                                   double xOffsetCoef, double yOffsetCoef, bool getByBoundingBox) {
        var offsets = GetOffsetsByDirection(directionType, xOffsetCoef, yOffsetCoef);
        var xOffset = offsets.xOffset;
        var yOffset = offsets.yOffset;

        // Получаем точку для размещения марки
        if(getByBoundingBox) {
            var boundingBox = element.get_BoundingBox(_viewOfPylon.ViewElement);
            var midleOfBoundingBox = (boundingBox.Max + boundingBox.Min) / 2;
            return midleOfBoundingBox + xOffset + yOffset;
        } else {
            var elementPoint = (element.Location as LocationPoint).Point;
            return elementPoint + xOffset + yOffset;
        }
    }

    public XYZ GetPointByDirection(XYZ point, DirectionType directionType, double xOffsetCoef, double yOffsetCoef) {
        var offsets = GetOffsetsByDirection(directionType, xOffsetCoef, yOffsetCoef);
        var xOffset = offsets.xOffset;
        var yOffset = offsets.yOffset;

        return point + xOffset + yOffset;
    }

    /// <summary>
    /// Возвращает точку пилона в зависимости от направления в зависимости от ориентации вида
    /// </summary>
    /// <param name="directionType">Направление, в котором находит интересующий угол пилона в плане по виду</param>
    internal XYZ GetPylonPointByDirection(PylonSheetInfo sheetInfo, DirectionType directionType) {
        var info = sheetInfo.ElemsInfo;
        return GetPointByDirection(info.HostOrigin, directionType, info.HostLength / 2, info.HostWidth / 2);
    }

    /// <summary>
    /// Метод возвращает точку элемента в системе координат вида
    /// </summary>
    /// <param name="getByBoundingBox">Если да, то точка - центр BoundingBox, нет - точка вставки элемента</param>
    public XYZ GetTransformedPoint(Element element, bool getByBoundingBox) {
        var transform = _viewOfPylon.ViewElement.CropBox.Transform;
        var inverseTransform = transform.Inverse;

        XYZ elementPoint;
        if(getByBoundingBox) {
            var boundingBox = element.get_BoundingBox(_viewOfPylon.ViewElement);
            elementPoint = (boundingBox.Min + boundingBox.Max) / 2;
        } else {
            elementPoint = (element.Location as LocationPoint).Point;
        }
        return inverseTransform.OfPoint(elementPoint);
    }

    /// <summary>
    /// Метод возвращает точку в системе координат вида
    /// </summary>
    public XYZ GetTransformedPoint(XYZ point) {
        var transform = _viewOfPylon.ViewElement.CropBox.Transform;
        var inverseTransform = transform.Inverse;
        return inverseTransform.OfPoint(point);
    }

    /// <summary>
    /// Возвращает точку спроецированную на плоскость вида
    /// </summary>
    public XYZ ProjectPointToViewFront(XYZ point) {
        var origin = _viewOfPylon.ViewElement.Origin;
        var normal = _viewOfPylon.ViewElement.ViewDirection.Normalize();

        // Вычисляем вектор от точки на плоскости к целевой точке
        var vector = point - origin;

        // Находим расстояние вдоль нормали (скалярное произведение)
        double distance = normal.DotProduct(vector);

        // Проецируем точку на плоскость
        return point - distance * normal;
    }

    /// <summary>
    /// Возвращает точку спроецированную на горизонтальную плоскость
    /// </summary>
    public XYZ ProjectPointToHorizontalPlane(XYZ planePoint, XYZ pointForProject) {
        var normal = XYZ.BasisZ;

        // Вычисляем вектор от точки на плоскости к целевой точке
        var vector = pointForProject - planePoint;

        // Находим расстояние вдоль нормали (скалярное произведение)
        double distance = normal.DotProduct(vector);

        // Проецируем точку на плоскость
        return pointForProject - distance * normal;
    }
}
