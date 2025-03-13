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


        public void AnalyzeCorners() {
            // Получаем CropBox вида
            BoundingBoxXYZ cropBox = _view.CropBox;

            // Получаем минимальную точку подрезки (нижний левый угол) в локальной системе координат вида
            XYZ cornerRightTopLocal = cropBox.Max;
            XYZ cornerLeftBottomLocal = cropBox.Min;
            XYZ cornerLeftTopLocal = new XYZ(cornerLeftBottomLocal.X, cornerRightTopLocal.Y, 0);
            XYZ cornerRightBottomLocal = new XYZ(cornerRightTopLocal.X, cornerLeftBottomLocal.Y, 0);


            // Преобразуем точку в глобальные координаты
            Transform transform = cropBox.Transform;  // Преобразование из локальной системы вида в глобальную
            CornerRightTopGlobal = transform.OfPoint(cornerRightTopLocal);
            CornerLeftBottomGlobal = transform.OfPoint(cornerLeftBottomLocal);
            CornerLeftTopGlobal = transform.OfPoint(cornerLeftTopLocal);
            CornerRightBottomGlobal = transform.OfPoint(cornerRightBottomLocal);
        }



        public Element GetElementByCorner(List<Element> elements, CornerType cornerType) {
            XYZ cornerPoint = default;
            switch(cornerType) {
                case CornerType.RightTop:
                    cornerPoint = CornerRightTopGlobal;
                    break;
                case CornerType.RightBottom:
                    cornerPoint = CornerRightBottomGlobal;
                    break;
                case CornerType.LeftBottom:
                    cornerPoint = CornerLeftBottomGlobal;
                    break;
                case CornerType.LeftTop:
                    cornerPoint = CornerLeftTopGlobal;
                    break;
                default:
                    return null;
            }

            // Находим элемент, ближайший к этой точке
            Element bottomLeftElement = null;
            double minDistance = double.MaxValue;
            foreach(Element element in elements) {
                if(element.Location is LocationPoint locationPoint) {
                    XYZ elementPoint = locationPoint.Point;
                    double distance = elementPoint.DistanceTo(cornerPoint);  // Расстояние до элемента

                    if(distance < minDistance) {
                        minDistance = distance;
                        bottomLeftElement = element;
                    }
                }
            }
            return bottomLeftElement;
        }


        public XYZ GetPointByCorner(View view, Element element, CornerType cornerType) {
            XYZ xOffset = new XYZ();
            XYZ yOffset = new XYZ();

            switch(cornerType) {
                case CornerType.RightTop:
                    xOffset = view.RightDirection.Normalize().Multiply(2);
                    yOffset = view.UpDirection.Normalize().Multiply(0.4);
                    break;
                case CornerType.RightBottom:
                    xOffset = view.RightDirection.Normalize().Multiply(2);
                    yOffset = view.UpDirection.Normalize().Negate().Multiply(0.4);
                    break;
                case CornerType.LeftBottom:
                    xOffset = view.RightDirection.Normalize().Negate().Multiply(1);
                    yOffset = view.UpDirection.Normalize().Negate().Multiply(0.4);
                    break;
                case CornerType.LeftTop:
                    xOffset = view.RightDirection.Normalize().Negate().Multiply(1);
                    yOffset = view.UpDirection.Normalize().Multiply(0.4);
                    break;
                default:
                    break;
            }

            // Получаем точку для размещения марки
            LocationPoint locationBottomRight = element.Location as LocationPoint;
            return locationBottomRight.Point + xOffset + yOffset;
        }
    }
}
