using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

namespace RevitRoughFinishingDesign.Models;
internal class CreatesLinesForFinishing {
    private readonly RevitRepository _revitRepository;
    private readonly WallDesignDataGetter _wallDesignDataGetter;

    private const double _maxDistanceFromBorder = 400; //мм, если стена находится на указанном расстоянии
                                                       //от границы - она остается на своем месте без смещения

    public CreatesLinesForFinishing(RevitRepository revitRepository, WallDesignDataGetter wallDesignDataGetter) {
        _revitRepository = revitRepository;
        _wallDesignDataGetter = wallDesignDataGetter;
    }

    public void DrawLines(RevitSettings settings) {
        var wallDesignDatas = _wallDesignDataGetter.GetWallDesignDatas(settings);
        double offset = _revitRepository.ConvertToFeetFromMillimeters(settings?.LineOffset ?? 0);
        IList<DetailLineForFinishing> linesForDraw = [];
        foreach(var wallDesignData in wallDesignDatas) {
            var correctLines = MakeOffsetForLines(
                wallDesignData,
                offset);
            foreach(var correctLine in correctLines) {
                if(wallDesignData.LineStyleId != ElementId.InvalidElementId &&
                    wallDesignData.LineStyleId != null) {

                    linesForDraw.Add(new DetailLineForFinishing(correctLine) {
                        LayerNumber = wallDesignData.LayerNumber,
                        Offset = offset,
                        DistanceFromBorder = wallDesignData.DistanceFromBorder,
                        LineStyleId = wallDesignData.LineStyleId
                    });
                }
            }
        }
        linesForDraw = ConnectLines(linesForDraw);
        foreach(var line in linesForDraw) {
            var detailLine = _revitRepository.Document.Create.NewDetailCurve(
                    _revitRepository.ActiveView, line.LineForFinishing) as DetailLine;
            AssignLineStyle(detailLine, line.LineStyleId);
        }
    }

    public IList<Line> MakeOffsetForLines(WallDesignData wallDesignData, double offset) {
        IList<Line> correctLines = [];
        var linesForCorrect = wallDesignData.LinesForDraw;
        var directionToRoom = wallDesignData.DirectionToRoom;
        double numberOfLayer = wallDesignData.LayerNumber;
        double tolerance = _revitRepository.ConvertToFeetFromMillimeters(_maxDistanceFromBorder);
        foreach(var line in linesForCorrect) {
            if(wallDesignData.DistanceFromBorder < tolerance) {

                // Получаем начальную и конечную точку линии
                var startPoint = line.GetEndPoint(0);
                var endPoint = line.GetEndPoint(1);
                double correctOffset = offset * numberOfLayer - wallDesignData.DistanceFromBorder;

                // Сдвигаем обе точки на заданное смещение внутрь помещения
                var newStartPoint = startPoint + directionToRoom * correctOffset;
                var newEndPoint = endPoint + directionToRoom * correctOffset;

                var newLine = Line.CreateBound(newStartPoint, newEndPoint);
                correctLines.Add(newLine);
            } else {
                correctLines.Add(line);
            }
        }
        return correctLines;
    }

    public IList<DetailLineForFinishing> ConnectLines(IList<DetailLineForFinishing> lines) {
        IList<DetailLineForFinishing> resultLines = [];
        var groupedResultLines = lines
            .GroupBy(l => l.LayerNumber);
        foreach(IList<DetailLineForFinishing> groupedLines in groupedResultLines) {
            foreach(var line in groupedLines) {
                try {
                    var closestIntersectionPoint = GetIntersectionsXYZPointsFromLines(line, groupedLines);
                    line.LineForFinishing = AdjustBaseLine(
                        line.LineForFinishing,
                        closestIntersectionPoint,
                        line.Offset);
                    resultLines.Add(line);
                } catch {
                }
            }
        }
        return resultLines;
    }

    public IList<XYZ> GetIntersectionsXYZPointsFromLines(
        DetailLineForFinishing currentLine,
        IList<DetailLineForFinishing> groupedLines) {
        var currentDirection = currentLine.LineForFinishing.Direction.Normalize();

        double correctOffset = currentLine.Offset * (currentLine.LayerNumber + 1);
        var currentExtendedLine = ExtendLine(currentLine.LineForFinishing, correctOffset);
        IList<XYZ> intersectionPoints = [];
        foreach(var line in groupedLines) {
            var lineDirection = line.LineForFinishing.Direction.Normalize();
            if(lineDirection.IsAlmostEqualTo(currentDirection) || currentLine.Guid == line.Guid) {
                continue;
            }
            var extendedLine = ExtendLine(line.LineForFinishing, correctOffset);
            // Проверяем, пересекаются ли линии
            var result = currentExtendedLine
                .Intersect(extendedLine, out var results);

            if(result == SetComparisonResult.Overlap && results != null && results.Size > 0) {
                var intersectPoint = results.get_Item(0).XYZPoint;
                intersectionPoints.Add(intersectPoint);
            }
        }
        return intersectionPoints;
    }

    /// <summary>
    /// Задает тип линии (LineStyle) для DetailLine
    /// </summary>
    private void AssignLineStyle(DetailLine detailLine, ElementId lineStyleId) {
        var doc = _revitRepository.Document;
        var categories = doc.Settings.Categories;

        _ = categories.get_Item(BuiltInCategory.OST_Lines);
        var lineStyles = _revitRepository.GetAllLineStyles();
        foreach(var lineStyle in lineStyles) {
            if(lineStyle.Id == lineStyleId) {
                detailLine.LineStyle = lineStyle;
                return;
            }
        }
    }

    public Line AdjustBaseLine(
        Line baseLine,
        IList<XYZ> intersectionPoints, double offset) {
        if(intersectionPoints == null || intersectionPoints.Count == 0) {
            return baseLine;
        }
        var startPoint = baseLine.GetEndPoint(0);
        var endPoint = baseLine.GetEndPoint(1);
        var extentedLine = ExtendLine(baseLine, offset);
        if(intersectionPoints.Count == 2) {
            return Line.CreateBound(intersectionPoints[0], intersectionPoints[1]);
        } else {
            foreach(var intersectionPoint in intersectionPoints) {
                double distanceToStart = extentedLine.GetEndPoint(0).DistanceTo(intersectionPoint);
                double distanceToEnd = extentedLine.GetEndPoint(1).DistanceTo(intersectionPoint);

                if(distanceToStart < distanceToEnd) {
                    startPoint = intersectionPoint;
                } else {
                    endPoint = intersectionPoint;
                }
            }
        }
        var newLine = Line.CreateBound(startPoint, endPoint);
        return newLine;
    }

    public Line ExtendLine(Line line, double offset) {
        var alongsideVector = line.Direction.Normalize();

        var startPoint = line.GetEndPoint(0);
        var endPoint = line.GetEndPoint(1);

        var newStartPoint = startPoint - alongsideVector * offset;
        var newEndPoint = endPoint + alongsideVector * offset;
        return Line.CreateBound(newStartPoint, newEndPoint);
    }
}
