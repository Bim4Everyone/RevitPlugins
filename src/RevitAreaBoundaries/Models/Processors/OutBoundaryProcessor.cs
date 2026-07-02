using System.Linq;

using Autodesk.Revit.DB;

using RevitAreaBoundaries.Services;
using RevitAreaBoundaries.Settings;

namespace RevitAreaBoundaries.Models.Processors;

internal class OutBoundaryProcessor (
    RevitRepository revitRepository,
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
        var targetViews = areaBoundarySettings.TargetViews;
        foreach(var view in targetViews) {
            DrawBoundary(view);
        }
    }

    private void DrawBoundary(View view) {
        // Получение кривых сечения
        var sectionCurves = elementSectionService.GetSectionCurves(revitRepository.Document, view);

        if(sectionCurves.Count == 0) {
            return;
        }
        
        // Нормализация кривых сечения
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
        var targetCurves = coarseCells.SelectMany(cellsBoundaryService.GetBoundaryCurves).ToList();
        
        if(targetCurves.Count == 0) {
            return;
        }
        
         // Режем пересекающиеся линии
         var croppedCurves = curveDividerService.SplitCurvesAtIntersections(targetCurves);
         
         // Закрываем разрывы прямыми линиями до 20мм
         var closedCurves = curveRepairService.RepairContour(croppedCurves, 1,20);
        
         // Очистка списка от дублирующихся кривых
         var cleanCurves = curveRepairService.CleanDuplicateCurves(closedCurves
             .Select(x => x)
             .ToList());
        
         // Возвращаем только те, которые соединены, удаляя концы за 1 итерацию
         var connectedCurves = curveRepairService.GetCurvesConnectedByBothEnds(cleanCurves);
         
         // Мержим
         var mergedCurves = collinearLineMergeService.MergeConnectedCollinearLines(connectedCurves);
         
         // Контур
         var contour = freeEndsJoinService.JoinNearestFreeEndsSmart(mergedCurves, maxJoinDistanceMm: 500);
        
         //Строим границы зоны
         drawBoundaryService.DrawBoundaryOnView(view, contour);
    }
}
