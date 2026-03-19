using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;

using Autodesk.Revit.DB;

using RevitDocumenter.Models.ViewServices;

using Point = System.Drawing.Point;
using Rectangle = System.Drawing.Rectangle;

namespace RevitDocumenter.Models.MapServices;
internal class ViewMapService {
    private SquareInfo[,] _map;
    private ExportOption _exportOption;

    public ViewMapService() { }

    public ExportOption ExportOption {
        get => _exportOption;
    }

    public SquareInfo[,] Map {
        get => _map;
    }

    public void CreateMap(string path, ExportOption exportOption) {
        _exportOption = exportOption;
        _map = AnalyzeImageSquares(path, exportOption.StepCountX, exportOption.StepCountY);
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


    public bool Check(params XYZ[] points) {
        return points != null && points.All(IsWhiteSquare);
    }

    public bool IsWhiteSquare(XYZ point) {
        var difVector = point - _exportOption.StartPointInRevit;
        double x = difVector.X;
        double y = difVector.Y;

        int stepsForX = (int) Math.Floor(x / _exportOption.MappingStepInFeet);
        int stepsForY = (int) Math.Floor(y / _exportOption.MappingStepInFeet);

        return _map[stepsForY, stepsForX].AllPixelsWhite;
    }

    public void Paint(params XYZ[] points) {
        points?.ToList().ForEach(PaintSquare);
    }

    public void PaintSquare(XYZ point) {
        var difVector = point - _exportOption.StartPointInRevit;
        double x = difVector.X;
        double y = difVector.Y;

        int stepsForX = (int) Math.Floor(x / _exportOption.MappingStepInFeet);
        int stepsForY = (int) Math.Floor(y / _exportOption.MappingStepInFeet);

        _map[stepsForY, stepsForX].AllPixelsWhite = false;
    }
}
