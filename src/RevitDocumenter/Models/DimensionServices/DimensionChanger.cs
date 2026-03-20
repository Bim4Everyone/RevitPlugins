using System;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitDocumenter.Models.MapServices;

namespace RevitDocumenter.Models.DimensionServices;

internal class DimensionChanger {
    private const double _horizontalOffsetFactor = 0.1;
    private const double _textHeightFactor = 2.0;
    private const double _verticalStepFactor = 1;
    private const double _horizontalStepFactor = 1.5;
    private const int _maxSearchSteps = 10;

    private readonly RevitRepository _revitRepository;
    private readonly DimensionCreator _dimensionCreator;
    private readonly BallCreator _ballCreator;
    private readonly ViewMapService _mapService;

    public DimensionChanger(
        RevitRepository revitRepository,
        DimensionCreator dimensionCreator,
        BallCreator ballCreator,
        ViewMapService mapService) {
        _revitRepository = revitRepository;
        _dimensionCreator = dimensionCreator;
        _ballCreator = ballCreator;
        _mapService = mapService;
    }

    public Dimension Change(Dimension dimension, MapInfo mapInfo, ReferenceArray dimensionReferences) {
        try {
            var textPoints = GetDimensionTextPoints(dimension);
            (var upDimensionVector, var rightDimensionVector) = GetDimensionVectors(dimension, textPoints);
            var verticalStep = upDimensionVector * mapInfo.MappingStepInFeet * _verticalStepFactor;
            var horizontalStep = rightDimensionVector
                * textPoints.BottomLeftCorner.DistanceTo(textPoints.BottomRightCorner)
                * _horizontalStepFactor;

            if(dimension.Value > _revitRepository.GetMinDimensionInView()) {
                dimension = DefinePositionForNormalDimension(
                    dimension,
                    dimensionReferences,
                    mapInfo,
                    textPoints,
                    verticalStep);
            } else {
                dimension = DefinePositionForSmallDimension(
                    dimension,
                    dimensionReferences,
                    textPoints,
                    mapInfo,
                    verticalStep,
                    horizontalStep);
            }
        } catch {
            // ignored
        }
        return dimension;
    }

    /// <summary>
    /// Определяет контрольные точки текстового поля размера
    /// </summary>
    private DimensionTextPoints GetDimensionTextPoints(Dimension dimension) {
        var textLineCenterPoint = GetDimensionCenterPoint(dimension);
        var leaderEndPosition = dimension.LeaderEndPosition;
        var textPosition = dimension.TextPosition;

        var directionToLeader = (leaderEndPosition - textLineCenterPoint).Normalize();
        var bottomLeft = leaderEndPosition + directionToLeader * _horizontalOffsetFactor;
        var bottomRight = bottomLeft + (textLineCenterPoint - bottomLeft) * 2;

        int viewScale = _revitRepository.Document.ActiveView.Scale;
        double textHeight = dimension.DimensionType.GetParamValue<double>(BuiltInParameter.TEXT_SIZE)
                            * viewScale
                            * _textHeightFactor;

        var upDirection = (dimension.TextPosition - textLineCenterPoint).Normalize();
        return
            new DimensionTextPoints(bottomLeft, bottomRight, upDirection * textHeight, textLineCenterPoint, textPosition);
    }

    private (XYZ DimensionUpVector, XYZ DimensionRightVector) GetDimensionVectors(
        Dimension dimension,
        DimensionTextPoints points) {
        return (
            (dimension.TextPosition - points.TextLineCenterPoint).Normalize(),
            (points.BottomRightCorner - points.BottomLeftCorner).Normalize());
    }

    /// <summary>
    /// Получает точку посередине размера под текстом значения размера
    /// </summary>
    private XYZ GetDimensionCenterPoint(Dimension dimension) {
        return dimension.Curve is not Line dimensionLine
            ? throw new InvalidOperationException("The dimension line must be straight.")
            : dimensionLine.Project(dimension.TextPosition).XYZPoint;
    }

