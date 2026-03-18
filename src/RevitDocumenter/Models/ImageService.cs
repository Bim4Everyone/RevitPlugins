using System;
using System.Drawing;
using System.IO;

using Autodesk.Revit.DB;

using Color = Autodesk.Revit.DB.Color;

namespace RevitDocumenter.Models;
internal class ImageService {
    private readonly RevitRepository _revitRepository;
    private readonly Document _doc;

    public ImageService(RevitRepository revitRepository) {
        _revitRepository = revitRepository;
        _doc = revitRepository.Document;
    }

    public void Delete(string path) {
        if(File.Exists(path)) {
            File.Delete(path);
        }
    }

    public string Export(ExportOption exportOption) {
        // Стандартное значение ширины изображения в пикселях, подходящее для обработки
        int standardX = 4096;
        int pixelPerSquare = standardX / exportOption.StepCountX;

        // Получаем точные значения изображения для анализа в пикселях в соответствии с Revit
        int pixelsX = pixelPerSquare * exportOption.StepCountX;
        int pixelsY = pixelPerSquare * exportOption.StepCountY;

        // Экспортируем вид в изображение, задавая желаемую ширину в пикселях
        string imagePath = PrintViewByPixelSize(_doc.ActiveView, pixelsX);

        // Подрезаем изображение по якорям и сохраняем
        string croppedImagePath = CropImageByColorPixels(imagePath, exportOption.ColorForAnchorLines);

        // Масштабируем изображение под нужный размер в пикселях, чтобы шаги соответствовали Revit
        return ScaledImageByPixels(croppedImagePath, pixelsX, pixelsY);
    }

    private string PrintViewByPixelSize(View view, int pixelSize) {
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

    private string CropImageByColorPixels(string imagePath, Color colorForFind) {
        // Загружаем изображение
        using var image = new Bitmap(imagePath);

        // Находим координаты пикселей
        (int startX, int startY) = FindBottomLeftColorPixel(image, colorForFind);
        (int endX, int endY) = FindTopRightColorPixel(image, colorForFind);

        // Вычисляем размеры новой области
        int newWidth = endX - startX + 1;
        int newHeight = startY - endY + 1;

        var cropRect = new System.Drawing.Rectangle(startX, endY, newWidth, newHeight);

        // Создаем обрезанное изображение
        using Bitmap croppedImage = image.Clone(cropRect, image.PixelFormat);

        // Сохраняем в MemoryStream
        byte[] imageBytes;
        using(var ms = new MemoryStream()) {
            croppedImage.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            imageBytes = ms.ToArray();
        }

        // Освобождаем ресурсы перед перезаписью
        image.Dispose();
        croppedImage.Dispose();

        // Перезаписываем оригинальный файл
        File.WriteAllBytes(imagePath, imageBytes);

        return imagePath;
    }

    private (int, int) FindBottomLeftColorPixel(Bitmap image, Color colorForFind) {
        // Ищем снизу вверх, слева направо
        for(int y = image.Height - 1; y >= 0; y--) {
            for(int x = 0; x < image.Width; x++) {
                var color = image.GetPixel(x, y);
                if(IsColor(color, colorForFind)) {
                    return (x, y);
                }
            }
        }
        throw new InvalidOperationException("Pink pixel not found!");
    }

    private (int, int) FindTopRightColorPixel(Bitmap image, Color colorForFind) {
        // Ищем сверху вниз, справа налево
        for(int y = 0; y < image.Height; y++) {
            for(int x = image.Width - 1; x >= 0; x--) {
                var pixel = image.GetPixel(x, y);
                if(IsColor(pixel, colorForFind)) {
                    return (x, y);
                }
            }
        }
        throw new InvalidOperationException("Pink pixel not found!");
    }

    private bool IsColor(System.Drawing.Color color, Color colorForComparison) {
        return
            color.R == colorForComparison.Red
            && color.G == colorForComparison.Green
            && color.B == colorForComparison.Blue;
    }

    private string ScaledImageByPixels(string imagePath, int targetWidth, int targetHeight) {
        using(var image = new Bitmap(imagePath)) {
            using(var resizedImage = new Bitmap(targetWidth, targetHeight)) {
                using(var graphics = Graphics.FromImage(resizedImage)) {
                    graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    graphics.DrawImage(image, 0, 0, targetWidth, targetHeight);
                }

                // Сохраняем в MemoryStream
                using(var ms = new MemoryStream()) {
                    resizedImage.Save(ms, System.Drawing.Imaging.ImageFormat.Png);

                    // Освобождаем ресурсы перед перезаписью
                    resizedImage.Dispose();
                    image.Dispose();

                    // Перезаписываем файл
                    File.WriteAllBytes(imagePath, ms.ToArray());
                }
            }
        }
        return imagePath;
    }
}
