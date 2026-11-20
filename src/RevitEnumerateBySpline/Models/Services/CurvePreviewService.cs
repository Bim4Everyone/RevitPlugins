using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

using Autodesk.Revit.DB;

using RevitEnumerateBySpline.ViewModels;

namespace RevitEnumerateBySpline.Models.Services;

/// <summary>Временные номерные метки выбранных линий. Не создаёт элементы документа.</summary>
/// <param name="repository">Доступ к документу и активному виду Revit.</param>
/// <param name="systemPluginConfig">Системные настройки размещения меток.</param>
internal sealed class CurvePreviewService(RevitRepository repository, SystemPluginConfig systemPluginConfig) {
    // Шаг в единицах модели; точки считаются по длине, а не по параметру сплайна.
    private readonly double _markerStep =
        UnitUtils.ConvertToInternalUnits(systemPluginConfig.CurveMarkerStepMm, UnitTypeId.Millimeters);
    private readonly List<int> _controls = [];
    private readonly List<InCanvasControlData> _controlData = [];
    private TemporaryGraphicsManager? _manager;
    private readonly Dictionary<string, string> _images = [];
    private readonly string _directory = Path.Combine(Path.GetTempPath(), "RevitEnumerateBySpline", Guid.NewGuid().ToString("N"));

    /// <summary>Обновляет номера линий и пересоздаёт их метки и стрелки на активном виде.</summary>
    /// <param name="curves">Линии в порядке нумерации.</param>
    public void Update(IEnumerable<CurveModelViewModel> curves) {
        RemoveControls();
        _manager ??= TemporaryGraphicsManager.GetTemporaryGraphicsManager(repository.Document);
        int number = 0;
        foreach(var item in curves) {
            item.Number = ++number;
            if(item.CurveModel?.CurveElement?.GeometryCurve is not { IsBound: true } curve) {
                continue;
            }
            string key = $"{number}_{item.MarkerColor}";
            if(!_images.TryGetValue(key, out string path)) {
                Directory.CreateDirectory(_directory);
                path = Path.Combine(_directory, $"{number}_{item.MarkerColor.TrimStart('#')}.bmp");
                CreateImage(path, number, item.MarkerColor);
                _images.Add(key, path);
            }
            var points = curve.Tessellate();
            foreach(var position in GetMarkerPositions(points)) {
                AddControl(path, position);
            }

            var view = repository.Document.ActiveView;
            XYZ tangent = curve.ComputeDerivatives(0, true).BasisX;
            double x = tangent.DotProduct(view.RightDirection);
            double y = -tangent.DotProduct(view.UpDirection);
            if(x * x + y * y > 1e-12) {
                double angle = Math.Atan2(y, x);
                int degrees = (int) Math.Round(angle * 180 / Math.PI);
                string arrowKey = $"arrow_{degrees}";
                if(!_images.TryGetValue(arrowKey, out string arrowPath)) {
                    Directory.CreateDirectory(_directory);
                    arrowPath = Path.Combine(_directory, $"{arrowKey}.bmp");
                    CreateArrowImage(arrowPath, degrees * Math.PI / 180);
                    _images.Add(arrowKey, arrowPath);
                }
                AddControl(arrowPath, curve.GetEndPoint(0));
            }
        }
        repository.ActiveUiDocument.RefreshActiveView();
    }

    /// <summary>Добавляет временную графическую метку на активный вид.</summary>
    /// <param name="path">Путь к BMP-изображению метки.</param>
    /// <param name="position">Точка привязки метки в координатах модели.</param>
    private void AddControl(string path, XYZ position) {
        var data = new InCanvasControlData(path, position);
        // Данные живут столько же, сколько метка, включая повторный показ окна.
        _controlData.Add(data);
        int index = _manager!.AddControl(data, repository.Document.ActiveView.Id);
        _controls.Add(index);
        _manager.SetVisibility(index, true);
    }

