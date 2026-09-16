using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

using Autodesk.Revit.DB;

using Color = Autodesk.Revit.DB.Color;
using Rectangle = System.Drawing.Rectangle;

namespace RevitDocumenter.Models.Mapping.ViewServices;
internal class ImageService {
    // Префикс временной папки, в которую выгружается изображение вида
    private const string _exportFolderPrefix = "RevitDocumenter_";
    // Допуск при поиске цвета якорных линий: экспорт сглаживает линии, и цвет по краям плывет
    private const int _colorTolerance = 24;
    // Допустимое отклонение пропорций подрезанного изображения от расчетных
    private const double _aspectTolerance = 0.1;

    private readonly RevitRepository _revitRepository;
    private readonly Document _doc;

    public ImageService(RevitRepository revitRepository) {
        _revitRepository = revitRepository.ThrowIfNull();
        _doc = revitRepository.Document;
    }

    /// <summary>
    /// Открывает изображение по пути при помощи стандартного приложения
    /// </summary>
    public void OpenImage(string imagePath) {
        if(!File.Exists(imagePath)) {
            return;
        }
        Process.Start(new ProcessStartInfo {
            FileName = imagePath,
            UseShellExecute = true
        });
    }

    /// <summary>
    /// Удаляет изображение по указанному пути вместе с временной папкой выгрузки
    /// </summary>
    public void Delete(string path) {
        if(string.IsNullOrEmpty(path) || !File.Exists(path)) {
            return;
        }
        string folder = Path.GetDirectoryName(path);
        File.Delete(path);

        // Временную папку выгрузки убираем за собой, чужие папки не трогаем
        if(folder != null
           && Path.GetFileName(folder).StartsWith(_exportFolderPrefix, StringComparison.Ordinal)
           && Directory.GetFileSystemEntries(folder).Length == 0) {
            Directory.Delete(folder);
        }
    }

    /// <summary>
    /// Экспортирует вид из Revit в формате png, обрезает по якорным линиям и подгоняет под нужный масштаб
    /// </summary>
    public string Export(ExportOption exportOption) {
        exportOption.ThrowIfNull();

        // Стандартное значение ширины изображения в пикселях, подходящее для обработки
        const int standardX = 4096;
        int pixelPerSquare = standardX / exportOption.StepCountX;

        // При слишком мелком шаге на клетку не остается ни одного пикселя и анализ теряет смысл
        if(pixelPerSquare < 1) {
            throw new InvalidOperationException(
                "Шаг анализа слишком мелкий для габаритов вида: на клетку карты не приходится ни одного пикселя.");
        }

        // Получаем точные значения изображения для анализа в пикселях в соответствии с Revit
        int pixelsX = pixelPerSquare * exportOption.StepCountX;
        int pixelsY = pixelPerSquare * exportOption.StepCountY;

        // Экспортируем вид в изображение, задавая желаемую ширину в пикселях
        string imagePath = PrintViewByPixelSize(_doc.ActiveView, pixelsX);

        try {
            // Подрезаем изображение по якорям и сохраняем
            CropImageByColorPixels(imagePath, exportOption.ColorForAnchorLines, pixelsX, pixelsY);

            // Масштабируем изображение под нужный размер в пикселях, чтобы шаги соответствовали Revit
            return ScaledImageByPixels(imagePath, pixelsX, pixelsY);
        } catch(Exception) {
            // Незавершенная выгрузка не должна оставаться во временной папке
            Delete(imagePath);
            throw;
        }
    }

