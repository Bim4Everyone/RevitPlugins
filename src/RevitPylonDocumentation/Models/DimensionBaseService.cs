using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

namespace RevitPylonDocumentation.Models {

    /// <summary>
    /// Сервис для получения элементов, на которых основывается размер - ссылках на опорные плоскости и линии размещения
    /// </summary>
    internal class DimensionBaseService {
        private readonly View _view;
        private readonly ParamValueService _paramValueService;
        private readonly XYZ _viewOrigin;
        private readonly XYZ _viewDirection;
        private readonly XYZ _viewUpDirection;
        private readonly XYZ _viewRightDirection;
        private readonly BoundingBoxXYZ _viewCropBox;

        public DimensionBaseService(View view, ParamValueService paramValueService) {
            _view = view;
            _paramValueService = paramValueService;

            _viewOrigin = _view.Origin;
            _viewDirection = _view.ViewDirection;
            _viewUpDirection = _view.UpDirection;
            _viewRightDirection = _view.RightDirection;
            _viewCropBox = _view.CropBox;
        }


        public Line GetDimensionLine(FamilyInstance rebar, DimensionOffsetType dimensionOffsetType,
                              double offsetCoefficient = 1) {
            // Задаем дефолтные точки на случай, если не сработает получение
            var pt1 = new XYZ(0, 0, 0);
            var pt2 = new XYZ(0, 100, 0);

            // Если взять краевую точку рамки подрезки вида, то она будет в локальных координатах вида
            // Для перевода в глобальные координаты получим объект Transform
            // Получаем начало локальной системы координат вида в глобальной системе координат
            var xUpDirectionRounded = Math.Round(_viewUpDirection.X);
            var yUpDirectionRounded = Math.Round(_viewUpDirection.Y);

            // Создаем матрицу трансформации
            Transform transform = Transform.Identity;
            transform.Origin = _viewOrigin;
            transform.BasisX = _viewRightDirection;
            transform.BasisY = _viewUpDirection;
            transform.BasisZ = _viewDirection;

            // Получаем правую верхнюю точку рамки подрезки вида в системе координат вида
            var cropBoxMax = _viewCropBox.Max;
            // Получаем левую нижнюю точку рамки подрезки вида в системе координат вида
            var cropBoxMin = _viewCropBox.Min;

            // Переводим их в глобальную систему координат
            XYZ cropBoxMaxGlobal = transform.OfPoint(cropBoxMax);
            XYZ cropBoxMinGlobal = transform.OfPoint(cropBoxMin);

            BoundingBoxXYZ bbox = rebar.get_BoundingBox(_view);
            switch(dimensionOffsetType) {
                case DimensionOffsetType.Top:
                    // Получаем единичный вектор вида направления вверх
                    var upDirectionNormalized = _viewUpDirection.Normalize();
                    // Получаем отступ для более корректного размещения размера относительно арматуры
                    var offsetTop = upDirectionNormalized.Multiply(offsetCoefficient);

                    // Получаем первую точку размерной линии по BoundingBox каркаса армирования + отступ
                    if(xUpDirectionRounded == -1 || (yUpDirectionRounded == -1)) {
                        pt1 = bbox.Min + offsetTop;
                    } else {
                        pt1 = bbox.Max + offsetTop;
                    }

                    // Если точка, куда нужно поставить размерную линию находится за рамкой подрезки,
                    // то ставим по рамки подрезки
                    if(xUpDirectionRounded.Equals(0.0) && yUpDirectionRounded.Equals(1.0)) {
                        if(pt1.Y > cropBoxMaxGlobal.Y) {
                            pt1 = new XYZ(pt1.X, cropBoxMaxGlobal.Y, pt1.Z);
                        }
                    }

                    if(xUpDirectionRounded.Equals(0.0) && yUpDirectionRounded.Equals(-1.0)) {
                        if(pt1.Y < cropBoxMaxGlobal.Y) {
                            pt1 = new XYZ(pt1.X, cropBoxMaxGlobal.Y, pt1.Z);
                        }
                    }

                    if(xUpDirectionRounded.Equals(1.0) && yUpDirectionRounded.Equals(0.0)) {
                        if(pt1.X > cropBoxMaxGlobal.X) {
                            pt1 = new XYZ(cropBoxMaxGlobal.X, pt1.Y, pt1.Z);
                        }
                    }

                    if(xUpDirectionRounded.Equals(-1.0) && yUpDirectionRounded.Equals(0.0)) {
                        if(pt1.X < cropBoxMaxGlobal.X) {
                            pt1 = new XYZ(cropBoxMaxGlobal.X, pt1.Y, pt1.Z);
                        }
                    }

                    pt2 = pt1 + _viewRightDirection;
                    break;
                case DimensionOffsetType.Bottom:
                    // Получаем единичный вектор вида направления вниз
                    var downDirectionNormalized = _viewUpDirection.Normalize().Negate();
                    // Получаем отступ для более корректного размещения размера относительно арматуры
                    var offsetBottom = downDirectionNormalized.Multiply(offsetCoefficient);

                    // Получаем первую точку размерной линии по BoundingBox каркаса армирования + отступ
                    if(xUpDirectionRounded == -1 || (yUpDirectionRounded == -1)) {
                        pt1 = bbox.Max + offsetBottom;
                    } else {
                        pt1 = bbox.Min + offsetBottom;
                    }

                    // Если точка, куда нужно поставить размерную линию находится за рамкой подрезки,
                    // то ставим по рамке подрезки
                    if(xUpDirectionRounded.Equals(0.0) && yUpDirectionRounded.Equals(1.0)) {
                        if(pt1.Y < cropBoxMinGlobal.Y) {
                            pt1 = new XYZ(pt1.X, cropBoxMinGlobal.Y, pt1.Z);
                        }
                    }

                    if(xUpDirectionRounded.Equals(0.0) && yUpDirectionRounded.Equals(-1.0)) {
                        if(pt1.Y > cropBoxMinGlobal.Y) {
                            pt1 = new XYZ(pt1.X, cropBoxMinGlobal.Y, pt1.Z);
                        }
                    }

                    if(xUpDirectionRounded.Equals(1.0) && yUpDirectionRounded.Equals(0.0)) {
                        if(pt1.X < cropBoxMinGlobal.X) {
                            pt1 = new XYZ(cropBoxMinGlobal.X, pt1.Y, pt1.Z);
                        }
                    }

                    if(xUpDirectionRounded.Equals(-1.0) && yUpDirectionRounded.Equals(0.0)) {
                        if(pt1.X > cropBoxMinGlobal.X) {
                            pt1 = new XYZ(cropBoxMinGlobal.X, pt1.Y, pt1.Z);
                        }
                    }

                    pt2 = pt1 + _viewRightDirection;
                    break;
                case DimensionOffsetType.Left:
                    // Получаем единичный вектор вида направления вверх
                    var leftDirectionNormalized = _viewRightDirection.Normalize().Negate();
                    // Получаем отступ для более корректного размещения размера относительно арматуры
                    var offsetLeft = leftDirectionNormalized.Multiply(offsetCoefficient);

                    // Получаем первую точку размерной линии по BoundingBox каркаса армирования + отступ
                    if(xUpDirectionRounded == 1 || (yUpDirectionRounded == -1)) {
                        pt1 = bbox.Max + offsetLeft;
                    } else {
                        pt1 = bbox.Min + offsetLeft;
                    }

                    // Если точка, куда нужно поставить размерную линию находится за рамкой подрезки,
                    // то ставим по рамки подрезки
                    if(xUpDirectionRounded.Equals(0.0) && yUpDirectionRounded.Equals(1.0)) {
                        if(pt1.X < cropBoxMinGlobal.X) {
                            pt1 = new XYZ(cropBoxMinGlobal.X, pt1.Y, pt1.Z);
                        }
                    }

                    if(xUpDirectionRounded.Equals(0.0) && yUpDirectionRounded.Equals(-1.0)) {
                        if(pt1.X > cropBoxMinGlobal.X) {
                            pt1 = new XYZ(cropBoxMinGlobal.X, pt1.Y, pt1.Z);
                        }
                    }

                    if(xUpDirectionRounded.Equals(1.0) && yUpDirectionRounded.Equals(0.0)) {
                        if(pt1.Y > cropBoxMinGlobal.Y) {
                            pt1 = new XYZ(pt1.X, cropBoxMinGlobal.Y, pt1.Z);
                        }
                    }

                    if(xUpDirectionRounded.Equals(-1.0) && yUpDirectionRounded.Equals(0.0)) {
                        if(pt1.Y < cropBoxMinGlobal.Y) {
                            pt1 = new XYZ(pt1.X, cropBoxMinGlobal.Y, pt1.Z);
                        }
                    }

                    pt2 = pt1 + _viewUpDirection;
                    break;
                case DimensionOffsetType.Right:
                    // Получаем единичный вектор вида направления вверх
                    var rightDirectionNormalized = _viewRightDirection.Normalize();
                    // Получаем отступ для более корректного размещения размера относительно арматуры
                    var offsetRight = rightDirectionNormalized.Multiply(offsetCoefficient);

                    // Получаем первую точку размерной линии по BoundingBox каркаса армирования + отступ
                    if(xUpDirectionRounded == 1 || (yUpDirectionRounded == -1)) {
                        pt1 = bbox.Min + offsetRight;
                    } else {
                        pt1 = bbox.Max + offsetRight;
                    }

                    // Если точка, куда нужно поставить размерную линию находится за рамкой подрезки,
                    // то ставим по рамки подрезки
                    if(xUpDirectionRounded.Equals(0.0) && yUpDirectionRounded.Equals(1.0)) {
                        if(pt1.X > cropBoxMaxGlobal.X) {
                            pt1 = new XYZ(cropBoxMaxGlobal.X, pt1.Y, pt1.Z);
                        }
                    }

                    if(xUpDirectionRounded.Equals(0.0) && yUpDirectionRounded.Equals(-1.0)) {
                        if(pt1.X < cropBoxMaxGlobal.X) {
                            pt1 = new XYZ(cropBoxMaxGlobal.X, pt1.Y, pt1.Z);
                        }
                    }

                    if(xUpDirectionRounded.Equals(1.0) && yUpDirectionRounded.Equals(0.0)) {
                        if(pt1.Y < cropBoxMaxGlobal.Y) {
                            pt1 = new XYZ(pt1.X, cropBoxMaxGlobal.Y, pt1.Z);
                        }
                    }

                    if(xUpDirectionRounded.Equals(-1.0) && yUpDirectionRounded.Equals(0.0)) {
                        if(pt1.Y > cropBoxMaxGlobal.Y) {
                            pt1 = new XYZ(pt1.X, cropBoxMaxGlobal.Y, pt1.Z);
                        }
                    }

                    pt2 = pt1 + _viewUpDirection;
                    break;
                default:
                    break;
            }
            return Line.CreateBound(pt1, pt2);
        }


