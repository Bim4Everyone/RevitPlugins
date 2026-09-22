using System;

using Autodesk.Revit.DB;

namespace RevitDocumenter.Models.Mapping.ViewServices;
internal class ViewPreparer {
    /// <summary>
    /// Отступ якорных линий от границы области подрезки, в целых шагах карты.
    /// </summary>
    /// <remarks>
    /// Линия, лежащая на самой границе подрезки, срезается при экспорте вида в изображение,
    /// и тогда найти ее не получается. Отступ задан в шагах карты, поэтому анализируемая
    /// область остается кратной шагу.
    /// </remarks>
    private const int _anchorInsetInSteps = 2;

    private readonly RevitRepository _revitRepository;
    private readonly AnchorLineService _anchorLineService;

    public ViewPreparer(RevitRepository revitRepository, AnchorLineService anchorLineService) {
        _revitRepository = revitRepository.ThrowIfNull();
        _anchorLineService = anchorLineService.ThrowIfNull();
    }

    /// <summary>
    /// В методе подготавливается вид, определяются краевые точки, по ним строятся якорные линии, 
    /// вычисляется количество квадратов
    /// </summary>
    public ExportOption Prepare(ViewPreparerOption viewPreparerOption) {
        viewPreparerOption.ThrowIfNull();

        var view = _revitRepository.Document.ActiveView;
        // Получаем точки рамки подрезки вида, смещенные немного внутрь, чтобы расстояние 
        // между ними было кратно указанному шагу
        (var viewMinFixed, var viewMaxFixed) = GetFixedCropBoxPoints(view, viewPreparerOption.MappingStepInFeet);

        // Количество квадратов в среде Revit
        int stepCountX = (int) Math.Round((viewMaxFixed.X - viewMinFixed.X) / viewPreparerOption.MappingStepInFeet);
        int stepCountY = (int) Math.Round((viewMaxFixed.Y - viewMinFixed.Y) / viewPreparerOption.MappingStepInFeet);

        // Создаем якорные линии в пространстве Revit, которые будут использованы для сопоставления 
        // пространства Revit и изображения
        var anchorLineIds = _anchorLineService.CreateAnchorLines(
            viewMinFixed,
            viewMaxFixed,
            viewPreparerOption.WeightForAnchorLines,
            viewPreparerOption.ColorForAnchorLines);

        return new ExportOption {
            MappingStepInFeet = viewPreparerOption.MappingStepInFeet,
            ColorForAnchorLines = viewPreparerOption.ColorForAnchorLines,
            StartPointInRevit = viewMinFixed,
            EndPointInRevit = viewMaxFixed,
            StepCountX = stepCountX,
            StepCountY = stepCountY,
            AnchorLineIds = anchorLineIds,
            Diagnostics = GetDiagnostics(
                view,
                viewPreparerOption.MappingStepInFeet,
                viewMinFixed,
                viewMaxFixed,
                stepCountX,
                stepCountY)
        };
    }

    /// <summary>
    /// Собирает сводку по подготовке вида для сообщения об ошибке построения карты.
    /// </summary>
    /// <remarks>
    /// Диагностика носит временный характер, убирается вместе с настройкой алгоритма.
    /// Значения переведены в миллиметры: в футах их неудобно сверять с планом.
    /// </remarks>
    private string GetDiagnostics(
        View view,
        double mappingStepInFeet,
        XYZ viewMinFixed,
        XYZ viewMaxFixed,
        int stepCountX,
        int stepCountY) {

        view.ThrowIfNull();
        viewMinFixed.ThrowIfNull();
        viewMaxFixed.ThrowIfNull();

        // Отредактированный контур подрезки поддерживается не всяким видом
        string isShapeSet;
        try {
            isShapeSet = view.GetCropRegionShapeManager().ShapeSet.ToString();
        } catch(Exception) {
            isShapeSet = "неизвестно";
        }

        var cropBox = view.CropBox;
        return string.Join(Environment.NewLine, new[] {
            "Масштаб вида: 1:" + view.Scale,
            "Шаг карты: " + FormatValue(mappingStepInFeet) + " мм",
            "CropBox.Min: " + FormatPoint(cropBox.Min),
            "CropBox.Max: " + FormatPoint(cropBox.Max),
            "CropBox.Transform: единичный = " + cropBox.Transform.IsIdentity
                + ", начало " + FormatPoint(cropBox.Transform.Origin),
            "Контур подрезки отредактирован: " + isShapeSet,
            "Проекция Min: " + FormatPoint(ProjectPointToViewPlan(view, cropBox.Min)),
            "Проекция Max: " + FormatPoint(ProjectPointToViewPlan(view, cropBox.Max)),
            "Якорь Min: " + FormatPoint(viewMinFixed),
            "Якорь Max: " + FormatPoint(viewMaxFixed),
            "Клеток: X = " + stepCountX + ", Y = " + stepCountY
        });
    }

    private static string FormatPoint(XYZ point) {
        return "(" + FormatValue(point.X) + "; " + FormatValue(point.Y) + "; " + FormatValue(point.Z) + ")";
    }

    private static string FormatValue(double valueInFeet) {
        return Math.Round(UnitUtilsHelper.ConvertFromInternalValue(valueInFeet)).ToString();
    }

    private (XYZ, XYZ) GetFixedCropBoxPoints(View view, double mappingStepInFeet) {
        view.ThrowIfNull();
        mappingStepInFeet.ThrowIfLessOrEqualThan();

        // Без активной подрезки CropBox возвращает габариты всей модели: количество шагов
        // становится огромным, а изображение вырождается. Карту по такому виду строить нельзя
        if(!view.CropBoxActive) {
            throw new InvalidOperationException(
                "Для анализа вида требуется включенная область подрезки (Crop View).");
        }

        var viewMax = ProjectPointToViewPlan(view, view.CropBox.Max);
        var viewMin = ProjectPointToViewPlan(view, view.CropBox.Min);

        double deltaX = viewMax.X - viewMin.X;
        double deltaY = viewMax.Y - viewMin.Y;

        // Одного остатка от деления на шаг для отступа недостаточно: он может оказаться
        // сколь угодно близким к нулю, и тогда якорная линия ложится точно на границу
        // подрезки и срезается при экспорте. Добавленные целые шаги дают гарантированный
        // отступ, не нарушая кратности анализируемой области шагу
        double anchorInset = mappingStepInFeet * _anchorInsetInSteps;
        double insetX = deltaX % mappingStepInFeet / 2 + anchorInset;
        double insetY = deltaY % mappingStepInFeet / 2 + anchorInset;

        if(deltaX <= insetX * 2 || deltaY <= insetY * 2) {
            throw new InvalidOperationException(
                "Область подрезки вида слишком мала для анализа с текущим шагом карты.");
        }

        var viewMaxFixed = new XYZ(viewMax.X - insetX, viewMax.Y - insetY, viewMax.Z);
        var viewMinFixed = new XYZ(viewMin.X + insetX, viewMin.Y + insetY, viewMin.Z);

        return (viewMinFixed, viewMaxFixed);
    }

    private XYZ ProjectPointToViewPlan(View view, XYZ point) {
        view.ThrowIfNull();
        point.ThrowIfNull();

        var pointOnViewPlan = new XYZ(0, 0, view.GenLevel.Elevation);
        var normal = view.ViewDirection.Normalize();

        // Вычисляем вектор от точки на плоскости к целевой точке
        var vector = point - pointOnViewPlan;

        // Находим расстояние вдоль нормали (скалярное произведение)
        double distance = normal.DotProduct(vector);

        // Проецируем точку на плоскость
        return point - distance * normal;
    }
}
