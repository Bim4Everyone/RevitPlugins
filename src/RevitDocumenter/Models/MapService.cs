using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

using Autodesk.Revit.DB;

using Color = Autodesk.Revit.DB.Color;
using Point = System.Drawing.Point;
using Rectangle = System.Drawing.Rectangle;

namespace RevitDocumenter.Models;
internal class MapService {
    private readonly RevitRepository _revitRepository;
    private readonly double _mappingStepInMm = 200.0;

    private string _croppedScaledImagePath;
    private SquareInfo[,] _map;
    private int _stepCountX;
    private int _stepCountY;
    private XYZ _startPoint;
    private double _revitStep;

    // Будет делаться тонкая розовая линия
    private readonly Color _colorForTestLines = new(255, 0, 255);
    private readonly int _weightForTestLines = 1;

    public MapService(RevitRepository revitRepository) {
        _revitRepository = revitRepository;
    }

    public double RevitStep {
        get => _revitStep;
    }

    public (SquareInfo[,], XYZ, double) GetMap() {
        var doc = _revitRepository.Document;
        var view = doc.ActiveView;

        _revitStep = UnitUtilsHelper.ConvertToInternalValue(_mappingStepInMm);

        var imagePreparer = new ImagePreparer(_revitRepository);

        // Получаем точки рамки подрезки вида,смещенные немного внутрь,чтобы расстояние 
        // между ними было кратно указанному шагу
        (XYZ viewMinFixed, XYZ viewMaxFixed) = imagePreparer.GetFixedCropBoxPoints(view, _mappingStepInMm);

        // Количество квадратов в среде Revit
        int stepCountX = (int) Math.Round((viewMaxFixed.X - viewMinFixed.X) / _revitStep);  // 93 (при 500.0)
        int stepCountY = (int) Math.Round((viewMaxFixed.Y - viewMinFixed.Y) / _revitStep);  // 65 (при 500.0)

        // Стандартное значение ширины изображения в пикселях, подходящее для обработки
        int standartX = 4096;
        int pixelPerSquare = standartX / stepCountX;

        // Получаем точные значения изображения для анализа в пикселях в соответствии с Revit
        int pixelsX = pixelPerSquare * stepCountX;
        int pixelsY = pixelPerSquare * stepCountY;


        // Создаем якорные линии в пространстве Revit, которые будут использованы для сопоставления 
        // пространства Revit и изображения
        imagePreparer.CreateAnchorLines(view, viewMinFixed, viewMaxFixed, _weightForTestLines, _colorForTestLines);

        // Экспортируем вид в изображение, задавая желаемую ширину в пикселях
        string imagePath = imagePreparer.PrintViewByPixelSize(view, pixelsX);

        // Подрезаем изображение по якорям и сохраняем
        string croppedImagePath = imagePreparer.CropImageByPinkPixels(imagePath);

        // Масштабируем изображение под нужный размер в пикселях, чтобы шаги соответствовали Revit
        string croppedScaledImagePath = imagePreparer.ScaledImageByPixels(croppedImagePath, pixelsX, pixelsY);

        // По идее могли бы просто получить BB по всем объектам на виде и построить точки



        var map = AnalyzeImageSquares(croppedScaledImagePath, stepCountX, stepCountY);


        _croppedScaledImagePath = croppedScaledImagePath;
        _map = map;
        _stepCountX = stepCountX;
        _stepCountY = stepCountY;
        _startPoint = viewMinFixed;

        MarkWhiteSquaresOnImage(croppedScaledImagePath, map, stepCountX, stepCountY);


        using var subTransaction = new SubTransaction(doc);
        subTransaction.Start();

        subTransaction.Commit();

        //CreateSphere(viewMax);
        //CreateTestSpheres(viewMinFixed, viewMaxFixed);

        return (map, viewMinFixed, _mappingStepInMm);
    }


    public bool IsWhiteSquare(XYZ point) {

        XYZ difVector = point - _startPoint;
        double x = difVector.X;
        double y = difVector.Y;

        int stepsForX = (int) Math.Floor(x / _revitStep);
        int stepsForY = (int) Math.Floor(y / _revitStep);

        return _map[stepsForY, stepsForX].AllPixelsWhite;
    }






