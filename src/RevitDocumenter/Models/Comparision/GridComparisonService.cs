using System;

using Autodesk.Revit.DB;

using RevitDocumenter.Models.DimensionServices;

namespace RevitDocumenter.Models.Comparision;
internal class GridComparisonService : IComparisonService {
    private readonly LineBasedElementFilterService _lineBasedElementFilterService;
    private readonly ReferenceAnalizeService _referenceAnalizeService;
    private readonly DimensionLineService _dimensionLineService;

    public GridComparisonService(
        LineBasedElementFilterService lineBasedElementFilterService,
        ReferenceAnalizeService referenceAnalizeService,
        DimensionLineService dimensionLineService) {

        _lineBasedElementFilterService = lineBasedElementFilterService;
        _referenceAnalizeService = referenceAnalizeService;
        _dimensionLineService = dimensionLineService;
    }

    public ReferenceArray Compare(IComparisonContext context) {
        var gridComparisonContext = context as GridComparisonContext
            ?? throw new ArgumentException("Invalid context type");

        var gridRefs = _lineBasedElementFilterService.GetGridReferencesByDirection(
            gridComparisonContext.Grids,
            gridComparisonContext.Direction);

        if(gridRefs.Count == 0)
            return null;

        var dimensionLine = _dimensionLineService.GetPerpendicularLine(XYZ.Zero, gridComparisonContext.Direction);

        return _referenceAnalizeService.FindClosestReferencesByDimension(
            gridComparisonContext.RebarReferences,
            gridRefs,
            dimensionLine);
    }
}
