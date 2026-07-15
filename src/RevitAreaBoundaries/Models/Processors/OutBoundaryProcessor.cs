using System.Diagnostics;
using System.Linq;
using System.Text;

using Autodesk.Revit.DB;

using RevitAreaBoundaries.Models.Enums;
using RevitAreaBoundaries.Services;
using RevitAreaBoundaries.Settings;

namespace RevitAreaBoundaries.Models.Processors;

internal class OutBoundaryProcessor (
    SystemPluginConfig systemPluginConfig,
    ElementSectionService elementSectionService, 
    CurveNormalizeService curveNormalizeService, 
    OuterSquareService outerSquareService,
    CurveDividerService curveDividerService,
    CellsBoundaryService cellsBoundaryService,
    CurveRepairService curveRepairService,
    DrawBoundaryService drawBoundaryService,
    CollinearLineMergeService collinearLineMergeService,
    FreeEndsJoinService freeEndsJoinService) : IBoundaryProcessor {
    

    public void DrawBoundaries(AreaBoundarySettings areaBoundarySettings) {
        var targetViews = areaBoundarySettings.Views;
        foreach(var revitElement in targetViews) {
            var view = revitElement.Element as View;
            DrawBoundary(view, areaBoundarySettings);
        }

        var o = systemPluginConfig.DefaultSectionHeightOffsetMm;
    }

    private void DrawBoundary(View view, AreaBoundarySettings areaBoundarySettings) {
        // Получение кривых сечения
        var sectionCurves = elementSectionService.GetSectionCurves(view, areaBoundarySettings);

        if(sectionCurves.Count == 0) {
            return;
        }
        
        // Перевод всех кривых в 0 по Z
        var normalizedCurves = curveNormalizeService.ProjectCurvesToXy(sectionCurves);
        
        if(normalizedCurves.Count == 0) {
            return;
        }
        
        // Построение ограничивающего квадрата их 4-х точек
        var outerVertex = outerSquareService.BuildOuterSquareVertices(normalizedCurves);
        
        // Деление линий на более короткие сегменты
        var dividedCurves = curveDividerService.DivideToShortCurves(normalizedCurves);
        
        // Получение грубых ячеек, проходящих по границе здания
        var coarseCells = cellsBoundaryService.GetCoarseCells(outerVertex, dividedCurves);
        
        if(coarseCells.Count == 0) {
            return;
        }
        
        // Получение кривых, максимально приближенных к наружней точке в каждой ячейке
        var targetCurves = coarseCells
            .SelectMany(cellsBoundaryService.GetBoundaryCurves)
            .ToList();
        
        if(targetCurves.Count == 0) {
            return;
        }
        
        // Режем пересекающиеся линии
        var croppedCurves = curveDividerService.SplitCurvesAtIntersections(targetCurves);
        
        // Закрываем разрывы прямыми линиями
        var closedCurves = curveRepairService.RepairContour(croppedCurves);
        
        // Очистка списка от дублирующихся кривых
        var cleanCurves = curveRepairService.CleanDuplicateCurves(closedCurves
            .Select(x => x)
            .ToList());
        
        // Возвращаем только те, которые соединены, удаляя концы за 1 итерацию
        var connectedCurves = curveRepairService.GetCurvesConnectedByBothEnds(cleanCurves);
        
        // Мержим в длинные границы коллинеарные отрезки
        var mergedCurves = collinearLineMergeService.MergeConnectedCollinearLines(connectedCurves);
       
        // Пытаемся соединить свободные концы
        var contour = freeEndsJoinService.JoinNearestFreeEndsSmart(mergedCurves);
        
        //Строим границы зоны
        drawBoundaryService.DrawBoundaryOnView(view, contour);
    }
}
