using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitRoughFinishingDesign.Models {
    internal class CreatesLinesForFinishing {
        private readonly RevitRepository _revitRepository;
        private readonly WallDesignDataGetter _wallDesignDataGetter;

        public CreatesLinesForFinishing(RevitRepository revitRepository, WallDesignDataGetter wallDesignDataGetter) {
            _revitRepository = revitRepository;
            _wallDesignDataGetter = wallDesignDataGetter;
        }

        public void DrawLines() {
            IList<WallDesignData> wallDesignDatas = _wallDesignDataGetter.GetWallDesignDatas();
            foreach(var wallDesignData in wallDesignDatas) {
                double offset = _revitRepository.ConvertToFeetFromMillimeters(30);
                IList<Line> correctLines = CorrectLines(
                    wallDesignData,
                    offset);
                foreach(Line line in correctLines) {
                    DetailLine detailLine = _revitRepository.Document.Create.NewDetailCurve(
                        _revitRepository.ActiveView, line) as DetailLine;
                    AssignLineStyle(detailLine, "!АИ_Потолок");
                }
            }
        }

        public IList<Line> CorrectLines(WallDesignData wallDesignData, double offset) {
            IList<Line> correctLines = new List<Line>();
            IList<Line> linesForCorrect = wallDesignData.LinesForDraw;
            XYZ direction = wallDesignData.DirectionToRoom;
            double numberOfLayer = wallDesignData.LayerNumber;
            foreach(Line line in linesForCorrect) {
                // Получаем начальную и конечную точку линии
                XYZ startPoint = line.GetEndPoint(0);
                XYZ endPoint = line.GetEndPoint(1);

                // Сдвигаем обе точки на заданное смещение в направлении вектора
                XYZ newStartPoint = startPoint - direction * (offset * numberOfLayer);
                XYZ newEndPoint = endPoint - direction * (offset * numberOfLayer);

                // Создаем новую линию с обновленными точками
                Line newLine = Line.CreateBound(newStartPoint, newEndPoint);
                correctLines.Add(newLine);
            }
            return correctLines;
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
    }
}
