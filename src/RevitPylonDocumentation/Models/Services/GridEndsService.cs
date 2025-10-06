using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitPylonDocumentation.Models.PluginOptions;

namespace RevitPylonDocumentation.Models.Services;
internal class GridEndsService {
    private readonly View _view;
    private readonly DimensionBaseService _dimensionBaseService;

    public GridEndsService(View view, DimensionBaseService dimensionBaseService) {
        _view = view;
        _dimensionBaseService = dimensionBaseService;
    }

    internal void EditGridEnds(Element elem, List<Grid> grids, OffsetOption offsetOption) {
        if(_view is null || elem is null) { return; }
        var rightDirection = _view.RightDirection;

        foreach(var grid in grids) {
            var gridLine = grid.Curve as Line;
            var gridDir = gridLine.Direction;

            if(rightDirection.IsAlmostEqualTo(gridDir)
                || rightDirection.IsAlmostEqualTo(gridDir.Negate())) {

                var curve = grid.GetCurvesInView(DatumExtentType.ViewSpecific, _view).First();

                var offsetLine1 = _dimensionBaseService.GetDimensionLine(elem, DirectionType.Left,
                                                                         offsetOption.LeftOffset);
                var pt1 = curve.Project(offsetLine1.Origin).XYZPoint;

                var offsetLine2 = _dimensionBaseService.GetDimensionLine(elem, DirectionType.Right,
                                                                         offsetOption.RightOffset);
                var pt2 = curve.Project(offsetLine2.Origin).XYZPoint;

                var newLine = Line.CreateBound(pt1, pt2);
                grid.SetCurveInView(DatumExtentType.ViewSpecific, _view, newLine);

            } else {
                var curve = grid.GetCurvesInView(DatumExtentType.ViewSpecific, _view).First();

                var offsetLine1 = _dimensionBaseService.GetDimensionLine(elem, DirectionType.Bottom,
                                                                         offsetOption.BottomOffset);
                var pt1 = curve.Project(offsetLine1.Origin).XYZPoint;

                var offsetLine2 = _dimensionBaseService.GetDimensionLine(elem, DirectionType.Top,
                                                                         offsetOption.TopOffset);
                var pt2 = curve.Project(offsetLine2.Origin).XYZPoint;

                var newLine = Line.CreateBound(pt1, pt2);
                grid.SetCurveInView(DatumExtentType.ViewSpecific, _view, newLine);
            }
        }
    }
}
