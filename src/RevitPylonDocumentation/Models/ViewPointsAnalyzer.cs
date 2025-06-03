using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitPylonDocumentation.Models.PylonSheetNView;

namespace RevitPylonDocumentation.Models {
    public class ViewPointsAnalyzer {
        private readonly PylonView _viewOfPylon;
        private bool _checked;
        private XYZ _cornerRightTopGlobal;
        private XYZ _cornerLeftBottomGlobal;
        private XYZ _cornerLeftTopGlobal;
        private XYZ _cornerRightBottomGlobal;
        private XYZ _quadrantTopGlobal;
        private XYZ _quadrantRightGlobal;
        private XYZ _quadrantBottomGlobal;
        private XYZ _quadrantLeftGlobal;

        public ViewPointsAnalyzer(PylonView pylonView) {
            _viewOfPylon = pylonView;
            _checked = false;
        }

        private void AnalyzeViewControlPoints() {
            // Получаем CropBox вида
            BoundingBoxXYZ cropBox = _viewOfPylon.ViewElement.CropBox;

            // Получаем минимальную точку подрезки (нижний левый угол) в локальной системе координат вида
            XYZ cornerRightTopLocal = cropBox.Max;
            XYZ cornerLeftBottomLocal = cropBox.Min;
            XYZ cornerLeftTopLocal = new XYZ(cornerLeftBottomLocal.X, cornerRightTopLocal.Y, 0);
            XYZ cornerRightBottomLocal = new XYZ(cornerRightTopLocal.X, cornerLeftBottomLocal.Y, 0);

            XYZ quadrantTopLocal = (cornerLeftTopLocal + cornerRightTopLocal) / 2;
            XYZ quadrantRightLocal = (cornerRightTopLocal + cornerRightBottomLocal) / 2;
            XYZ quadrantBottomLocal = (cornerLeftBottomLocal + cornerRightBottomLocal) / 2;
            XYZ quadrantLeftLocal = (cornerLeftBottomLocal + cornerLeftTopLocal) / 2;

            // Преобразуем точку в глобальные координаты
            Transform transform = cropBox.Transform;  // Преобразование из локальной системы вида в глобальную
            _cornerRightTopGlobal = transform.OfPoint(cornerRightTopLocal);
            _cornerLeftBottomGlobal = transform.OfPoint(cornerLeftBottomLocal);
            _cornerLeftTopGlobal = transform.OfPoint(cornerLeftTopLocal);
            _cornerRightBottomGlobal = transform.OfPoint(cornerRightBottomLocal);

            _quadrantTopGlobal = transform.OfPoint(quadrantTopLocal);
            _quadrantRightGlobal = transform.OfPoint(quadrantRightLocal);
            _quadrantBottomGlobal = transform.OfPoint(quadrantBottomLocal);
            _quadrantLeftGlobal = transform.OfPoint(quadrantLeftLocal);

            _checked = true;
        }


        public Element GetElementByDirection(List<Element> elements, DirectionType directionType, bool getByBoundingBox) {
            if(!_checked) {
                AnalyzeViewControlPoints();
            }

            XYZ pointForCompare = default;
            switch(directionType) {
                case DirectionType.Top:
                    pointForCompare = _quadrantTopGlobal;
                    break;
                case DirectionType.Right:
                    pointForCompare = _quadrantRightGlobal;
                    break;
                case DirectionType.Bottom:
                    pointForCompare = _quadrantBottomGlobal;
                    break;
                case DirectionType.Left:
                    pointForCompare = _quadrantLeftGlobal;
                    break;

                case DirectionType.RightTop:
                    pointForCompare = _cornerRightTopGlobal;
                    break;
                case DirectionType.RightBottom:
                    pointForCompare = _cornerRightBottomGlobal;
                    break;
                case DirectionType.LeftBottom:
                    pointForCompare = _cornerLeftBottomGlobal;
                    break;
                case DirectionType.LeftTop:
                    pointForCompare = _cornerLeftTopGlobal;
                    break;
                default:
                    return null;
            }

            // Находим элемент, ближайший к этой точке
            Element neededElement = null;
            double minDistance = double.MaxValue;
            foreach(Element element in elements) {
                if(getByBoundingBox) {
                    // Получаем середину BoundingBox объекта
                    BoundingBoxXYZ boundingBox = element.get_BoundingBox(_viewOfPylon.ViewElement);
                    var midleOfBoundingBox = (boundingBox.Max + boundingBox.Min) / 2;

                    // Получаем и сравниваем расстояние от точки для сравнения до центра BoundingBox
                    double distance = midleOfBoundingBox.DistanceTo(pointForCompare);
                    if(distance < minDistance) {
                        minDistance = distance;
                        neededElement = element;
                    }
                } else {
                    XYZ elementPoint = (element.Location as LocationPoint).Point;
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
                BoundingBoxXYZ boundingBox = element.get_BoundingBox(_viewOfPylon.ViewElement);
                var midleOfBoundingBox = (boundingBox.Max + boundingBox.Min) / 2;
                return midleOfBoundingBox + xOffset + yOffset;
            } else {
                XYZ elementPoint = (element.Location as LocationPoint).Point;
                return elementPoint + xOffset + yOffset;
            }
        }

        public XYZ GetPointByDirection(XYZ point, DirectionType directionType, int xOffsetCoef, double yOffsetCoef) {
            var offsets = GetOffsetsByDirection(directionType, xOffsetCoef, yOffsetCoef);
            var xOffset = offsets.xOffset;
            var yOffset = offsets.yOffset;

            return point + xOffset + yOffset;
        }
    }
}
