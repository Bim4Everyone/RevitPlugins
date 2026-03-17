using System;
using System.Drawing;

using Autodesk.Revit.DB;

namespace RevitDocumenter.Models;
internal class ImageExporter {
    private readonly Document _doc;

    public ImageExporter(Document doc) {
        _doc = doc;
    }

    public string Export(ExportOption exportOption) {
        // Стандартное значение ширины изображения в пикселях, подходящее для обработки
        int standartX = 4096;
        int pixelPerSquare = standartX / exportOption.StepCountX;

        // Получаем точные значения изображения для анализа в пикселях в соответствии с Revit
        int pixelsX = pixelPerSquare * exportOption.StepCountX;
        int pixelsY = pixelPerSquare * exportOption.StepCountY;

        // Экспортируем вид в изображение, задавая желаемую ширину в пикселях
        string imagePath = PrintViewByPixelSize(_doc.ActiveView, pixelsX);

        // Подрезаем изображение по якорям и сохраняем
        string croppedImagePath = CropImageByPinkPixels(imagePath);

        // Масштабируем изображение под нужный размер в пикселях, чтобы шаги соответствовали Revit
        return ScaledImageByPixels(croppedImagePath, pixelsX, pixelsY);
    }


    public string PrintViewByPixelSize(View view, int pixelSize) {
        try {
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\";
            var options = new ImageExportOptions {
                FilePath = desktopPath,
                PixelSize = pixelSize,
                FitDirection = FitDirectionType.Horizontal,
                ImageResolution = ImageResolution.DPI_600,
                HLRandWFViewsFileType = ImageFileType.PNG,
                ShadowViewsFileType = ImageFileType.PNG,
                ExportRange = ExportRange.SetOfViews,
            };
            options.SetViewsAndSheets([view.Id]);
            _doc.ExportImage(options);

            return desktopPath + ImageExportOptions.GetFileName(_doc, view.Id) + ".png";
        } catch(Exception) {
            return string.Empty;
        }
    }


    public string CropImageByPinkPixels(string imagePath) {
        // Загружаем изображение
        using var image = new Bitmap(imagePath);
        // Находим координаты розовых пикселей
        (int startX, int startY) = FindBottomLeftPinkPixel(image);
        (int endX, int endY) = FindTopRightPinkPixel(image);

        // Вычисляем размеры новой области
        int newWidth = endX - startX + 1;
        int newHeight = startY - endY + 1;

        var cropRect = new System.Drawing.Rectangle(startX, endY, newWidth, newHeight);
        Bitmap croppedImage = image.Clone(cropRect, image.PixelFormat);

        croppedImage.Save(imagePath.Replace(".png", "1.png"), System.Drawing.Imaging.ImageFormat.Png);
        return imagePath.Replace(".png", "1.png");
    }

    private (int, int) FindBottomLeftPinkPixel(Bitmap image) {
        // Ищем снизу вверх, слева направо
        for(int y = image.Height - 1; y >= 0; y--) {
            for(int x = 0; x < image.Width; x++) {
                var color = image.GetPixel(x, y);
                if(IsPink(color)) {
                    return (x, y);
                }
            }
        }
        throw new InvalidOperationException("Pink pixel not found!");
    }

    private (int, int) FindTopRightPinkPixel(Bitmap image) {
        // Ищем сверху вниз, справа налево
        for(int y = 0; y < image.Height; y++) {
            for(int x = image.Width - 1; x >= 0; x--) {
                var pixel = image.GetPixel(x, y);
                if(IsPink(pixel)) {
                    return (x, y);
                }
            }
        }
        throw new InvalidOperationException("Pink pixel not found!");
    }

    private bool IsPink(System.Drawing.Color color) {
        return color.R == 255 && color.G == 0 && color.B == 255;
    }


    public string ScaledImageByPixels(string imagePath, int targetWidth, int targetHeight) {
        using(var image = new Bitmap(imagePath)) {
            using(Bitmap resizedImage = new Bitmap(targetWidth, targetHeight)) {
                using(Graphics graphics = Graphics.FromImage(resizedImage)) {
                    graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    graphics.DrawImage(image, 0, 0, targetWidth, targetHeight);
                }

                // Сохранение результата
                resizedImage.Save(imagePath.Replace(".png", $"2.png"), System.Drawing.Imaging.ImageFormat.Png);
            }
        }
        return imagePath.Replace(".png", $"2.png");
    }
}
