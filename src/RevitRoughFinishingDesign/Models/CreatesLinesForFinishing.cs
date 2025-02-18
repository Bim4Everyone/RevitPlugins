using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

namespace RevitRoughFinishingDesign.Models {
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
            IList<WallDesignData> wallDesignDatas = _wallDesignDataGetter.GetWallDesignDatas(settings);
            double offset = _revitRepository.ConvertToFeetFromMillimeters(settings?.LineOffset ?? 0);
            IList<DetailLineForFinishing> linesForDraw = new List<DetailLineForFinishing>();
            foreach(var wallDesignData in wallDesignDatas) {
                IList<Line> correctLines = MakeOffsetForLines(
                    wallDesignData,
                    offset);
                foreach(Line correctLine in correctLines) {
                    linesForDraw.Add(new DetailLineForFinishing(correctLine) {
                        LayerNumber = wallDesignData.LayerNumber,
                        Offset = offset,
                        DistanceFromBorder = wallDesignData.DistanceFromBorder,
                        LineStyleId = wallDesignData.LineStyleId
                    });
                }
            }
            linesForDraw = ConnectLines(linesForDraw);
            foreach(DetailLineForFinishing line in linesForDraw) {
                DetailLine detailLine = _revitRepository.Document.Create.NewDetailCurve(
                        _revitRepository.ActiveView, line.LineForFinishing) as DetailLine;
                AssignLineStyle(detailLine, line.LineStyleId);
            }
        }

        public IList<Line> MakeOffsetForLines(WallDesignData wallDesignData, double offset) {
            IList<Line> correctLines = new List<Line>();
            IList<Line> linesForCorrect = wallDesignData.LinesForDraw;
            XYZ directionToRoom = wallDesignData.DirectionToRoom;
            double numberOfLayer = wallDesignData.LayerNumber;
            double tolerance = _revitRepository.ConvertToFeetFromMillimeters(_maxDistanceFromBorder);
            foreach(Line line in linesForCorrect) {
                if(wallDesignData.DistanceFromBorder < tolerance) {

                    // Получаем начальную и конечную точку линии
                    XYZ startPoint = line.GetEndPoint(0);
                    XYZ endPoint = line.GetEndPoint(1);
                    double correctOffset = (offset * numberOfLayer) - wallDesignData.DistanceFromBorder;

                    // Сдвигаем обе точки на заданное смещение внутрь помещения
                    XYZ newStartPoint = startPoint + directionToRoom * (correctOffset);
                    XYZ newEndPoint = endPoint + directionToRoom * (correctOffset);

                    Line newLine = Line.CreateBound(newStartPoint, newEndPoint);
                    correctLines.Add(newLine);
                } else {
                    correctLines.Add(line);
                }
            }
            return correctLines;
        }

        public IList<DetailLineForFinishing> ConnectLines(IList<DetailLineForFinishing> lines) {
            IList<DetailLineForFinishing> resultLines = new List<DetailLineForFinishing>();
            var groupedResultLines = lines
                .GroupBy(l => l.LayerNumber);
            foreach(IList<DetailLineForFinishing> groupedLines in groupedResultLines) {
                foreach(DetailLineForFinishing line in groupedLines) {
                    try {
                        IList<XYZ> closestIntersectionPoint = GetIntersectionsXYZPointsFromLines(line, groupedLines);
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
            XYZ currentDirection = currentLine.LineForFinishing.Direction.Normalize();
            XYZ intersectPoint = null;
            double correctOffset = currentLine.Offset;
            Line currentExtendedLine = ExtendLine(currentLine.LineForFinishing, correctOffset);
            IList<XYZ> intersectionPoints = new List<XYZ>();
            foreach(DetailLineForFinishing line in groupedLines) {
                XYZ lineDirection = line.LineForFinishing.Direction.Normalize();
                if(lineDirection.IsAlmostEqualTo(currentDirection) || currentLine.Guid == line.Guid) {
                    continue;
                }
                Line extendedLine = ExtendLine(line.LineForFinishing, correctOffset);
                // Проверяем, пересекаются ли линии
                IntersectionResultArray results;
                SetComparisonResult result = currentExtendedLine
                    .Intersect(extendedLine, out results);

                if(result == SetComparisonResult.Overlap && results != null && results.Size > 0) {
                    intersectPoint = results.get_Item(0).XYZPoint; // Возвращаем первую точку пересечения
                    intersectionPoints.Add(intersectPoint);
                }
            }
            return intersectionPoints;
        }

        /// <summary>
        /// Задает тип линии (LineStyle) для DetailLine
        /// </summary>
        private void AssignLineStyle(DetailLine detailLine, ElementId lineStyleId) {
            Document doc = _revitRepository.Document;
            Categories categories = doc.Settings.Categories;
            Category linesCategory = categories.get_Item(BuiltInCategory.OST_Lines);
            IList<GraphicsStyle> lineStyles = _revitRepository.GetAllLineStyles();
            foreach(GraphicsStyle lineStyle in lineStyles) {
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
            XYZ startPoint = baseLine.GetEndPoint(0);
            XYZ endPoint = baseLine.GetEndPoint(1);
            Line extentedLine = ExtendLine(baseLine, offset);
            if(intersectionPoints.Count == 2) {
                return Line.CreateBound(intersectionPoints[0], intersectionPoints[1]);
            } else {
                foreach(XYZ intersectionPoint in intersectionPoints) {
                    double distanceToStart = extentedLine.GetEndPoint(0).DistanceTo(intersectionPoint);
                    double distanceToEnd = extentedLine.GetEndPoint(1).DistanceTo(intersectionPoint);

                    if(distanceToStart < distanceToEnd) {
                        startPoint = intersectionPoint;
                    } else {
                        endPoint = intersectionPoint;
                    }
                }
            }
            Line newLine = Line.CreateBound(startPoint, endPoint);
            return newLine;
        }

        public Line ExtendLine(Line line, double offset) {
            XYZ alongsideVector = line.Direction.Normalize();

            XYZ startPoint = line.GetEndPoint(0);
            XYZ endPoint = line.GetEndPoint(1);

            XYZ newStartPoint = startPoint - alongsideVector * (offset);
            XYZ newEndPoint = endPoint + alongsideVector * (offset);
            return Line.CreateBound(newStartPoint, newEndPoint);
        }
    }
}
