using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitPylonDocumentation.Models.PluginOptions;
using RevitPylonDocumentation.Models.PylonSheetNView;

namespace RevitPylonDocumentation.Models.Services;
/// <summary>
/// Сервис для расчёта точек линий обрыва.
/// </summary>
internal class BreakLinePointsService {
    private readonly ViewPointsAnalyzerService _viewPointsAnalyzer;
    private readonly FloorAnalyzerService _floorAnalyzerService;
    private readonly PylonSheetInfo _sheetInfo;
    private readonly double _breakLinesOffsetX;
    private readonly double _breakLinesOffsetY;
    private readonly double _breakLinesOffsetYBottom;

    public BreakLinePointsService(ViewPointsAnalyzerService viewPointsAnalyzer, 
                                  FloorAnalyzerService floorAnalyzerService, PylonSheetInfo sheetInfo,
                                  double breakLinesOffsetX, double breakLinesOffsetY, double breakLinesOffsetYBottom) {
        _viewPointsAnalyzer = viewPointsAnalyzer;
        _floorAnalyzerService = floorAnalyzerService;
        _sheetInfo = sheetInfo;
        _breakLinesOffsetX = breakLinesOffsetX;
        _breakLinesOffsetY = breakLinesOffsetY;
        _breakLinesOffsetYBottom = breakLinesOffsetYBottom;
    }

    /// <summary>
    /// Получает точки для создания нижних линий обрыва
    /// </summary>
    public List<XYZ> GetBreakLinePointsForLowerLines(View view, bool isForPerpView) {
        var origin = _sheetInfo.ElemsInfo.HostOrigin;
        var viewMax = _viewPointsAnalyzer.ProjectPointToViewFront(view.CropBox.Transform.OfPoint(view.CropBox.Max));
        var viewMin = _viewPointsAnalyzer.ProjectPointToViewFront(view.CropBox.Transform.OfPoint(view.CropBox.Min));

        XYZ leftPoint, rightPoint;

        if(isForPerpView && _sheetInfo.RebarInfo.SkeletonParentRebarForParking) {
            rightPoint = _viewPointsAnalyzer.GetPointByDirection(viewMax, DirectionType.Left, _breakLinesOffsetX, 0);
            leftPoint = _viewPointsAnalyzer.GetPointByDirection(viewMin, DirectionType.Right, _breakLinesOffsetX, 0);
            rightPoint = _viewPointsAnalyzer.ProjectPointToHorizontalPlane(origin, rightPoint);
            leftPoint = _viewPointsAnalyzer.ProjectPointToHorizontalPlane(origin, leftPoint);
        } else {
            double horizOriginOffset = isForPerpView
                ? _sheetInfo.ElemsInfo.HostWidth * 0.5 + _breakLinesOffsetY
                : _sheetInfo.ElemsInfo.HostLength * 0.5 + _breakLinesOffsetY;

            var pylonRightMinPoint = _viewPointsAnalyzer.GetPointByDirection(origin, DirectionType.Right, 
                                                                             horizOriginOffset, 0);
            rightPoint = _viewPointsAnalyzer.ProjectPointToViewFront(pylonRightMinPoint);

            var pylonLeftMinPoint = _viewPointsAnalyzer.GetPointByDirection(origin, DirectionType.Left, 
                                                                            horizOriginOffset, 0);
            leftPoint = _viewPointsAnalyzer.ProjectPointToViewFront(pylonLeftMinPoint);
        }

        var point1 = leftPoint;
        var point2 = _viewPointsAnalyzer.GetPointByDirection(new XYZ(leftPoint.X, leftPoint.Y, viewMin.Z), 
                                                             DirectionType.Top, 
                                                             0, 
                                                             _breakLinesOffsetYBottom);
        var point3 = _viewPointsAnalyzer.GetPointByDirection(new XYZ(rightPoint.X, rightPoint.Y, viewMin.Z), 
                                                             DirectionType.Top, 
                                                             0, 
                                                             _breakLinesOffsetYBottom);
        var point4 = rightPoint;

        return [point1, point2, point3, point4];
    }

