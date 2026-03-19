using System;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitDocumenter.Models.MapServices;

namespace RevitDocumenter.Models.DimensionServices;

internal class DimensionChanger {
    private const double _horizontalOffsetFactor = 0.1;
    private const double _textHeightAdjustmentFactor = 2.0;
    private const double _verticalStepFactor = 1;
    private const double _horizontalStepFactor = 1.2;
    private const int _maxSearchSteps = 10;

    private readonly RevitRepository _revitRepository;
    private readonly DimensionCreator _dimensionCreator;
    private readonly BallCreator _ballCreator;

    public DimensionChanger(RevitRepository revitRepository, DimensionCreator dimensionCreator, BallCreator ballCreator) {
        _revitRepository = revitRepository;
        _dimensionCreator = dimensionCreator;
        _ballCreator = ballCreator;
    }

    public Dimension Change(
        Dimension dimension,
        ViewMapService mapService,
        double verticalStepValue,
        ReferenceArray dimensionReferences) {
        try {
            var textPoints = GetDimensionTextPoints(dimension);
            (var upDimensionVector, var rightDimensionVector) = GetDimensionVectors(dimension, textPoints);

            if(dimension.Value < _revitRepository.GetMinDimensionInView()) {
                var horizontalStep =
                    rightDimensionVector
                    * textPoints.BottomLeftCorner.DistanceTo(textPoints.BottomRightCorner)
                    * _horizontalStepFactor;
                dimension = DefinePositionForSmallDimension(
                    dimension,
                    dimensionReferences,
                    textPoints,
                    mapService,
                    upDimensionVector * verticalStepValue * _verticalStepFactor,
                    horizontalStep);
            } else {
                dimension = DefinePositionForNormalDimension(
                    dimension,
                    dimensionReferences,
                    textPoints,
                    mapService,
                    upDimensionVector * verticalStepValue * _verticalStepFactor);
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
        var centerPoint = GetDimensionCenterPoint(dimension);
        var leaderEndPosition = dimension.LeaderEndPosition;
        var textPosition = dimension.TextPosition;

        var directionToLeader = (leaderEndPosition - centerPoint).Normalize();
        var bottomLeft = leaderEndPosition + directionToLeader * _horizontalOffsetFactor;
        var bottomRight = bottomLeft + (centerPoint - bottomLeft) * 2;

        int viewScale = _revitRepository.Document.ActiveView.Scale;
        double textHeight = dimension.DimensionType.GetParamValue<double>(BuiltInParameter.TEXT_SIZE)
                            * viewScale
                            * _textHeightAdjustmentFactor;

        var upDirection = (dimension.TextPosition - centerPoint).Normalize();
        return
            new DimensionTextPoints(bottomLeft, bottomRight, upDirection, textHeight, centerPoint, textPosition);
    }

    private (XYZ DimensionUpVector, XYZ DimensionRightVector) GetDimensionVectors(
        Dimension dimension,
        DimensionTextPoints points) {
        return (
            (dimension.TextPosition - points.DimensionCenterPoint).Normalize(),
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
        DimensionTextPoints textPoints,
        ViewMapService mapService,
        XYZ verticalStep) {

        for(int stepIndex = 0; stepIndex <= _maxSearchSteps; stepIndex++) {
            // При первом заходе в цикл не ищем для размера новое, а проверяем его стандартное положение
            if(stepIndex > 0) {
                int directionFactor = stepIndex % 2 == 1 ? stepIndex : -stepIndex;
                textPoints.Translate(verticalStep * directionFactor);
            }
            if(!mapService.Check(
                   textPoints.BottomLeftCorner,
                   textPoints.BottomRightCorner,
                   textPoints.TopLeftCorner,
                   textPoints.TopRightCorner)) {
                continue;
            }

            // Если текст размера ничего не пересекает и сдвиг был, пересоздаем размер на новом месте
            if(stepIndex > 0) {
                dimension = RecreateDimension(dimension, textPoints, references, mapService);
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
        ViewMapService mapService,
        XYZ verticalStep,
        XYZ horizontalStep) {

        // Маленький размер всегда устанавливается с текстом по середине, поэтому перемещать его нужно сразу
        for(int stepIndex = 1; stepIndex <= _maxSearchSteps; stepIndex++) {
            bool success = false;

            // Если по бокам места нет, делаем шаг по вертикали и сбрасываем боковой сдвиг
            int verticalDirectionFactor = stepIndex % 2 == 1 ? -stepIndex : stepIndex;
            textPoints.Translate(verticalStep * verticalDirectionFactor);

            // Пробуем смещение вправо (1), затем влево (-2 от текущей позиции)
            foreach(int horizontalDirectionFactor in new[] { 1, -2 }) {
                textPoints.Translate(horizontalStep * horizontalDirectionFactor);

                if(!mapService.Check(
                       textPoints.BottomLeftCorner,
                       textPoints.BottomRightCorner,
                       textPoints.TopLeftCorner,
                       textPoints.TopRightCorner)) {
                    continue;
                }

                // Центрируем текст для маленьких размеров, чтобы он смотрелся корректно
                dimension = RecreateDimension(dimension, textPoints, references, mapService);
                dimension.TextPosition =
                    (textPoints.BottomLeftCorner + textPoints.BottomRightCorner) / 2
                    + (textPoints.TextPositionPoint - textPoints.DimensionCenterPoint);
                success = true;
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
        ReferenceArray references,
        ViewMapService mapService) {

        var dimensionLine = Line.CreateBound(points.BottomLeftCorner, points.BottomRightCorner);
        var newDimension = _dimensionCreator.Create(dimensionLine, references, oldDimension.DimensionType);
        _revitRepository.Document.Delete(oldDimension.Id);

        mapService.Paint(
            points.BottomLeftCorner,
            points.BottomRightCorner,
            points.TopLeftCorner,
            points.TopRightCorner);
        return newDimension;
    }
}
