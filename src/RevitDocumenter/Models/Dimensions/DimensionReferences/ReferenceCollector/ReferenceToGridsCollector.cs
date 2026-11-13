using Autodesk.Revit.DB;

using RevitDocumenter.Models.Dimensions.DimensionLines;

namespace RevitDocumenter.Models.Dimensions.DimensionReferences.ReferenceCollector;
/// <summary>
/// Класс, который собирает референсы от объекта до осей
/// </summary>
internal class ReferenceToGridsCollector : IReferenceCollector<ReferenceToGridsCollectorContext> {
    private readonly GridDirectionFilter _lineBasedElementFilterService;
    private readonly ReferenceAnalizeService _referenceAnalizeService;
    private readonly RebarZoneDimensionLineProvider _dimensionLineService;

    public ReferenceToGridsCollector(
        GridDirectionFilter lineBasedElementFilterService,
        ReferenceAnalizeService referenceAnalizeService,
        RebarZoneDimensionLineProvider dimensionLineService) {

        _lineBasedElementFilterService = lineBasedElementFilterService.ThrowIfNull();
        _referenceAnalizeService = referenceAnalizeService.ThrowIfNull();
        _dimensionLineService = dimensionLineService.ThrowIfNull();
    }

    /// <summary>
    /// Метод находит ближайшие референсы между элементами и осями
    /// </summary>
    /// <param name="context">Содержит информацию об осях, с которыми нужно работать, интересующем направлении осей
    /// и референсах элементов, от которых нужно отталкиваться</param>
    /// <returns>Массив референсов самых ближайших между референсами на плоскости в элементе и осями</returns>
    public ReferenceArray CollectReferences(ReferenceToGridsCollectorContext context) {
        context.ThrowIfNull();
        context.Grids.ThrowIfNull();
        context.Direction.ThrowIfNull();
        context.ElementReferences.ThrowIfNullOrEmpty();

        var gridRefs = _lineBasedElementFilterService.GetGridReferencesByDirection(
            context.Grids,
            context.Direction);

        if(gridRefs.Count == 0)
            return null;

        var dimensionLine = _dimensionLineService.GetPerpendicularLine(XYZ.Zero, context.Direction);

        return _referenceAnalizeService.FindClosestReferencesByDimension(
            context.ElementReferences,
            gridRefs,
            dimensionLine,
            context.MinDimension);
    }
}
