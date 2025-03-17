using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitPylonDocumentation.Models {
    public class ViewCornersAnalyzer {
        private readonly View _view;
        public ViewCornersAnalyzer(View view) {
            _view = view;
        }

        public XYZ CornerRightTopGlobal { get; set; }
        public XYZ CornerLeftBottomGlobal { get; set; }
        public XYZ CornerLeftTopGlobal { get; set; }
        public XYZ CornerRightBottomGlobal { get; set; }
        public XYZ QuadrantTopGlobal { get; private set; }
        public XYZ QuadrantRightGlobal { get; private set; }
        public XYZ QuadrantBottomGlobal { get; private set; }
        public XYZ QuadrantLeftGlobal { get; private set; }

        public void AnalyzeCorners() {
            // Получаем CropBox вида
            BoundingBoxXYZ cropBox = _view.CropBox;

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
            CornerRightTopGlobal = transform.OfPoint(cornerRightTopLocal);
            CornerLeftBottomGlobal = transform.OfPoint(cornerLeftBottomLocal);
            CornerLeftTopGlobal = transform.OfPoint(cornerLeftTopLocal);
            CornerRightBottomGlobal = transform.OfPoint(cornerRightBottomLocal);

            QuadrantTopGlobal = transform.OfPoint(quadrantTopLocal);
            QuadrantRightGlobal = transform.OfPoint(quadrantRightLocal);
            QuadrantBottomGlobal = transform.OfPoint(quadrantBottomLocal);
            QuadrantLeftGlobal = transform.OfPoint(quadrantLeftLocal);
        }


        public Element GetElementByDirection(List<Element> elements, DirectionType directionType) {
            XYZ pointForCompare = default;
            switch(directionType) {
                case DirectionType.Top:
                    pointForCompare = QuadrantTopGlobal;
                    break;
                case DirectionType.Right:
                    pointForCompare = QuadrantRightGlobal;
                    break;
                case DirectionType.Bottom:
                    pointForCompare = QuadrantBottomGlobal;
                    break;
                case DirectionType.Left:
                    pointForCompare = QuadrantLeftGlobal;
                    break;

                case DirectionType.RightTop:
                    pointForCompare = CornerRightTopGlobal;
                    break;
                case DirectionType.RightBottom:
                    pointForCompare = CornerRightBottomGlobal;
                    break;
                case DirectionType.LeftBottom:
                    pointForCompare = CornerLeftBottomGlobal;
                    break;
                case DirectionType.LeftTop:
                    pointForCompare = CornerLeftTopGlobal;
                    break;
                default:
                    return null;
            }

            // Находим элемент, ближайший к этой точке
            Element neededElement = null;
            double minDistance = double.MaxValue;
            foreach(Element element in elements) {
                // Получаем середину BoundingBox объекта
                BoundingBoxXYZ boundingBox = element.get_BoundingBox(_view);
                var midleOfBoundingBox = (boundingBox.Max + boundingBox.Min) / 2;

                // Получаем и сравниваем расстояние от точки для сравнения до центра BoundingBox
                double distance = midleOfBoundingBox.DistanceTo(pointForCompare);
                if(distance < minDistance) {
                    minDistance = distance;
                    neededElement = element;
                }

                //if(element.Location is LocationPoint locationPoint) {
                //    XYZ elementPoint = locationPoint.Point;

                //}
            }
            return neededElement;
        }


        public XYZ GetPointByDirection(View view, Element element, DirectionType cornerType, double xOffsetCoef, double yOffsetCoef) {
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

            // Получаем точку для размещения марки
            BoundingBoxXYZ boundingBox = element.get_BoundingBox(view);
            var midleOfBoundingBox = (boundingBox.Max + boundingBox.Min) / 2;
            return midleOfBoundingBox + xOffset + yOffset;
        }
    }
}