        public ReferenceArray GetDimensionRefs(FamilyInstance elem, char keyRefNamePart,
                                                List<string> importantRefNameParts, ReferenceArray oldRefArray = null) {
            var references = new List<Reference>();
            foreach(FamilyInstanceReferenceType referenceType in Enum.GetValues(typeof(FamilyInstanceReferenceType))) {
                references.AddRange(elem.GetReferences(referenceType));
            }

            // # является управляющим символом, сигнализирующим, что плоскость нужно использовать для образмеривания
            // и разделяющим имя плоскости на имя параметра проверки и остальной текст с ключевыми словами
            ReferenceArray refArray = new ReferenceArray();
            if(oldRefArray != null) {
                foreach(Reference reference in oldRefArray) {
                    refArray.Append(reference);
                }
            }

            importantRefNameParts.Add(keyRefNamePart.ToString());
            foreach(Reference reference in references) {
                string referenceName = elem.GetReferenceName(reference);
                if(!importantRefNameParts.All(namePart => referenceName.Contains(namePart))) {
                    continue;
                }

                string paramName = referenceName.Split(keyRefNamePart)[0];
                int paramValue = paramName == string.Empty ? 1 : _paramValueService.GetParamValueAnywhere(elem, paramName);

                if(paramValue == 1) {
                    refArray.Append(reference);
                }
            }
            return refArray;
        }

