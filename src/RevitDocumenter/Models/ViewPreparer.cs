using System;

using Autodesk.Revit.DB;

namespace RevitDocumenter.Models;
internal class ViewPreparer {
    private readonly RevitRepository _revitRepository;

    public ViewPreparer(RevitRepository revitRepository) {
        _revitRepository = revitRepository;
    }

    public ExportOption Prepare(ViewPreparerOption viewPreparerOption, AnchorLineService anchorLinesManager) {
        var view = _revitRepository.Document.ActiveView;
        // Получаем точки рамки подрезки вида, смещенные немного внутрь, чтобы расстояние 
        // между ними было кратно указанному шагу
        (var viewMinFixed, var viewMaxFixed) = GetFixedCropBoxPoints(view, viewPreparerOption.MappingStepInMm);

        // Количество квадратов в среде Revit
        int stepCountX = (int) Math.Round((viewMaxFixed.X - viewMinFixed.X) / viewPreparerOption.MappingStepInFeet);
        int stepCountY = (int) Math.Round((viewMaxFixed.Y - viewMinFixed.Y) / viewPreparerOption.MappingStepInFeet);

        // Создаем якорные линии в пространстве Revit, которые будут использованы для сопоставления 
        // пространства Revit и изображения
        var anchorLineIds = anchorLinesManager.CreateAnchorLines(
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
            AnchorLineIds = anchorLineIds
        };
    }

    private (XYZ, XYZ) GetFixedCropBoxPoints(View view, double mappingStepInMm) {
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

    private XYZ ProjectPointToViewPlan(View view, XYZ point) {
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
