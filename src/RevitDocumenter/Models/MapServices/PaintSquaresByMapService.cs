using System.Drawing;

namespace RevitDocumenter.Models.MapServices;
internal class PaintSquaresByMapService {
    public void MarkWhiteSquaresOnImage(string imagePath, SquareInfo[,] analysisResults, int stepCountX, int stepCountY) {
        using(var image = new Bitmap(imagePath)) {

            using(var graphics = Graphics.FromImage(image)) {
                // Настраиваем качество отрисовки
                graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

                // Создаем розовую полупрозрачную кисть
                using(var brush = new SolidBrush(System.Drawing.Color.FromArgb(128, 255, 192, 203))) {
                    // Проходим по всем квадратам
                    for(int row = 0; row < stepCountY; row++) {
                        for(int col = 0; col < stepCountX; col++) {
                            var square = analysisResults[row, col];

                            // Если все пиксели квадрата белые - закрашиваем его
                            if(square.AllPixelsWhite) {
                                // Создаем прямоугольник для заливки
                                var rect = new Rectangle(
                                    square.StartPoint.X,                            // X левого верхнего угла
                                    square.StartPoint.Y,                            // Y левого верхнего угла
                                    square.EndPoint.X - square.StartPoint.X + 1,    // Ширина
                                    square.EndPoint.Y - square.StartPoint.Y + 1     // Высота
                                );

                                // Закрашиваем прямоугольник розовым
                                graphics.FillRectangle(brush, rect);
                            }
                        }
                    }
                }
            }
            image.Save(imagePath.Replace(".png", $"_marked.png"), System.Drawing.Imaging.ImageFormat.Png);
        }
    }
}