    /// <summary>
    /// Метод анализирует положение текста размера с учетом бинарной карты, определяет нужно ли выполнять смещение
    /// и при необходимости пытается подобрать положение, чтобы текст ничего не пересекал на чертеже, а затем
    /// пересоздает размер.
    /// Если текст размера с учетом масштаба укладывается между засечками размера, то размер считается нормальным и
    /// для него ищется смещение только по вертикали 
    /// </summary>
    private Dimension DefinePositionForNormalDimension(
        Dimension dimension,
        ReferenceArray references,
        MapInfo mapInfo,
        DimensionTextPoints textPoints,
        XYZ verticalStep) {

        var dimensionLine = dimension.Curve as Line;
        if(dimensionLine == null)
            return dimension;

        double dimensionLineDirX = dimensionLine.Direction.X;
        double tolerance = 1e-10;
        bool isOrthogonalDimension =
            Math.Abs(dimensionLineDirX - 0) < tolerance
            || Math.Abs(dimensionLineDirX - 1) < tolerance
            || Math.Abs(dimensionLineDirX + 1) < tolerance;

        for(int stepIndex = 0; stepIndex <= _maxSearchSteps; stepIndex++) {
            // При первом заходе в цикл не ищем для размера новое, а проверяем его стандартное положение
            if(stepIndex > 0) {
                int directionFactor = stepIndex % 2 == 1 ? stepIndex : -stepIndex;
                textPoints.Translate(verticalStep * directionFactor);
            }

            if(isOrthogonalDimension) {
                if(!_mapService.CheckInRectangle(mapInfo, textPoints.BottomLeftCorner, textPoints.TopRightCorner)) {
                    continue;
                }
            } else {
                if(!_mapService.CheckInRectangle(
                    mapInfo,
                    textPoints.TextMiddlePoint,
                    1)) {
                    continue;
                }
            }

            // Если текст размера ничего не пересекает и сдвиг был, пересоздаем размер на новом месте
            if(stepIndex > 0) {
                dimension = RecreateDimension(dimension, textPoints, references);
            }

            // Фиксируем в карте, что теперь эти квадраты заняты
            if(isOrthogonalDimension) {
                _mapService.PaintInRectangle(mapInfo, textPoints.BottomLeftCorner, textPoints.TopRightCorner);
            } else {
                _mapService.PaintInRectangle(mapInfo, textPoints.TextMiddlePoint, 1);
            }
            break;
        }
        return dimension;
    }

    /// <summary>
    /// Метод анализирует положение текста размера с учетом бинарной карты, определяет нужно ли выполнять смещение
    /// и при необходимости пытается подобрать положение, чтобы текст ничего не пересекал на чертеже, а затем
    /// пересоздает размер.
    /// Если текст размера с учетом масштаба не укладывается между засечками размера, то размер считается маленьким и
    /// для него ищется смещение по вертикали и в бок (влево и вправо) 
    /// </summary>
    private Dimension DefinePositionForSmallDimension(
        Dimension dimension,
        ReferenceArray references,
        DimensionTextPoints textPoints,
        MapInfo mapInfo,
        XYZ verticalStep,
        XYZ horizontalStep) {

        var dimensionLine = dimension.Curve as Line;
        if(dimensionLine == null)
            return dimension;

        double dimensionLineDirX = dimensionLine.Direction.X;
        double tolerance = 1e-10;
        bool isOrthogonalDimension =
            Math.Abs(dimensionLineDirX - 0) < tolerance
            || Math.Abs(dimensionLineDirX - 1) < tolerance
            || Math.Abs(dimensionLineDirX + 1) < tolerance;

        // Маленький размер всегда устанавливается с текстом по середине, поэтому перемещать его нужно сразу
        for(int stepIndex = 1; stepIndex <= _maxSearchSteps; stepIndex++) {
            bool success = false;

            // Если по бокам места нет, делаем шаг по вертикали и сбрасываем боковой сдвиг
            int verticalDirectionFactor = stepIndex % 2 == 1 ? -stepIndex : stepIndex;
            textPoints.Translate(verticalStep * verticalDirectionFactor);

            // Пробуем смещение вправо (1), затем влево (-2 от текущей позиции)
            foreach(int horizontalDirectionFactor in new[] { 1, -2 }) {
                textPoints.Translate(horizontalStep * horizontalDirectionFactor);

                if(isOrthogonalDimension) {
                    if(!_mapService.CheckInRectangle(mapInfo, textPoints.BottomLeftCorner, textPoints.TopRightCorner)) {
                        continue;
                    }
                } else {
                    if(!_mapService.CheckInRectangle(
                        mapInfo,
                        textPoints.TextMiddlePoint,
                        1)) {
                        continue;
                    }
                }

                // Центрируем текст для маленьких размеров, чтобы он смотрелся корректно
                dimension = RecreateDimension(dimension, textPoints, references);

                // Фиксируем в карте, что теперь эти квадраты заняты
                if(isOrthogonalDimension) {
                    _mapService.PaintInRectangle(mapInfo, textPoints.BottomLeftCorner, textPoints.TopRightCorner);
                } else {
                    _mapService.PaintInRectangle(mapInfo, textPoints.TextMiddlePoint, 1);
                }
                success = true;
                break;
            }
            // Если нашли нужно положение, то больше не ищем
            if(success)
                break;

            // Возвращаем текст к центру
            textPoints.Translate(horizontalStep);
        }
        return dimension;
    }

    private Dimension RecreateDimension(
        Dimension oldDimension,
        DimensionTextPoints points,
        ReferenceArray references) {

        var dimensionLine = Line.CreateBound(points.BottomLeftCorner, points.BottomRightCorner);
        var newDimension = _dimensionCreator.Create(dimensionLine, references, oldDimension.DimensionType);
        newDimension.TextPosition = points.TextPositionPoint;
        _revitRepository.Document.Delete(oldDimension.Id);
        return newDimension;
    }
}
