using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;

using Autodesk.Revit.DB;

using RevitDocumenter.Models.Mapping.ViewServices;

using Point = System.Drawing.Point;
using Rectangle = System.Drawing.Rectangle;

namespace RevitDocumenter.Models.MapServices;
internal class ViewMapService {
    public ViewMapService() { }

    public MapInfo CreateMap(string path, ExportOption exportOption) {
        var map = AnalyzeImageSquares(path, exportOption.StepCountX, exportOption.StepCountY);
        return new MapInfo(
            map,
            path,
            exportOption.MappingStepInFeet,
            exportOption.StepCountX,
            exportOption.StepCountY,
            exportOption.StartPointInRevit);
    }

    public SquareInfo[,] AnalyzeImageSquares(string imagePath, int stepCountX, int stepCountY) {
        using(var image = new Bitmap(imagePath)) {
            // Блокируем биты исходного изображения для быстрого доступа
            var imageData = image.LockBits(
                new Rectangle(0, 0, image.Width, image.Height),
                ImageLockMode.ReadOnly,
                PixelFormat.Format32bppArgb);

            int stride = imageData.Stride;
            byte[] imagePixels = new byte[stride * image.Height];
            Marshal.Copy(imageData.Scan0, imagePixels, 0, imagePixels.Length);
            image.UnlockBits(imageData);

            // Вычисляем размеры квадратов
            int squareSize = Math.Min(image.Width / stepCountX, image.Height / stepCountY);

            // Результирующий массив с информацией о квадратах
            var results = new SquareInfo[stepCountY, stepCountX];

            for(int row = 0; row < stepCountY; row++) {
                for(int col = 0; col < stepCountX; col++) {
                    int startX = col * squareSize;
                    int startY = image.Height - (row + 1) * squareSize;

                    var info = AnalyzeSquare(
                        imagePixels,
                        stride,
                        startX,
                        startY,
                        squareSize,
                        image.Width,
                        image.Height);

                    results[row, col] = info;
                }
            }
            return results;
        }
    }

    private SquareInfo AnalyzeSquare(byte[] pixels, int stride, int startX, int startY, int size, int imageWidth, int imageHeight) {
        // Корректируем границы, если квадрат выходит за пределы изображения
        int endX = Math.Min(startX + size, imageWidth);
        int endY = Math.Min(startY + size, imageHeight);

        bool allPixelsWhite = true;  // Предполагаем, что все пиксели белые, пока не найдем не-белый
        const int whiteThreshold = 240;

        // Проходим по всем пикселям квадрата
        // Выходим при обнаружении первого не-белого
        for(int y = startY; y < endY && allPixelsWhite; y++) {
            for(int x = startX; x < endX && allPixelsWhite; x++) {
                int index = y * stride + x * 4;

                // Проверяем, является ли пиксель белым R, G, B
                bool isWhite = pixels[index + 2] >= whiteThreshold &&
                              pixels[index + 1] >= whiteThreshold &&
                              pixels[index] >= whiteThreshold;
                if(!isWhite) {
                    // Нашли не-белый пиксель
                    allPixelsWhite = false;
                }
            }
        }

        // Создаем и возвращаем результат с координатами в системе всего изображения
        return new SquareInfo {
            AllPixelsWhite = allPixelsWhite,
            // Левый нижний угол квадрата
            StartPoint = new Point(startX, startY),
            // Правый верхний угол квадрата
            EndPoint = new Point(endX - 1, endY - 1)
        };
    }

    /// <summary>
    /// Анализирует квадраты между точками на наличие белых групп пикселей
    /// </summary>
    public bool CheckInRectangle(MapInfo mapInfo, XYZ point1, XYZ point2) {
        // Переводим координаты точек в Revit в индексы квадратов на карте
        (int indexX1, int indexY1) = GetMapIndexes(mapInfo, point1);
        (int indexX2, int indexY2) = GetMapIndexes(mapInfo, point2);

        // Определяем границы прямоугольника
        int minX = Math.Min(indexX1, indexX2);
        int maxX = Math.Max(indexX1, indexX2);
        int minY = Math.Min(indexY1, indexY2);
        int maxY = Math.Max(indexY1, indexY2);

        // Проходим по всем SquareInfo в прямоугольнике
        for(int x = minX; x <= maxX; x++) {
            for(int y = minY; y <= maxY; y++) {
                if(!mapInfo.Map[y, x].AllPixelsWhite) {
                    // Нашли не белый квадрат
                    return false;
                }
            }
        }
        return true;
    }

