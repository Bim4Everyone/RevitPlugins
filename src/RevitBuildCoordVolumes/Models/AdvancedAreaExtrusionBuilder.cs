using System.Collections.Generic;
using System.Linq;
using System.Windows;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitBuildCoordVolumes.Models.Geometry;
using RevitBuildCoordVolumes.Models.Interfaces;
using RevitBuildCoordVolumes.Models.Services;
using RevitBuildCoordVolumes.Models.Settings;

namespace RevitBuildCoordVolumes.Models;
internal class AdvancedAreaExtrusionBuilder : IBuildAreaExtrusion {
    private readonly RevitRepository _revitRepository;
    private readonly BuildCoordVolumesSettings _buildCoordVolumesSetting;
    private readonly SolidsService _solidsService;
    private readonly GeometryService _geometryService;
    private readonly IEnumerable<SlabElement> _allSlabs;
    private readonly double _minimalSide;
    private readonly double _side;

    public AdvancedAreaExtrusionBuilder(
        RevitRepository revitRepository,
        BuildCoordVolumesSettings buildCoordVolumesSettings,
        SolidsService solidsService,
        GeometryService geometryService) {
        _revitRepository = revitRepository;
        _buildCoordVolumesSetting = buildCoordVolumesSettings;
        _solidsService = solidsService;
        _geometryService = geometryService;
        _allSlabs = _revitRepository.GetSlabsByTypesAndDocs(_buildCoordVolumesSetting.TypeSlabs, _buildCoordVolumesSetting.Documents);
        _minimalSide = _revitRepository.Application.ShortCurveTolerance;
        _side = UnitUtils.ConvertToInternalUnits(_buildCoordVolumesSetting.SearchSide, UnitTypeId.Millimeters);
    }


