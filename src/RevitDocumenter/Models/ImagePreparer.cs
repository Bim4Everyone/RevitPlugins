using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

using Autodesk.Revit.DB;

using Color = Autodesk.Revit.DB.Color;

namespace RevitDocumenter.Models;
internal class ImagePreparer {
    private readonly RevitRepository _revitRepository;

    public ImagePreparer(RevitRepository revitRepository) {
        _revitRepository = revitRepository;
    }


    public (XYZ, XYZ) GetFixedCropBoxPoints(View view, double mappingStepInMm) {
        var viewMax = ProjectPointToViewPlan(view, view.CropBox.Max);
        var viewMin = ProjectPointToViewPlan(view, view.CropBox.Min);

        double step = UnitUtilsHelper.ConvertToInternalValue(mappingStepInMm);

        double deltaX = viewMax.X - viewMin.X;
        double deltaY = viewMax.Y - viewMin.Y;

        double halfRemainderX = (deltaX % step) / 2;
        double halfRemainderY = (deltaY % step) / 2;

        var viewMaxFixed = new XYZ(viewMax.X - halfRemainderX, viewMax.Y - halfRemainderY, viewMax.Z);
        var viewMinFixed = new XYZ(viewMin.X + halfRemainderX, viewMin.Y + halfRemainderY, viewMin.Z);

        return (viewMinFixed, viewMaxFixed);
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


    public void CreateAnchorLines(View view, XYZ viewMinFixed, XYZ viewMaxFixed, int weightForTestLines, Color colorForTestLines) {
        var lineGeom1 = Line.CreateBound(viewMinFixed, viewMinFixed + XYZ.BasisX);
        var lineGeom2 = Line.CreateBound(viewMaxFixed, viewMaxFixed - XYZ.BasisX);

        var overrideSettings = new OverrideGraphicSettings();
        overrideSettings.SetProjectionLineWeight(weightForTestLines);
        overrideSettings.SetProjectionLineColor(colorForTestLines);

        var doc = _revitRepository.Document;
        using var subTransaction = new SubTransaction(doc);
        subTransaction.Start();

        var detailLine1 = doc.Create.NewDetailCurve(view, lineGeom1);
        var detailLine2 = doc.Create.NewDetailCurve(view, lineGeom2);

        view.SetElementOverrides(detailLine1.Id, overrideSettings);
        view.SetElementOverrides(detailLine2.Id, overrideSettings);
        subTransaction.Commit();
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
            _revitRepository.Document.ExportImage(options);

            return desktopPath + ImageExportOptions.GetFileName(_revitRepository.Document, view.Id) + ".png";
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


    public void CreateTestSpheres(XYZ viewMinFixed, XYZ viewMaxFixed, double mappingStepInMm) {
        double tX = viewMaxFixed.X - viewMinFixed.X;
        double tY = viewMaxFixed.Y - viewMinFixed.Y;

        double step = UnitUtilsHelper.ConvertToInternalValue(mappingStepInMm);

        double countX = tX / step;
        double countY = tY / step;

        for(int i = 0; i <= countX; i++) {
            for(int j = 0; j <= countY; j++) {
                var ptForTest = viewMinFixed + new XYZ(step * i, step * j, 0);
                CreateSphere(ptForTest);
            }
        }
    }

    public void CreateSphere(XYZ center) {
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
}
