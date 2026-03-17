using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitDocumenter.Models;
internal class ViewPreparer {
    private readonly RevitRepository _revitRepository;

    public ViewPreparer(RevitRepository revitRepository) {
        _revitRepository = revitRepository;
    }

    public void Prepare(ExportOption exportOption) {
        var view = _revitRepository.Document.ActiveView;
        // Получаем точки рамки подрезки вида, смещенные немного внутрь, чтобы расстояние 
        // между ними было кратно указанному шагу
        (var viewMinFixed, var viewMaxFixed) = GetFixedCropBoxPoints(view, exportOption.MappingStepInMm);

        exportOption.StartPointInRevit = viewMinFixed;
        exportOption.EndPointInRevit = viewMaxFixed;

        // Количество квадратов в среде Revit
        int stepCountX = (int) Math.Round((viewMaxFixed.X - viewMinFixed.X) / exportOption.MappingStepInFeet);
        int stepCountY = (int) Math.Round((viewMaxFixed.Y - viewMinFixed.Y) / exportOption.MappingStepInFeet);

        exportOption.StepCountX = stepCountX;
        exportOption.StepCountY = stepCountY;

        // Создаем якорные линии в пространстве Revit, которые будут использованы для сопоставления 
        // пространства Revit и изображения
        exportOption.AnchorLineIds =
            CreateAnchorLines(viewMinFixed, viewMaxFixed, exportOption.WeightForAnchorLines, exportOption.ColorForAnchorLines);
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

    private List<ElementId> CreateAnchorLines(XYZ viewMinFixed, XYZ viewMaxFixed, int lineWeight, Color lineColor) {
        var overrideSettings = new OverrideGraphicSettings();
        overrideSettings.SetProjectionLineWeight(lineWeight);
        overrideSettings.SetProjectionLineColor(lineColor);

        return [
            CreateLineWithOverrides(viewMinFixed, viewMinFixed + XYZ.BasisX, overrideSettings).Id,
            CreateLineWithOverrides(viewMaxFixed, viewMaxFixed - XYZ.BasisX, overrideSettings).Id
        ];
    }

    public DetailCurve CreateLineWithOverrides(XYZ pt1, XYZ pt2, OverrideGraphicSettings overrideSettings) {
        var doc = _revitRepository.Document;

        var lineGeom = Line.CreateBound(pt1, pt2);
        using var subTransaction = new SubTransaction(doc);
        subTransaction.Start();

        var detailLine = doc.Create.NewDetailCurve(doc.ActiveView, lineGeom);
        _revitRepository.Document.ActiveView.SetElementOverrides(detailLine.Id, overrideSettings);

        subTransaction.Commit();
        return detailLine;
    }
}