    public SquareInfo[,] AnalyzeImageSquares(string imagePath, int stepCountX, int stepCountY) {
        using(var image = new Bitmap(imagePath)) {
            // Блокируем биты исходного изображения для быстрого доступа
            BitmapData imageData = image.LockBits(
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
        for(int y = startY; y < endY && allPixelsWhite; y++)  // Выходим при обнаружении первого не-белого
        {
            for(int x = startX; x < endX && allPixelsWhite; x++) {
                int index = y * stride + x * 4;

                // Проверяем, является ли пиксель белым
                bool isWhite = pixels[index + 2] >= whiteThreshold && // R
                              pixels[index + 1] >= whiteThreshold && // G
                              pixels[index] >= whiteThreshold;      // B

                if(!isWhite) {
                    allPixelsWhite = false;  // Нашли не-белый пиксель
                                             // Немедленный выход из циклов благодаря условиям в for
                }
            }
        }

        // Создаем и возвращаем результат с координатами в системе всего изображения
        return new SquareInfo {
            AllPixelsWhite = allPixelsWhite,
            StartPoint = new Point(startX, startY),           // Левый нижний угол квадрата
            EndPoint = new Point(endX - 1, endY - 1)          // Правый верхний угол квадрата
        };
    }




    public void MarkWhiteSquaresOnImage(string imagePath, SquareInfo[,] analysisResults, int stepCountX, int stepCountY) {
        using(var image = new Bitmap(imagePath)) {

            using(var graphics = Graphics.FromImage(image)) {
                // Настраиваем качество отрисовки
                graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

                // Создаем розовую полупрозрачную кисть
                // Color.FromArgb(alpha, red, green, blue) - alpha 128 делает полупрозрачной
                using(var brush = new SolidBrush(System.Drawing.Color.FromArgb(128, 255, 192, 203))) // Полупрозрачный розовый
                {
                    // Проходим по всем квадратам
                    for(int row = 0; row < stepCountY; row++) {
                        for(int col = 0; col < stepCountX; col++) {
                            var square = analysisResults[row, col];

                            // Если все пиксели квадрата белые - закрашиваем его
                            if(square.AllPixelsWhite) {
                                // Создаем прямоугольник для заливки
                                var rect = new Rectangle(
                                    square.StartPoint.X,                    // X левого верхнего угла
                                    square.StartPoint.Y,                    // Y левого верхнего угла
                                    square.EndPoint.X - square.StartPoint.X + 1,  // Ширина
                                    square.EndPoint.Y - square.StartPoint.Y + 1   // Высота
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



    // Покраска нескольких квадратов по их координатам
    public void MarkWhiteSquaresOnImage(List<List<int>> list) {
        string imagePath = _croppedScaledImagePath;
        SquareInfo[,] analysisResults = _map;
        int stepCountX = _stepCountX;
        int stepCountY = _stepCountY;


        using(var image = new Bitmap(imagePath)) {

            using(var graphics = Graphics.FromImage(image)) {
                // Настраиваем качество отрисовки
                graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

                // Создаем розовую полупрозрачную кисть
                // Color.FromArgb(alpha, red, green, blue) - alpha 128 делает полупрозрачной
                using(var brush = new SolidBrush(System.Drawing.Color.FromArgb(128, 255, 192, 203))) // Полупрозрачный розовый
                {
                    // Проходим по всем квадратам
                    for(int row = 0; row < stepCountY; row++) {
                        for(int col = 0; col < stepCountX; col++) {
                            var square = analysisResults[row, col];

                            // Если все пиксели квадрата белые - закрашиваем его
                            if(square.AllPixelsWhite) {
                                // Создаем прямоугольник для заливки
                                var rect = new Rectangle(
                                    square.StartPoint.X,                    // X левого верхнего угла
                                    square.StartPoint.Y,                    // Y левого верхнего угла
                                    square.EndPoint.X - square.StartPoint.X + 1,  // Ширина
                                    square.EndPoint.Y - square.StartPoint.Y + 1   // Высота
                                );

                                // Закрашиваем прямоугольник розовым
                                graphics.FillRectangle(brush, rect);
                            }
                        }
                    }
                }

                foreach(List<int> item in list) {
                    var stepsForY = item[0];
                    var stepsForX = item[1];

                    using(var brush = new SolidBrush(System.Drawing.Color.FromArgb(255, 255, 255, 255))) {
                        var square = analysisResults[stepsForY, stepsForX];

                        // Создаем прямоугольник для заливки
                        var rect = new Rectangle(
                            square.StartPoint.X,                    // X левого верхнего угла
                            square.StartPoint.Y,                    // Y левого верхнего угла
                            square.EndPoint.X - square.StartPoint.X + 1,  // Ширина
                            square.EndPoint.Y - square.StartPoint.Y + 1   // Высота
                        );

                        graphics.FillRectangle(brush, rect);
                    }
                }
            }
            image.Save(imagePath.Replace(".png", $"_marked1.png"), System.Drawing.Imaging.ImageFormat.Png);
        }
    }
}


public class SquareInfo {
    public bool AllPixelsWhite { get; set; }      // true, если ВСЕ пиксели в квадрате белые
    public Point StartPoint { get; set; }         // Начальная точка квадрата в координатах ВСЕГО изображения
    public Point EndPoint { get; set; }           // Конечная точка квадрата в координатах ВСЕГО изображения
}
