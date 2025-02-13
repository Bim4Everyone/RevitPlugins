using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

namespace RevitRoughFinishingDesign.Models {
    internal class CreatesLinesForFinishing {
        private readonly RevitRepository _revitRepository;
        private readonly WallDesignDataGetter _wallDesignDataGetter;

        public CreatesLinesForFinishing(RevitRepository revitRepository, WallDesignDataGetter wallDesignDataGetter) {
            _revitRepository = revitRepository;
            _wallDesignDataGetter = wallDesignDataGetter;
        }

        public void DrawLines(PluginConfig config) {
            IList<WallDesignData> wallDesignDatas = _wallDesignDataGetter.GetWallDesignDatas();
            double offset = _revitRepository.ConvertToFeetFromMillimeters(config.LineOffset.Value);
            IList<DetailLineForFinishing> linesForDraw = new List<DetailLineForFinishing>();
            foreach(var wallDesignData in wallDesignDatas) {
                //double offset = _revitRepository.ConvertToFeetFromMillimeters(30);
                IList<Line> correctLines = MakeOffsetForLines(
                    wallDesignData,
                    offset);
                foreach(Line correctLine in correctLines) {
                    linesForDraw.Add(new DetailLineForFinishing(correctLine) {
                        LayerNumber = wallDesignData.LayerNumber,
                        Offset = offset,
                        DistanceFromBorder = wallDesignData.DistanceFromBorder
                    });
                }
                //foreach(Line line in correctLines) {
                //    DetailLine detailLine = _revitRepository.Document.Create.NewDetailCurve(
                //        _revitRepository.ActiveView, line) as DetailLine;
                //    AssignLineStyle(detailLine, "!АИ_Потолок");
                //}
            }
            linesForDraw = ConnectLines(linesForDraw);
            foreach(DetailLineForFinishing line in linesForDraw) {
                DetailLine detailLine = _revitRepository.Document.Create.NewDetailCurve(
                        _revitRepository.ActiveView, line.LineForFinishing) as DetailLine;
                AssignLineStyle(detailLine, "!АИ_Потолок");
            }
        }

        public IList<Line> MakeOffsetForLines(WallDesignData wallDesignData, double offset) {
            IList<Line> correctLines = new List<Line>();
            IList<Line> linesForCorrect = wallDesignData.LinesForDraw;
            XYZ directionToRoom = wallDesignData.DirectionToRoom;
            double numberOfLayer = wallDesignData.LayerNumber;
            foreach(Line line in linesForCorrect) {

                XYZ alongsideVector = line.Direction.Normalize();
                // Получаем начальную и конечную точку линии
                XYZ startPoint = line.GetEndPoint(0);
                XYZ endPoint = line.GetEndPoint(1);
                double correctOffset = (offset * numberOfLayer) - wallDesignData.DistanceFromBorder;

                // Сдвигаем обе точки на заданное смещение внутрь помещения
                XYZ newStartPoint = startPoint + directionToRoom * (correctOffset);
                XYZ newEndPoint = endPoint + directionToRoom * (correctOffset);

                //Удлиняем линию
                //newStartPoint = newStartPoint - alongsideVector * (correctOffset);
                //newEndPoint = newEndPoint + alongsideVector * (correctOffset);
                // Создаем новую линию с обновленными точками
                Line newLine = Line.CreateBound(newStartPoint, newEndPoint);
                correctLines.Add(newLine);
            }
            return correctLines;
        }

        //public bool IsCornerLine(Line line, IList<Line> allLines, double tolerance = 1e-6) {
        //    XYZ startPoint = line.GetEndPoint(0);
        //    XYZ endPoint = line.GetEndPoint(1);
        //    XYZ direction = line.Direction.Normalize();

        //    bool hasDifferentDirectionAtStart = allLines.Any(l =>
        //        l != line &&
        //        l.GetEndPoint(0).IsAlmostEqualTo(startPoint, tolerance) || l.GetEndPoint(1).IsAlmostEqualTo(startPoint, tolerance) &&
        //        !l.Direction.Normalize().IsAlmostEqualTo(direction, tolerance));

        //    bool hasDifferentDirectionAtEnd = allLines.Any(l =>
        //        l != line &&
        //        l.GetEndPoint(0).IsAlmostEqualTo(endPoint, tolerance) || l.GetEndPoint(1).IsAlmostEqualTo(endPoint, tolerance) &&
        //        !l.Direction.Normalize().IsAlmostEqualTo(direction, tolerance));

        //    return hasDifferentDirectionAtStart || hasDifferentDirectionAtEnd;
        //}

        public IList<DetailLineForFinishing> ConnectLines(IList<DetailLineForFinishing> lines) {
            IList<DetailLineForFinishing> resultLines = new List<DetailLineForFinishing>();
            var groupedResultLines = lines
                .GroupBy(l => l.LayerNumber);
            foreach(IList<DetailLineForFinishing> groupedLines in groupedResultLines) {
                foreach(DetailLineForFinishing line in groupedLines) {
                    IList<XYZ> closestIntersectionPoint = GetIntersectionsXYZPointsFromLines(line, groupedLines);
                    line.LineForFinishing = AdjustBaseLine(line.LineForFinishing, closestIntersectionPoint);
                    resultLines.Add(line);
                }
            }
            return resultLines;
        }

        public IList<XYZ> GetIntersectionsXYZPointsFromLines(
            DetailLineForFinishing currentLine,
            IList<DetailLineForFinishing> groupedLines) {
            XYZ currentDirection = currentLine.LineForFinishing.Direction.Normalize();
            XYZ intersectPoint = null;
            double correctOffset = (currentLine.Offset * currentLine.LayerNumber) - currentLine.DistanceFromBorder;
            Line extentedLine = ExtendLine(currentLine.LineForFinishing, correctOffset);
            IList<XYZ> intersectionPoints = new List<XYZ>();
            foreach(DetailLineForFinishing line in groupedLines) {
                XYZ lineDirection = line.LineForFinishing.Direction.Normalize();
                if(lineDirection.IsAlmostEqualTo(currentDirection) || currentLine.Guid == line.Guid) {
                    continue;
                }
                // Проверяем, пересекаются ли линии
                IntersectionResultArray results;
                SetComparisonResult result = extentedLine
                    .Intersect(line.LineForFinishing, out results);

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
        private void AssignLineStyle(DetailLine detailLine, string lineStyleName) {
            Document doc = _revitRepository.Document;
            Categories categories = doc.Settings.Categories;
            Category linesCategory = categories.get_Item(BuiltInCategory.OST_Lines);

            foreach(Category subCategory in linesCategory.SubCategories) {
                if(subCategory.Name == lineStyleName) {
                    detailLine.LineStyle = subCategory.GetGraphicsStyle(GraphicsStyleType.Projection);
                    return;
                }
            }
        }

        public static Line AdjustBaseLine(
            Line baseLine,
            IList<XYZ> intersectionPoints) {
            if(intersectionPoints == null) {
                return baseLine;
            }
            Line revitBaseLine = baseLine;
            XYZ startPoint = revitBaseLine.GetEndPoint(0);
            XYZ endPoint = revitBaseLine.GetEndPoint(1);
            foreach(XYZ intersectionPoint in intersectionPoints) {
                double distanceToStart = revitBaseLine.GetEndPoint(0).DistanceTo(intersectionPoint);
                double distanceToEnd = revitBaseLine.GetEndPoint(1).DistanceTo(intersectionPoint);
                if(distanceToStart < distanceToEnd) {
                    startPoint = intersectionPoint;
                } else {
                    endPoint = intersectionPoint;
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
