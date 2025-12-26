using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitBuildCoordVolumes.Models.Geometry;
using RevitBuildCoordVolumes.Models.Interfaces;
using RevitBuildCoordVolumes.Models.Settings;

namespace RevitBuildCoordVolumes.Models;
internal class ExtrusionContourBuilder : IExtrusionBuilder {
    private readonly ISpatialElementDividerService _spatialElementDividerService;
    private readonly ISlabNormalizeService _slabNormalizeService;
    private readonly IColumnFactory _columnFactory;
    private readonly IGeomElementFactory _geomElementFactory;
    private readonly RevitRepository _revitRepository;
    private readonly BuildCoordVolumesSettings _settings;

    public ExtrusionContourBuilder(
        ISpatialElementDividerService spatialElementDividerService,
        ISlabNormalizeService slabNormalizeService,
        IColumnFactory columnFactory,
        IGeomElementFactory geomElementFactory,
        RevitRepository revitRepository,
        BuildCoordVolumesSettings settings) {
        _spatialElementDividerService = spatialElementDividerService;
        _slabNormalizeService = slabNormalizeService;
        _columnFactory = columnFactory;
        _geomElementFactory = geomElementFactory;
        _revitRepository = revitRepository;
        _settings = settings;
    }

    public List<GeomElement> BuildVolumes(RevitArea revitArea) {
        // Разделяем зону на полигоны
        double sidePolygon = UnitUtils.ConvertToInternalUnits(_settings.SearchSide, UnitTypeId.Millimeters);
        var polygons = _spatialElementDividerService.DivideSpatialElement(revitArea.Area, sidePolygon);

        // Получаем все плиты перекрытия из настроек
        var allSlabs = _revitRepository.GetSlabsByTypesAndDocs(_settings.TypeSlabs, _settings.Documents).ToList();

        // Получаем все плиты с чистыми поверхностями
        var normalizedSlabs = _slabNormalizeService.GetNormalizeSlabs(allSlabs);

        // Строим группы колонн  
        var columnGroups = _columnFactory.GenerateColumnGroups(polygons, normalizedSlabs);

        // Получаем ориентацию полигона для Transform
        double spatialElementPosition = polygons[0].Sides[0].GetEndPoint(0).Z;

        var geomElements = new List<GeomElement>();
        foreach(var columnGroup in columnGroups) {
            var listColumns = columnGroup.ToList();
            var firstElement = listColumns[0];

            var element = firstElement.IsSloped || listColumns.Count == 1
                ? _geomElementFactory.GetSeparatedGeomElement(listColumns, spatialElementPosition)
                : _geomElementFactory.GetUnitedGeomElement(listColumns, spatialElementPosition);

            if(element != null) {
                geomElements.Add(element);
            }
        }
        return geomElements;
    }
}