    /// <summary>
    /// Получает точки для создания верхних линий обрыва
    /// </summary>
    public List<XYZ> GetBreakLinePointsForUpperLines(View view) {
        var topElement = _sheetInfo.HostElems.Last();
        double pylonMaxZ = _viewPointsAnalyzer.ProjectPointToViewFront(topElement.get_BoundingBox(view).Max).Z;

        var viewMax = _viewPointsAnalyzer.ProjectPointToViewFront(view.CropBox.Transform.OfPoint(view.CropBox.Max));
        var viewMin = _viewPointsAnalyzer.ProjectPointToViewFront(view.CropBox.Transform.OfPoint(view.CropBox.Min));

        double lastFloorDepth = UnitUtilsHelper.ConvertToInternalValue(200);
        var lastFloor = _floorAnalyzerService.GetLastFloor();
        if(lastFloor != null) {
            lastFloorDepth = lastFloor.GetParamValue<double>(BuiltInParameter.FLOOR_ATTR_THICKNESS_PARAM);
        }

        var point1 = _viewPointsAnalyzer.GetPointByDirection(new XYZ(viewMax.X, viewMax.Y, pylonMaxZ), 
                                                             DirectionType.LeftBottom, 
                                                             _breakLinesOffsetX, 
                                                             0);
        var point2 = _viewPointsAnalyzer.GetPointByDirection(point1, 
                                                             DirectionType.Top, 
                                                             0, 
                                                             lastFloorDepth);
        var point4 = _viewPointsAnalyzer.GetPointByDirection(new XYZ(viewMin.X, viewMin.Y, pylonMaxZ), 
                                                             DirectionType.RightBottom, 
                                                             _breakLinesOffsetX, 
                                                             0);
        var point3 = _viewPointsAnalyzer.GetPointByDirection(point4, 
                                                             DirectionType.Top, 
                                                             0, 
                                                             lastFloorDepth);
        return [point1, point2, point3, point4];
    }

    /// <summary>
    /// Получает точки для создания средних линий обрыва
    /// </summary>
    public List<XYZ> GetBreakLinePointsForMiddleLines(View view, bool isForPerpView) {
        var points = new List<XYZ>();
        double horizOriginOffset = isForPerpView
            ? _sheetInfo.ElemsInfo.HostWidth * 0.5 + _breakLinesOffsetY
            : _sheetInfo.ElemsInfo.HostLength * 0.5 + _breakLinesOffsetY;

        var origin = _sheetInfo.ElemsInfo.HostOrigin;
        var pylonLeftMinPoint = _viewPointsAnalyzer.GetPointByDirection(origin, DirectionType.Left, 
                                                                        horizOriginOffset, 0);
        pylonLeftMinPoint = _viewPointsAnalyzer.ProjectPointToViewFront(pylonLeftMinPoint);

        var pylonRightMinPoint = _viewPointsAnalyzer.GetPointByDirection(origin, DirectionType.Right, 
                                                                         horizOriginOffset, 0);
        pylonRightMinPoint = _viewPointsAnalyzer.ProjectPointToViewFront(pylonRightMinPoint);

        var previousElement = _sheetInfo.HostElems.First();
        for(int i = 1; i < _sheetInfo.HostElems.Count; i++) {
            var currentElement = _sheetInfo.HostElems[i];
            double currentBbMinZ = currentElement.get_BoundingBox(view).Min.Z;
            double previousBbMaxZ = previousElement.get_BoundingBox(view).Max.Z;

            points.Add(new XYZ(pylonLeftMinPoint.X, pylonLeftMinPoint.Y, currentBbMinZ));
            points.Add(new XYZ(pylonLeftMinPoint.X, pylonLeftMinPoint.Y, previousBbMaxZ));
            points.Add(new XYZ(pylonRightMinPoint.X, pylonRightMinPoint.Y, previousBbMaxZ));
            points.Add(new XYZ(pylonRightMinPoint.X, pylonRightMinPoint.Y, currentBbMinZ));

            previousElement = currentElement;
        }
        return points;
    }
}
