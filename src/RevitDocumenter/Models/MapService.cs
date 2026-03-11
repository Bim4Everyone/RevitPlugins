using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using Color = Autodesk.Revit.DB.Color;
using Frame = Autodesk.Revit.DB.Frame;

namespace RevitDocumenter.Models;
internal class MapService {
    private readonly RevitRepository _revitRepository;

    private readonly double _mappingStep = 5000.0;

    // Будет делаться тонкая розовая линия
    private readonly Color _colorForTestLines = new(255, 0, 255);
    private readonly int _weightForTestLines = 1;

    public MapService(RevitRepository revitRepository) {
        _revitRepository = revitRepository;
    }


    public void GetMap() {
        var doc = _revitRepository.Document;
        var view = doc.ActiveView;

        double step = UnitUtilsHelper.ConvertToInternalValue(_mappingStep);

        (XYZ viewMinFixed, XYZ viewMaxFixed) = GetFixedCropBoxPoints(view);

        double stepCountX = (viewMaxFixed.X - viewMinFixed.X) / step;
        double stepCountY = (viewMaxFixed.Y - viewMinFixed.Y) / step;

        CreateAnchorLines(view, viewMinFixed, viewMaxFixed);

        string imagePath = PrintViewByPixelSize(view, 4096);

        GetImageDimensions(imagePath);



        using var subTransaction = new SubTransaction(doc);
        subTransaction.Start();






        subTransaction.Commit();






        //CreateSphere(viewMax);
        //CreateTestSpheres(viewMinFixed, viewMaxFixed);
        //GetMapFromPNG(imagePath);
    }

    private void GetMapFromPNG(string imagePath) {
        // Проверяем существование файла
        if(!File.Exists(imagePath)) {
            throw new FileNotFoundException(imagePath);
        }

        int exportDpi = 600;

        using var bitmap = new Bitmap(imagePath);

        int actualWidth = bitmap.Width;
        int actualHeight = bitmap.Height;

        // Рассчитываем соотношение сторон
        double aspectRatio = (double) actualWidth / actualHeight;

        // Физические размеры в дюймах (используем DPI из экспорта)
        double widthInches = (double) actualWidth / exportDpi;
        double heightInches = (double) actualHeight / exportDpi;

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

        TaskDialog.Show("GetImageDimensions", $"{width}x{height}");

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

        for(int i = 1; i < countX; i++) {
            for(int j = 1; j < countY; j++) {
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
}