    private string PrintViewByPixelSize(View view, int pixelSize) {
        view.ThrowIfNull();
        pixelSize.ThrowIfLessOrEqualThan();

        // Имя файла Revit берет от имени вида, поэтому каждая выгрузка идет в свою папку:
        // иначе два запуска подряд читали бы изображение друг друга
        string exportFolder = Path.Combine(Path.GetTempPath(), _exportFolderPrefix + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(exportFolder);

        try {
            var options = new ImageExportOptions {
                FilePath = exportFolder + Path.DirectorySeparatorChar,
                PixelSize = pixelSize,
                FitDirection = FitDirectionType.Horizontal,
                ImageResolution = ImageResolution.DPI_600,
                HLRandWFViewsFileType = ImageFileType.PNG,
                ShadowViewsFileType = ImageFileType.PNG,
                ExportRange = ExportRange.SetOfViews,
            };
            options.SetViewsAndSheets([view.Id]);
            _doc.ExportImage(options);
        } catch(Exception exception) {
            throw new InvalidOperationException("Не удалось выгрузить вид в изображение.", exception);
        }

        string imagePath = Path.Combine(exportFolder, ImageExportOptions.GetFileName(_doc, view.Id) + ".png");
        return File.Exists(imagePath)
            ? imagePath
            : throw new InvalidOperationException("Revit не создал файл изображения вида.");
    }

    /// <summary>
    /// Подрезает изображение по габаритам якорных линий, восстанавливая соответствие с координатами модели
    /// </summary>
    private string CropImageByColorPixels(string imagePath, Color colorForFind, int expectedWidth, int expectedHeight) {
        imagePath.ThrowIfNullOrEmpty();
        imagePath.ThrowIfFileNotExist();
        colorForFind.ThrowIfNull();

        byte[] imagePixels;
        int stride;
        int width;
        int height;
        PixelFormat pixelFormat;

        using(var image = new Bitmap(imagePath)) {
            width = image.Width;
            height = image.Height;
            pixelFormat = image.PixelFormat;

            // Чтение через LockBits вместо GetPixel: на изображении в 4096 пикселей
            // попиксельный доступ занимает десятки миллионов вызовов
            var imageData = image.LockBits(
                new Rectangle(0, 0, width, height),
                ImageLockMode.ReadOnly,
                PixelFormat.Format32bppArgb);

            stride = imageData.Stride;
            imagePixels = new byte[stride * height];
            Marshal.Copy(imageData.Scan0, imagePixels, 0, imagePixels.Length);
            image.UnlockBits(imageData);
        }

        (int minX, int minY, int maxX, int maxY) = FindColorBounds(imagePixels, stride, width, height, colorForFind);

        int newWidth = maxX - minX + 1;
        int newHeight = maxY - minY + 1;

        // Если пропорции подрезки разошлись с расчетными, якоря найдены неверно.
        // Молча растягивать такое изображение нельзя - вся привязка к модели уедет
        double expectedRatio = (double) expectedWidth / expectedHeight;
        double actualRatio = (double) newWidth / newHeight;
        if(Math.Abs(actualRatio - expectedRatio) > expectedRatio * _aspectTolerance) {
            throw new InvalidOperationException(
                "Пропорции подрезанного изображения не совпадают с габаритами вида: "
                + "якорные линии определены неверно.");
        }

        byte[] croppedBytes;
        using(var image = new Bitmap(imagePath)) {
            using var croppedImage = image.Clone(new Rectangle(minX, minY, newWidth, newHeight), pixelFormat);
            using var memoryStream = new MemoryStream();
            croppedImage.Save(memoryStream, ImageFormat.Png);
            croppedBytes = memoryStream.ToArray();
        }

        File.WriteAllBytes(imagePath, croppedBytes);
        return imagePath;
    }

    /// <summary>
    /// Находит габариты пикселей заданного цвета на изображении
    /// </summary>
    private (int minX, int minY, int maxX, int maxY) FindColorBounds(
        byte[] pixels,
        int stride,
        int width,
        int height,
        Color colorForFind) {

        pixels.ThrowIfNullOrEmpty();
        colorForFind.ThrowIfNull();

        int minX = int.MaxValue;
        int minY = int.MaxValue;
        int maxX = -1;
        int maxY = -1;

        for(int y = 0; y < height; y++) {
            int rowIndex = y * stride;
            for(int x = 0; x < width; x++) {
                int index = rowIndex + x * 4;

                if(!IsColor(pixels[index + 2], pixels[index + 1], pixels[index], colorForFind)) {
                    continue;
                }
                if(x < minX) {
                    minX = x;
                }
                if(x > maxX) {
                    maxX = x;
                }
                if(y < minY) {
                    minY = y;
                }
                if(y > maxY) {
                    maxY = y;
                }
            }
        }

        return maxX < 0
            ? throw new InvalidOperationException(
                "На изображении вида не найдены якорные линии. "
                + "Возможная причина - переопределение графики вида, из-за которого их цвет изменился.")
            : (minX, minY, maxX, maxY);
    }

    private bool IsColor(byte red, byte green, byte blue, Color colorForComparison) {
        colorForComparison.ThrowIfNull();
        return Math.Abs(red - colorForComparison.Red) <= _colorTolerance
            && Math.Abs(green - colorForComparison.Green) <= _colorTolerance
            && Math.Abs(blue - colorForComparison.Blue) <= _colorTolerance;
    }

    private string ScaledImageByPixels(string imagePath, int targetWidth, int targetHeight) {
        imagePath.ThrowIfNullOrEmpty();
        imagePath.ThrowIfFileNotExist();
        targetWidth.ThrowIfLessOrEqualThan();
        targetHeight.ThrowIfLessOrEqualThan();

        byte[] resizedBytes;
        using(var image = new Bitmap(imagePath)) {
            using var resizedImage = new Bitmap(targetWidth, targetHeight);
            using(var graphics = Graphics.FromImage(resizedImage)) {
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.DrawImage(image, 0, 0, targetWidth, targetHeight);
            }

            using var memoryStream = new MemoryStream();
            resizedImage.Save(memoryStream, ImageFormat.Png);
            resizedBytes = memoryStream.ToArray();
        }

        File.WriteAllBytes(imagePath, resizedBytes);
        return imagePath;
    }
}
