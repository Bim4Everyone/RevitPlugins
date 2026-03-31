using System.Drawing;

namespace RevitDocumenter.Models.Mapping.MapServices;
internal class PaintSquaresByMapService {
    public string MarkWhiteSquaresOnImage(MapInfo mapInfo, string imageFileSuffix) {
        mapInfo.ThrowIfNull();
        imageFileSuffix.ThrowIfNullOrEmpty();
        mapInfo.ImagePath.ThrowIfFileNotExist();

        using(var image = new Bitmap(mapInfo.ImagePath)) {

            using(var graphics = Graphics.FromImage(image)) {
                // Настраиваем качество отрисовки
                graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

                // Создаем розовую полупрозрачную кисть
                using(var brush = new SolidBrush(Color.FromArgb(128, 255, 192, 203))) {
                    // Проходим по всем квадратам
                    for(int row = 0; row < mapInfo.StepCountY; row++) {
                        for(int col = 0; col < mapInfo.StepCountX; col++) {
                            var square = mapInfo.Map[row, col];

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
            string newImagePath = mapInfo.ImagePath.Replace(".png", $"{imageFileSuffix}.png");
            image.Save(newImagePath, System.Drawing.Imaging.ImageFormat.Png);
            return newImagePath;
        }
    }
}
