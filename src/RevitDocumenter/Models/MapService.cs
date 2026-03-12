using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using Color = Autodesk.Revit.DB.Color;
using Frame = Autodesk.Revit.DB.Frame;
using Point = System.Drawing.Point;
using Rectangle = System.Drawing.Rectangle;

namespace RevitDocumenter.Models;
internal class MapService {
    private readonly RevitRepository _revitRepository;

    private readonly double _mappingStep = 500.0;

    // Будет делаться тонкая розовая линия
    private readonly Color _colorForTestLines = new(255, 0, 255);
    private readonly int _weightForTestLines = 1;

    public MapService(RevitRepository revitRepository) {
        _revitRepository = revitRepository;
    }


    public void GetMap() {
        var doc = _revitRepository.Document;
        var view = doc.ActiveView;

        double revitStep = UnitUtilsHelper.ConvertToInternalValue(_mappingStep);

        (XYZ viewMinFixed, XYZ viewMaxFixed) = GetFixedCropBoxPoints(view);

        // Количество квадратов по Revit
        int stepCountX = (int) Math.Round((viewMaxFixed.X - viewMinFixed.X) / revitStep);  // 93
        int stepCountY = (int) Math.Round((viewMaxFixed.Y - viewMinFixed.Y) / revitStep);  // 65

        int standartX = 4096;
        int coefficient = standartX / stepCountX;

        int pixelsX = coefficient * stepCountX;
        int pixelsY = coefficient * stepCountY;



        CreateAnchorLines(view, viewMinFixed, viewMaxFixed);

        string imagePath = PrintViewByPixelSize(view, pixelsX);

        string croppedImagePath = CropImageByPinkPixels(imagePath);

        // допустим шаг revitStep = 400
        //(var x, var y) = GetImageDimensions(imagePath);

        // Теперь здесь подготовленное изображение, размеры которого точно кратны шагам в Revit
        string croppedScaledImagePath = ScaledImageByPixels(croppedImagePath, pixelsX, pixelsY);






        var map = AnalyzeImageSquares(croppedImagePath, stepCountX, stepCountY);

        MarkWhiteSquaresOnImage(croppedImagePath, map, stepCountX, stepCountY);







        using var subTransaction = new SubTransaction(doc);
        subTransaction.Start();

        subTransaction.Commit();

        //CreateSphere(viewMax);
        //CreateTestSpheres(viewMinFixed, viewMaxFixed);
    }