    /// <summary>Распределяет метки вдоль аппроксимации кривой с шагом из конфигурации.</summary>
    /// <param name="points">Последовательные точки аппроксимации кривой.</param>
    /// <returns>Точки размещения меток в координатах модели.</returns>
    private IEnumerable<XYZ> GetMarkerPositions(IList<XYZ> points) {
        double length = 0;
        for(int i = 1; i < points.Count; i++) {
            length += points[i - 1].DistanceTo(points[i]);
        }
        // На короткой линии один кружок посередине, на длинной — с шагом из конфигурации.
        double next = Math.Min(_markerStep / 2, length / 2);
        double traversed = 0;
        for(int i = 1; i < points.Count; i++) {
            XYZ start = points[i - 1];
            XYZ edge = points[i] - start;
            double edgeLength = edge.GetLength();
            if(edgeLength < 1e-9) {
                continue;
            }
            while(next <= traversed + edgeLength) {
                yield return start + edge.Multiply((next - traversed) / edgeLength);
                next += _markerStep;
            }
            traversed += edgeLength;
        }
    }

    /// <summary>Сохраняет изображение стрелки, кончик которой совпадает с точкой привязки.</summary>
    /// <param name="path">Путь для сохранения BMP-изображения.</param>
    /// <param name="angle">Угол направления стрелки в плоскости экрана, в радианах.</param>
    private static void CreateArrowImage(string path, double angle) {
        using var bitmap = new Bitmap(80, 80, PixelFormat.Format24bppRgb);
        using var graphics = Graphics.FromImage(bitmap);
        graphics.Clear(System.Drawing.Color.FromArgb(0, 128, 128));
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        graphics.TranslateTransform(40, 40);
        graphics.RotateTransform((float) (angle * 180 / Math.PI));
        // Кончик стрелки совпадает с началом кривой, тело остаётся перед ней.
        // Сдвиг в пикселях не зависит от масштаба вида.
        graphics.TranslateTransform(-34, 0);
        using var halo = new Pen(System.Drawing.Color.White, 9) { StartCap = LineCap.Round };
        using var shaft = new Pen(System.Drawing.Color.Red, 6) { StartCap = LineCap.Round };
        graphics.DrawLine(halo, 0, 0, 23, 0);
        graphics.DrawLine(shaft, 0, 0, 23, 0);
        PointF[] head = [new(34, 0), new(19, -10), new(19, 10)];
        using var outline = new Pen(System.Drawing.Color.White, 2) { LineJoin = LineJoin.Round };
        graphics.FillPolygon(Brushes.Red, head);
        graphics.DrawPolygon(outline, head);
        bitmap.Save(path, ImageFormat.Bmp);
    }

    /// <summary>Удаляет предпросмотр, временные изображения и освобождает графические ресурсы.</summary>
    public void Clear() {
        RemoveControls();
        foreach(string path in _images.Values) {
            File.Delete(path);
        }
        _images.Clear();
        _manager?.Dispose();
        _manager = null;
        if(Directory.Exists(_directory)) {
            Directory.Delete(_directory);
        }
        repository.ActiveUiDocument.RefreshActiveView();
    }

    /// <summary>Удаляет метки с вида и освобождает их данные, сохраняя кэш изображений.</summary>
    private void RemoveControls() {
        foreach(int index in _controls) {
            _manager?.RemoveControl(index);
        }
        _controls.Clear();
        foreach(var data in _controlData) {
            data.Dispose();
        }
        _controlData.Clear();
    }

    /// <summary>Сохраняет изображение цветного кружка с номером линии.</summary>
    /// <param name="path">Путь для сохранения BMP-изображения.</param>
    /// <param name="number">Номер линии, отображаемый внутри кружка.</param>
    /// <param name="color">Цвет заливки в формате HTML.</param>
    private static void CreateImage(string path, int number, string color) {
        // Revit использует этот цвет BMP как прозрачный фон.
        using var bitmap = new Bitmap(30, 30, PixelFormat.Format24bppRgb);
        using var graphics = Graphics.FromImage(bitmap);
        graphics.Clear(System.Drawing.Color.FromArgb(0, 128, 128));
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        using var fill = new SolidBrush(ColorTranslator.FromHtml(color));
        using var outline = new Pen(System.Drawing.Color.White, 2);
        graphics.FillEllipse(fill, 3, 3, 24, 24);
        graphics.DrawEllipse(outline, 3, 3, 24, 24);
        using var font = new Font("Segoe UI", number < 100 ? 12 : 9, FontStyle.Bold, GraphicsUnit.Pixel);
        using var format = new StringFormat {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center
        };
        graphics.DrawString(number.ToString(), font, Brushes.White, new RectangleF(3, 3, 24, 24), format);
        bitmap.Save(path, ImageFormat.Bmp);
    }
}