    /// <summary>
    /// Анализирует квадраты по центральной точке квадрата и отступам на наличие белых групп пикселей
    /// </summary>
    public bool CheckInRectangle(MapInfo mapInfo, XYZ point, int offset) {
        // Переводим координаты точек в Revit в индексы квадратов на карте
        (int indexX, int indexY) = GetMapIndexes(mapInfo, point);

        // Определяем границы прямоугольника
        int minX = indexX - offset;
        int maxX = indexX + offset;
        int minY = indexY - offset;
        int maxY = indexY + offset;

        // Проходим по всем SquareInfo в прямоугольнике
        for(int x = minX; x <= maxX; x++) {
            for(int y = minY; y <= maxY; y++) {
                if(!mapInfo.Map[y, x].AllPixelsWhite) {
                    // Нашли не белый квадрат
                    return false;
                }
            }
        }
        return true;
    }

    public (int indexX, int indexY) GetMapIndexes(MapInfo mapInfo, XYZ point) {
        var difVector = point - mapInfo.StartPointInRevit;
        double x = difVector.X;
        double y = difVector.Y;

        int indexX = (int) Math.Floor(x / mapInfo.MappingStepInFeet);
        int indexY = (int) Math.Floor(y / mapInfo.MappingStepInFeet);

        return (indexX, indexY);
    }

    public bool Check(MapInfo mapInfo, params XYZ[] points) {
        return points != null && points.All(point => IsWhiteSquare(mapInfo, point));
    }

    public bool IsWhiteSquare(MapInfo mapInfo, XYZ point) {
        var difVector = point - mapInfo.StartPointInRevit;
        double x = difVector.X;
        double y = difVector.Y;

        int stepsForX = (int) Math.Floor(x / mapInfo.MappingStepInFeet);
        int stepsForY = (int) Math.Floor(y / mapInfo.MappingStepInFeet);

        return mapInfo.Map[stepsForY, stepsForX].AllPixelsWhite;
    }


    public void PaintInRectangle(MapInfo mapInfo, XYZ point1, XYZ point2) {
        // Переводим координаты точек в Revit в индексы квадратов на карте
        (int indexX1, int indexY1) = GetMapIndexes(mapInfo, point1);
        (int indexX2, int indexY2) = GetMapIndexes(mapInfo, point2);

        // Определяем границы прямоугольника
        int minX = Math.Min(indexX1, indexX2);
        int maxX = Math.Max(indexX1, indexX2);
        int minY = Math.Min(indexY1, indexY2);
        int maxY = Math.Max(indexY1, indexY2);

        // Проходим по всем SquareInfo в прямоугольнике
        for(int x = minX; x <= maxX; x++) {
            for(int y = minY; y <= maxY; y++) {
                mapInfo.Map[y, x].AllPixelsWhite = false;
            }
        }
    }

    public void PaintInRectangle(MapInfo mapInfo, XYZ point, int offset) {
        // Переводим координаты точек в Revit в индексы квадратов на карте
        (int indexX, int indexY) = GetMapIndexes(mapInfo, point);

        // Определяем границы прямоугольника
        int minX = indexX - offset;
        int maxX = indexX + offset;
        int minY = indexY - offset;
        int maxY = indexY + offset;

        // Проходим по всем SquareInfo в прямоугольнике
        for(int x = minX; x <= maxX; x++) {
            for(int y = minY; y <= maxY; y++) {
                mapInfo.Map[y, x].AllPixelsWhite = false;
            }
        }
    }


    public void Paint(MapInfo mapInfo, params XYZ[] points) {
        points?.ToList().ForEach(point => PaintSquare(mapInfo, point));
    }

    public void PaintSquare(MapInfo mapInfo, XYZ point) {
        var difVector = point - mapInfo.StartPointInRevit;
        double x = difVector.X;
        double y = difVector.Y;

        int stepsForX = (int) Math.Floor(x / mapInfo.MappingStepInFeet);
        int stepsForY = (int) Math.Floor(y / mapInfo.MappingStepInFeet);

        mapInfo.Map[stepsForY, stepsForX].AllPixelsWhite = false;
    }
}
