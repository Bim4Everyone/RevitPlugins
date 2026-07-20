using System.Diagnostics;
using System.Linq;
using System.Text;

using Autodesk.Revit.DB;

using RevitAreaBoundaries.Models.Enums;
using RevitAreaBoundaries.Services;
using RevitAreaBoundaries.Settings;

namespace RevitAreaBoundaries.Models.Processors;

internal class OutBoundaryProcessor (
    ElementSectionService elementSectionService, 
    CurveNormalizeService curveNormalizeService, 
    OuterSquareService outerSquareService,
    CurveDividerService curveDividerService,
    CellsBoundaryService cellsBoundaryService,
    CurveRepairService curveRepairService,
    DrawBoundaryService drawBoundaryService,
    CollinearLineMergeService collinearLineMergeService,
    FreeEndsJoinService freeEndsJoinService) : IBoundaryProcessor {
    

    public void DrawBoundaries(AreaBoundarySettings areaBoundarySettings, ProgressService progressService) {
        var targetViews = areaBoundarySettings.Views;
        progressService.AreaPlanCount = targetViews.Count.ToString();
        
        for(int i = 0; i < targetViews.Count; i++) {
            progressService.CancellationToken.ThrowIfCancellationRequested();
            progressService.AreaPlanNumber = (i + 1).ToString();
            var view = targetViews[i].Element as View;
            DrawBoundary(view, areaBoundarySettings, progressService);
        }
    }

    private void DrawBoundary(View view, AreaBoundarySettings areaBoundarySettings, ProgressService progressService) {
        // Получение кривых сечения
        var sectionCurves = elementSectionService.GetSectionCurves(view, areaBoundarySettings, progressService);

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
        var targetCurves = cellsBoundaryService.GetBoundaryCurves(coarseCells, progressService).ToList();
        
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
        drawBoundaryService.DrawBoundaryOnView(view, contour, progressService);
    }
}