    private string ScaledImageByPixels(string imagePath, int targetWidth, int targetHeight) {
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





    public Bitmap[,] SplitImageIntoSquaresArrayFromBottomLeft(string imagePath, int stepCountX, int stepCountY) {
        using(var image = new Bitmap(imagePath)) {

            double pixelsInStepX = (double) image.Width / stepCountX;
            double pixelsInStepY = (double) image.Height / stepCountY;

            int pixelsInStepXRounded = (int) Math.Floor(pixelsInStepX);
            int pixelsInStepYRounded = (int) Math.Floor(pixelsInStepY);

            // Сколько пикселей на самом деле находится в шаге (стороне квадрата)
            int pixelsInStepRounded = pixelsInStepXRounded < pixelsInStepYRounded
                ? pixelsInStepXRounded
                : pixelsInStepYRounded;

            var grid = new Bitmap[stepCountY, stepCountX];

            for(int row = 0; row < stepCountY; row++) // row = 0 - нижний ряд
            {
                for(int col = 0; col < stepCountX; col++) {
                    var square = new Bitmap(pixelsInStepRounded, pixelsInStepRounded);

                    using(var graphics = Graphics.FromImage(square)) {
                        // Устанавливаем высокое качество
                        graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                        graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                        graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                        graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

                        var sourceRect = new System.Drawing.Rectangle(
                            col * pixelsInStepRounded,
                            image.Height - (row + 1) * pixelsInStepRounded,
                            pixelsInStepRounded,
                            pixelsInStepRounded
                        );

                        graphics.DrawImage(
                            image,
                            0, 0,
                            sourceRect,
                            GraphicsUnit.Pixel
                        );
                    }
                    grid[row, col] = square;
                    //square.Save(imagePath.Replace(".png", $"2-{row}-{col}.png"), System.Drawing.Imaging.ImageFormat.Png);
                }
            }
            return grid;
        }
    }


    private void GetMapFromPNG(string imagePath, double stepCountX, double stepCountY) {
        // Проверяем существование файла
        if(!File.Exists(imagePath)) {
            throw new FileNotFoundException(imagePath);
        }

        using var bitmap = new Bitmap(imagePath);

        double pixelsInStepX = Math.Floor(bitmap.Width / stepCountX);
        double pixelsInStepY = Math.Floor(bitmap.Height / stepCountY);

        TaskDialog.Show("fd", $"{pixelsInStepX} {pixelsInStepY}");







        //for(int row = 0; row < pixelsInStepY; row++) {
        //    for(int col = 0; col < pixelsInStepX; col++) {
        //        // Создаем новый квадрат
        //        Bitmap square = new Bitmap(squareSize, squareSize);

        //        using(Graphics graphics = Graphics.FromImage(square)) {
        //            // Вырезаем соответствующую область из оригинального изображения
        //            Rectangle sourceRect = new Rectangle(
        //                col * squareSize,
        //                row * squareSize,
        //                squareSize,
        //                squareSize
        //            );

        //            graphics.DrawImage(
        //                originalImage,
        //                0, 0, sourceRect, GraphicsUnit.Pixel
        //            );
        //        }

        //        squares.Add(square);
        //    }
        //}
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





    private (XYZ, XYZ) GetFixedCropBoxPoints(View view) {
        var viewMax = ProjectPointToViewPlan(view, view.CropBox.Max);
        var viewMin = ProjectPointToViewPlan(view, view.CropBox.Min);

        double step = UnitUtilsHelper.ConvertToInternalValue(_mappingStep);

        double deltaX = viewMax.X - viewMin.X;
        double deltaY = viewMax.Y - viewMin.Y;

        double halfRemainderX = (deltaX % step) / 2;
        double halfRemainderY = (deltaY % step) / 2;

        var viewMaxFixed = new XYZ(viewMax.X - halfRemainderX, viewMax.Y - halfRemainderY, viewMax.Z);
        var viewMinFixed = new XYZ(viewMin.X + halfRemainderX, viewMin.Y + halfRemainderY, viewMin.Z);

        return (viewMinFixed, viewMaxFixed);
    }


    private void CreateAnchorLines(View view, XYZ viewMinFixed, XYZ viewMaxFixed) {
        var lineGeom1 = Line.CreateBound(viewMinFixed, viewMinFixed + XYZ.BasisX);
        var lineGeom2 = Line.CreateBound(viewMaxFixed, viewMaxFixed - XYZ.BasisX);

        var overrideSettings = new OverrideGraphicSettings();
        overrideSettings.SetProjectionLineWeight(_weightForTestLines);
        overrideSettings.SetProjectionLineColor(_colorForTestLines);

        var doc = _revitRepository.Document;
        using var subTransaction = new SubTransaction(doc);
        subTransaction.Start();

        var detailLine1 = doc.Create.NewDetailCurve(view, lineGeom1);
        var detailLine2 = doc.Create.NewDetailCurve(view, lineGeom2);

        view.SetElementOverrides(detailLine1.Id, overrideSettings);
        view.SetElementOverrides(detailLine2.Id, overrideSettings);
        subTransaction.Commit();
    }



    private (int width, int height) GetImageDimensions(string imagePath) {
        // Проверяем существование файла
        if(!File.Exists(imagePath)) {
            throw new FileNotFoundException(imagePath);
        }

        using var bitmap = new Bitmap(imagePath);

        int width = bitmap.Width;
        int height = bitmap.Height;

        //TaskDialog.Show("GetImageDimensions", $"{width}x{height}");

        return (width, height);
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
            _revitRepository.Document.ExportImage(options);

            return desktopPath + ImageExportOptions.GetFileName(_revitRepository.Document, view.Id) + ".png";
        } catch(Exception) {
            return string.Empty;
        }
    }

    private string PrintViewByZoom(View view) {
        try {
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\";
            var options = new ImageExportOptions {
                FilePath = desktopPath,
                ZoomType = ZoomFitType.Zoom,
                Zoom = 50,
                ImageResolution = ImageResolution.DPI_600,
                HLRandWFViewsFileType = ImageFileType.PNG,
                ShadowViewsFileType = ImageFileType.PNG,
                ExportRange = ExportRange.SetOfViews,
            };
            options.SetViewsAndSheets([view.Id]);
            _revitRepository.Document.ExportImage(options);

            return desktopPath + ImageExportOptions.GetFileName(_revitRepository.Document, view.Id) + ".png";
        } catch(Exception) {
            return string.Empty;
        }
    }

    public XYZ ProjectPointToViewPlan(View view, XYZ point) {
        var pointOnViewPlan = new XYZ(0, 0, view.GenLevel.Elevation);
        var normal = view.ViewDirection.Normalize();

        // Вычисляем вектор от точки на плоскости к целевой точке
        var vector = point - pointOnViewPlan;

        // Находим расстояние вдоль нормали (скалярное произведение)
        double distance = normal.DotProduct(vector);

        // Проецируем точку на плоскость
        return point - distance * normal;
    }

    private void CreateTestSpheres(XYZ viewMinFixed, XYZ viewMaxFixed) {
        double tX = viewMaxFixed.X - viewMinFixed.X;
        double tY = viewMaxFixed.Y - viewMinFixed.Y;

        double step = UnitUtilsHelper.ConvertToInternalValue(_mappingStep);

        double countX = tX / step;
        double countY = tY / step;

        for(int i = 0; i <= countX; i++) {
            for(int j = 0; j <= countY; j++) {
                var ptForTest = viewMinFixed + new XYZ(step * i, step * j, 0);
                CreateSphere(ptForTest);
            }
        }
    }

    private void CreateSphere(XYZ center) {
        List<Curve> profile = new List<Curve>();

        // first create sphere with 0.5' radius
        double radius = 0.5;
        XYZ profile00 = center;
        XYZ profilePlus = center + new XYZ(0, radius, 0);
        XYZ profileMinus = center - new XYZ(0, radius, 0);

        profile.Add(Line.CreateBound(profilePlus, profileMinus));
        profile.Add(Arc.Create(profileMinus, profilePlus, center + new XYZ(radius, 0, 0)));

        CurveLoop curveLoop = CurveLoop.Create(profile);
        SolidOptions options = new SolidOptions(ElementId.InvalidElementId, ElementId.InvalidElementId);

        var doc = _revitRepository.Document;

        Frame frame = new Frame(center, XYZ.BasisX, -XYZ.BasisZ, XYZ.BasisY);
        if(Frame.CanDefineRevitGeometry(frame) == true) {
            Solid sphere = GeometryCreationUtilities.CreateRevolvedGeometry(frame, new CurveLoop[] { curveLoop }, 0, 2 * Math.PI, options);

            DirectShape ds = DirectShape.CreateElement(doc, new ElementId(BuiltInCategory.OST_GenericModel));

            ds.SetShape(new GeometryObject[] { sphere });
        }
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
}


public class SquareInfo {
    public bool AllPixelsWhite { get; set; }      // true, если ВСЕ пиксели в квадрате белые
    public Point StartPoint { get; set; }         // Начальная точка квадрата в координатах ВСЕГО изображения
    public Point EndPoint { get; set; }           // Конечная точка квадрата в координатах ВСЕГО изображения
}