    public void BuildAreaExtrusion(RevitArea area) {

        // 1) Большой солид зоны
        var zoneSolid = _solidsService.ExtrudeArea(area.Area,
            _allSlabs.Min(s => s.Floor.GetBoundingBox().Max.Z),
            _allSlabs.Max(s => s.Floor.GetBoundingBox().Max.Z) - _allSlabs.Min(s => s.Floor.GetBoundingBox().Max.Z));

        // 2) Плиты, пересекающие зону
        var targetSlabs = _allSlabs
            .Where(s => {
                var intersection = BooleanOperationsUtils.ExecuteBooleanOperation(s.FloorSolid, zoneSolid, BooleanOperationsType.Intersect);
                return intersection != null && intersection.Volume > 1e-6;
            })
            .OrderBy(s => s.Floor.GetBoundingBox().Max.Z)
            .ToList();


        if(!targetSlabs.Any()) {
            return;
        }

        // 3) группировка плит по уровню для промежуточных сегментов
        var slabsByLevel = targetSlabs
            .GroupBy(s => s.LevelName)
            .ToDictionary(g => g.Key, g => g.OrderBy(sl => sl.Floor.GetBoundingBox().Max.Z).ToList());

        // 4) формируем глобальный порядок уровней
        var levelSequence = targetSlabs
            .OrderBy(s => s.Floor.GetBoundingBox().Max.Z)
            .Select(s => s.LevelName)
            .Distinct()
            .ToList();

        // 5) делим зону на полигоны
        var polygons = _geometryService.DivideArea(area.Area, _side, _minimalSide);

        var columns = new List<Column>();

        foreach(var polygon in polygons) {

            //// 6) плиты, в которые входит центр полигона
            //var insideSlabs = targetSlabs
            //    .Where(slab => _geometryService.IsPointInsidePolygon(polygon.Center, slab.ExternalContourPoints))
            //    .OrderBy(slab => slab.Floor.GetBoundingBox().Max.Z)
            //    .ToList();

            // 6) плиты, в которые входит центр полигона
            var insideSlabs = targetSlabs
                .Where(slab => {
                    var allContours = slab.FullExternalContourPoints;
                    _geometryService.ClassifyContours(allContours, out var outer, out var holes);
                    return _geometryService.IsPointInsideFloorPolygon(polygon.Center, outer, holes);
                })
                .OrderBy(slab => slab.Floor.GetBoundingBox().Max.Z)
                .ToList();

            if(!insideSlabs.Any()) {
                continue;
            }

            // 7) строим колонны
            for(int i = 0; i < insideSlabs.Count; i++) {

                var startSlab = insideSlabs[i];
                string startLevel = startSlab.LevelName;
                double startZ = startSlab.Floor.GetBoundingBox().Max.Z;

                // ищем следующую плиту для выдавливания
                var nextSlab = (i + 1 < insideSlabs.Count) ? insideSlabs[i + 1] : null;

                if(nextSlab == null) {
                    break; // нет плит выше
                }

                string nextLevel = nextSlab.LevelName;
                double endZ = nextSlab.Floor.GetBoundingBox().Max.Z;

                int idxStart = levelSequence.IndexOf(startLevel);
                int idxEnd = levelSequence.IndexOf(nextLevel);

                // 8) проверяем пропуск уровней
                if(idxEnd == idxStart + 1) {
                    // подряд — строим одну колонну
                    var solid = _solidsService.ExtrudePolygon(polygon, startZ, endZ - startZ);
                    columns.Add(new Column() {
                        Polygon = polygon,
                        Solid = solid,
                        StartZ = startZ,
                        EndZ = endZ,
                        StartLevel = startLevel,
                        EndLevel = nextLevel
                    });
                } else {
                    // есть пропуск — создаем сегменты через все плитки между уровнями
                    for(int k = idxStart + 1; k <= idxEnd; k++) {
                        string levelName = levelSequence[k];
                        var slabsAtLevel = slabsByLevel[levelName];
                        foreach(var slab in slabsAtLevel) {
                            double segStartZ = startZ;
                            double segEndZ = slab.Floor.GetBoundingBox().Max.Z;
                            var solid = _solidsService.ExtrudePolygon(polygon, segStartZ, segEndZ - segStartZ);
                            columns.Add(new Column() {
                                Polygon = polygon,
                                Solid = solid,
                                StartZ = segStartZ,
                                EndZ = segEndZ,
                                StartLevel = "Undefined",
                                EndLevel = slab.LevelName
                            });
                            startZ = segEndZ; // следующий сегмент начинается с конца предыдущего
                        }
                    }
                }
            }
        }

        MessageBox.Show("Привет, это расширенный алгоритм");

        //// 9) создаем DirectShape штучно
        //foreach(var col in columns.Where(c => c.Solid != null && c.Solid.Volume > 1e-6)) {
        //    var ds = DirectShape.CreateElement(_revitRepository.Document, new ElementId(BuiltInCategory.OST_GenericModel));

        //    ds.ApplicationId = "RevitBuildCoordVolumes";
        //    ds.ApplicationDataId = Guid.NewGuid().ToString();
        //    ds.SetShape([col.Solid]);
        //    ds.LookupParameter("ФОП_Этаж СМР").Set(col.StartLevel + "-" + col.EndLevel);
        //}

        //var groupedColumns = columns
        //    .GroupBy(x => x.StartLevel + "_" + x.EndLevel);

        //foreach(var columnGroup in groupedColumns) {
        //    var solids = columnGroup
        //        .Select(x => x.Solid)
        //        .Where(x => x != null)
        //        .ToList();

        //    if(solids.Count == 0) {
        //        continue;
        //    }

        //    var directShape = DirectShape.CreateElement(_revitRepository.Document, new ElementId(BuiltInCategory.OST_GenericModel));

        //    var (resultSolid, failedSolids) = _solidsService.UnionSolids(solids, directShape, 50);

        //    if(resultSolid != null) {
        //        try {
        //            directShape.ApplicationId = "RevitBuildCoordVolumes";
        //            directShape.ApplicationDataId = Guid.NewGuid().ToString();
        //            directShape.SetShape([resultSolid]);
        //            directShape.LookupParameter("ФОП_Этаж СМР").Set(columnGroup.Key);
        //        } catch {
        //        }

        //    }

        //    foreach(var lostSolid in failedSolids) {
        //        try {
        //            var dsa = DirectShape.CreateElement(_revitRepository.Document, new ElementId(BuiltInCategory.OST_GenericModel));
        //            dsa.ApplicationId = "RevitBuildCoordVolumes";
        //            dsa.ApplicationDataId = Guid.NewGuid().ToString();
        //            dsa.SetShape([lostSolid]);
        //            dsa.LookupParameter("ФОП_Этаж СМР").Set(columnGroup.Key);
        //        } catch {
        //        }
        //    }
        //}
    }
}
