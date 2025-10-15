using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitPylonDocumentation.Models.PluginOptions;

namespace RevitPylonDocumentation.Models.Services;

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


    public Line GetDimensionLine(Element rebar, DirectionType directionType, double offsetCoefficient) {
        // Задаем дефолтные точки на случай, если не сработает получение
        var pt1 = new XYZ(0, 0, 0);
        var pt2 = new XYZ(0, 100, 0);

        directionType = directionType switch {
            DirectionType.LeftTop => DirectionType.Left,
            DirectionType.LeftBottom => DirectionType.Left,
            DirectionType.RightTop => DirectionType.Right,
            DirectionType.RightBottom => DirectionType.Right,
            _ => directionType
        };

        // Если взять краевую точку рамки подрезки вида, то она будет в локальных координатах вида
        // Для перевода в глобальные координаты получим объект Transform
        // Получаем начало локальной системы координат вида в глобальной системе координат
        double xUpDirectionRounded = Math.Round(_viewUpDirection.X);
        double yUpDirectionRounded = Math.Round(_viewUpDirection.Y);
        double zUpDirectionRounded = Math.Round(_viewUpDirection.Z);

        double xRightDirectionRounded = Math.Round(_viewRightDirection.X);
        double yRightDirectionRounded = Math.Round(_viewRightDirection.Y);

        // Создаем матрицу трансформации
        var transform = Transform.Identity;
        transform.Origin = _viewOrigin;
        transform.BasisX = _viewRightDirection;
        transform.BasisY = _viewUpDirection;
        transform.BasisZ = _viewDirection;

        // Получаем правую верхнюю точку рамки подрезки вида в системе координат вида
        var cropBoxMax = _viewCropBox.Max;
        // Получаем левую нижнюю точку рамки подрезки вида в системе координат вида
        var cropBoxMin = _viewCropBox.Min;

        // Переводим их в глобальную систему координат
        var cropBoxMaxGlobal = transform.OfPoint(cropBoxMax);
        var cropBoxMinGlobal = transform.OfPoint(cropBoxMin);

        var bbox = rebar.get_BoundingBox(_view);
        switch(directionType) {
            case DirectionType.Top:
                // Получаем единичный вектор вида направления вверх
                var upDirectionNormalized = _viewUpDirection.Normalize();
                // Получаем отступ для более корректного размещения размера относительно арматуры
                var offsetTop = upDirectionNormalized.Multiply(offsetCoefficient);

                // Получаем первую точку размерной линии по BoundingBox каркаса армирования + отступ
                if(xUpDirectionRounded == -1 || yUpDirectionRounded == -1) {
                    pt1 = bbox.Min + offsetTop;
                } else {
                    pt1 = bbox.Max + offsetTop;
                }
                pt2 = pt1 + _viewRightDirection;
                break;
            case DirectionType.Bottom:
                // Получаем единичный вектор вида направления вниз
                var downDirectionNormalized = _viewUpDirection.Normalize().Negate();
                // Получаем отступ для более корректного размещения размера относительно арматуры
                var offsetBottom = downDirectionNormalized.Multiply(offsetCoefficient);

                // Получаем первую точку размерной линии по BoundingBox каркаса армирования + отступ
                if(xUpDirectionRounded == -1 || yUpDirectionRounded == -1) {
                    pt1 = bbox.Max + offsetBottom;
                } else {
                    pt1 = bbox.Min + offsetBottom;
                }
                pt2 = pt1 + _viewRightDirection;
                break;
            case DirectionType.Left:
                // Получаем единичный вектор вида направления вверх
                var leftDirectionNormalized = _viewRightDirection.Normalize().Negate();
                // Получаем отступ для более корректного размещения размера относительно арматуры
                var offsetLeft = leftDirectionNormalized.Multiply(offsetCoefficient);

                // Получаем первую точку размерной линии по BoundingBox каркаса армирования + отступ
                if(zUpDirectionRounded == 1) {
                    // Вертикальное сечение
                    if(xRightDirectionRounded == -1 || yRightDirectionRounded == -1) {
                        pt1 = bbox.Max + offsetLeft;
                    } else {
                        pt1 = bbox.Min + offsetLeft;
                    }
                } else {
                    // Горизонтальное сечение
                    if(xUpDirectionRounded == 1 || yUpDirectionRounded == -1) {
                        pt1 = bbox.Max + offsetLeft;
                    } else {
                        pt1 = bbox.Min + offsetLeft;
                    }
                }
                pt2 = pt1 + _viewUpDirection;
                break;
            case DirectionType.Right:
                // Получаем единичный вектор вида направления вверх
                var rightDirectionNormalized = _viewRightDirection.Normalize();
                // Получаем отступ для более корректного размещения размера относительно арматуры
                var offsetRight = rightDirectionNormalized.Multiply(offsetCoefficient);

                // Получаем первую точку размерной линии по BoundingBox каркаса армирования + отступ
                if(zUpDirectionRounded == 1) {
                    // Вертикальное сечение
                    if(xRightDirectionRounded == -1 || yRightDirectionRounded == -1) {
                        pt1 = bbox.Min + offsetRight;
                    } else {
                        pt1 = bbox.Max + offsetRight;
                    }
                } else {
                    // Горизонтальное сечение
                    if(xUpDirectionRounded == 1 || yUpDirectionRounded == -1) {
                        pt1 = bbox.Min + offsetRight;
                    } else {
                        pt1 = bbox.Max + offsetRight;
                    }
                }
                pt2 = pt1 + _viewUpDirection;
                break;
            default:
                break;
        }
        return Line.CreateBound(pt1, pt2);
    }


    /// <summary>
    /// Возвращает массив опорных плоскостей, взятых с элемента и отфильтрованных по имени.
    /// Имя опорной плоскости задается по маске: "ПарамФильтр1/ПарамФильтр2#_КлючСлово1_КлючСлово2",
    /// где параметров фильтрации и ключевых слов может быть сколько угодно.
    /// </summary>
    /// <param name="elem">Элемент,у которого необходимо получить опорные плоскости</param>
    /// <param name="keyRefNamePart">Символ-разделитель имени опорной плоскости между частью с информацией 
    /// о фильтрующих параметрах и частью с ключевыми словами</param>
    /// <param name="refNameParamsSeparator">Символ-разделитель имени опорной плоскости между фильтрующими параметрами</param>
    /// <param name="importantRefNameParts">Список ключевых слов, которые должны быть в имени опорной плоскости</param>
    /// <param name="unimportantRefNameParts">Список ключевых слов, которые НЕ должны быть в имени опорной плоскости</param>
    /// <param name="oldRefArray">Предыдущий массив опорных плоскостей, в который можно дописать новые плоскости</param>
    /// <returns></returns>
    public ReferenceArray GetDimensionRefs(FamilyInstance elem, List<string> importantRefNameParts, 
                                           List<string> unimportantRefNameParts = null,
                                           char keyRefNamePart = '#', char refNameParamsSeparator = '/',
                                           ReferenceArray oldRefArray = null) {
        var references = new List<Reference>();
        foreach(FamilyInstanceReferenceType referenceType in Enum.GetValues(typeof(FamilyInstanceReferenceType))) {
            references.AddRange(elem.GetReferences(referenceType));
        }

        // # является управляющим символом, сигнализирующим, что плоскость нужно использовать для образмеривания
        // и разделяющим имя плоскости на имя параметра проверки и остальной текст с ключевыми словами
        var refArray = new ReferenceArray();
        if(oldRefArray != null) {
            foreach(Reference reference in oldRefArray) {
                refArray.Append(reference);
            }
        }
        importantRefNameParts.Add(keyRefNamePart.ToString());
        unimportantRefNameParts = unimportantRefNameParts is null ? [] : unimportantRefNameParts;
        foreach(var reference in references) {
            string referenceName = elem.GetReferenceName(reference);
            if(!importantRefNameParts.All(referenceName.Contains) || unimportantRefNameParts.Any(referenceName.Contains)) {
                continue;
            }

            // мод_ФОП_Доборный 1/мод_ФОП_Доборный 1_Массив#1_торец
            // мод_ФОП_Доборный 1#1_торец
            string paramParts = referenceName.Split(keyRefNamePart)[0];
            int paramValue = 0;

            string[] parameters = [paramParts];
            if(paramParts.Contains(refNameParamsSeparator)) {
                parameters = paramParts.Split(refNameParamsSeparator);
            }

            foreach(string paramPart in parameters) {
                if(paramPart == string.Empty) {
                    paramValue = 1;
                } else {
                    paramValue = _paramValueService.GetParamValueAnywhere<int>(elem, paramPart);
                }
                if(paramValue == 0) {
                    break;
                }
            }
            if(paramValue == 1) {
                refArray.Append(reference);
            }
        }
        return refArray;
    }


    public ReferenceArray GetDimensionRefs(List<Grid> grids, XYZ direction, ReferenceArray oldRefArray = null) {
        // Создаем матрицу трансформации
        var transform = Transform.Identity;
        transform.Origin = _viewOrigin;
        transform.BasisX = _viewRightDirection;
        transform.BasisY = _viewUpDirection;
        transform.BasisZ = _viewDirection;

        var refArray = new ReferenceArray();
        if(oldRefArray != null) {
            foreach(Reference reference in oldRefArray) {
                refArray.Append(reference);
            }
        }
        // Нормализуем направление, заданное для проверки
        var normalizedDirection = direction.Normalize();

        foreach(var grid in grids) {
            if(grid.Curve is Line line) {
                // Получаем направление линии оси
                var lineDirection = line.Direction.Normalize();
                var lineDirectionByView = transform.OfVector(lineDirection);

                // Оси могут быть проложены в двух направлениях
                // В случае, если это вертикальный вид, то нужно привязываться к осям всегда
                // В случае, если это горизонтальный вид, то нужно привязываться в зависимости от направления вида
                if(_view.UpDirection.IsAlmostEqualTo(XYZ.BasisZ)
                    || lineDirectionByView.IsAlmostEqualTo(normalizedDirection, 0.01)
                    || lineDirectionByView.IsAlmostEqualTo(normalizedDirection.Negate(), 0.01)) {
                    var gridRef = new Reference(grid);
                    if(gridRef != null) {
                        refArray.Append(gridRef);
                    }
                } 
            }
        }
        return refArray;
    }
}