        public ReferenceArray GetDimensionRefs(List<Grid> grids, XYZ direction, ReferenceArray oldRefArray = null) {
            // Создаем матрицу трансформации
            Transform transform = Transform.Identity;
            transform.Origin = _viewOrigin;
            transform.BasisX = _viewRightDirection;
            transform.BasisY = _viewUpDirection;
            transform.BasisZ = _viewDirection;

            ReferenceArray refArray = new ReferenceArray();
            if(oldRefArray != null) {
                foreach(Reference reference in oldRefArray) {
                    refArray.Append(reference);
                }
            }
            // Нормализуем направление, заданное для проверки
            XYZ normalizedDirection = direction.Normalize();

            foreach(Grid grid in grids) {
                if(grid.Curve is Line line) {
                    // Получаем направление линии оси
                    XYZ lineDirection = line.Direction.Normalize();
                    XYZ lineDirectionByView = transform.OfVector(lineDirection);

                    if(lineDirectionByView.IsAlmostEqualTo(normalizedDirection, 0.01)
                        || lineDirectionByView.IsAlmostEqualTo(normalizedDirection.Negate(), 0.01)) {

                        Reference gridRef = new Reference(grid);
                        if(gridRef != null) {
                            refArray.Append(gridRef);
                        }
                    }
                }
            }
            return refArray;
        }
    }
}
