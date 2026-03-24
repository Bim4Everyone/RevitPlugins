using Autodesk.Revit.DB;

using RevitDocumenter.Models.DimensionServices;

namespace RevitDocumenter.Models.ReferenceCollectors;
/// <summary>
/// Класс, который собирает референсы от объекта до осей
/// </summary>
internal class ReferenceToGridsCollector : IReferenceCollector<ReferenceToGridsCollectorContext> {
    private readonly LineBasedElementFilterService _lineBasedElementFilterService;
    private readonly ReferenceAnalizeService _referenceAnalizeService;
    private readonly RebarElementDimensionLineProvider _dimensionLineService;

    public ReferenceToGridsCollector(
        LineBasedElementFilterService lineBasedElementFilterService,
        ReferenceAnalizeService referenceAnalizeService,
        RebarElementDimensionLineProvider dimensionLineService) {

        _lineBasedElementFilterService = lineBasedElementFilterService;
        _referenceAnalizeService = referenceAnalizeService;
        _dimensionLineService = dimensionLineService;
    }

    /// <summary>
    /// Метод находит ближайшие референсы между элементами и осями
    /// </summary>
    /// <param name="context">Содержит информацию об осях, с которыми нужно работать, интересующем направлении осей
    /// и референсах элементов, от которых нужно отталкиваться</param>
    /// <returns>Массив референсов самых ближайших между референсами на плоскости в элементе и осями</returns>
    public ReferenceArray CollectReferences(ReferenceToGridsCollectorContext context) {
        var gridRefs = _lineBasedElementFilterService.GetGridReferencesByDirection(
            context.Grids,
            context.Direction);

        if(gridRefs.Count == 0)
            return null;

        var dimensionLine = _dimensionLineService.GetPerpendicularLine(XYZ.Zero, context.Direction);

        return _referenceAnalizeService.FindClosestReferencesByDimension(
            context.ElementReferences,
            gridRefs,
            dimensionLine);
    }
}
