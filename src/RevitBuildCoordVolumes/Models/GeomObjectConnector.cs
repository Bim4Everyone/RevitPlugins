using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitBuildCoordVolumes.Models.Enums;
using RevitBuildCoordVolumes.Models.Geometry;
using RevitBuildCoordVolumes.Models.Interfaces;
using RevitBuildCoordVolumes.Models.Services;
using RevitBuildCoordVolumes.Models.Utilites;

namespace RevitBuildCoordVolumes.Models;
internal class GeomObjectConnector : IGeomObjectConnector {
    private const int _attemptCount = 3; //Количество попыток смешивания
    private readonly RevitRepository _revitRepository;
    private readonly SystemPluginConfig _systemPluginConfig;

    public GeomObjectConnector(RevitRepository revitRepository, SystemPluginConfig systemPluginConfig) {
        _revitRepository = revitRepository;
        _systemPluginConfig = systemPluginConfig;
    }

    public List<GeomObject> UnionGeomObjects(List<GeomObject> geomObjects, ProgressService progressService) {
        if(geomObjects.Count == 0) {
            return [];
        }

        var groups = geomObjects
            .GroupBy(geomObject => geomObject.FloorName);



        var unitedGeomObjects = new List<GeomObject>();
        foreach(var group in groups) {

            var solids = group
                .Where(g => g.GeometryObjects != null)
                .SelectMany(g => g.GeometryObjects)
                .OfType<Solid>();

            if(solids.Count() == 0) {
                continue;
            }

            var unitedSolids = CreateUnitedSolids([.. solids], progressService);

            if(unitedSolids.Count == 0) {
                continue;
            }

            bool check = ValidateSolids([.. unitedSolids]);

            int attempt = 0;
            while(!check && attempt < _attemptCount) {
                var shuffledSolids = SolidUtility.ShuffleSolidsByGuid(unitedSolids);
                check = ValidateSolids([.. shuffledSolids]);

                if(check) {
                    unitedSolids = shuffledSolids;
                    break;
                }

                attempt++;
            }

            if(check) {
                unitedGeomObjects.Add(new GeomObject {
                    GeometryObjects = [.. unitedSolids],
                    FloorName = group.Key,
                    Volume = SolidUtility.GetSolidsVolume(unitedSolids)
                });
            } else {
                unitedGeomObjects.AddRange(group);
            }
        }
        return unitedGeomObjects;
    }

    public static IList<Solid> CreateUnitedSolids(IList<Solid> solids, ProgressService progressService) {
        var solid = solids[0];

        progressService?.BeginStage(ProgressType.UnionVolumes);

        int total = solids.Count;
        int processed = 0;
        int reported = 0;

        List<Solid> list = [];
        for(int i = 1; i < solids.Count; i++) {
            progressService?.CancellationToken.ThrowIfCancellationRequested();

            var solid2 = solids[i];
            try {
                solid = BooleanOperationsUtils.ExecuteBooleanOperation(solid, solid2, BooleanOperationsType.Union);
            } catch {
                list.Add(solid);
                solid = solid2;
            }

            processed++;
            int current = processed * 100 / total;
            if(current > 100) {
                current = 100;
            }
            if(current > reported) {
                reported = current;
                progressService?.ProgressCount?.Report(reported);
            }
        }

        list.Add(solid);
        return list;
    }

    // Валидация солидов для DirectShape
    private bool ValidateSolids(IList<GeometryObject> solids) {
        var directShape = DirectShape.CreateElement(_revitRepository.Document, _systemPluginConfig.ElementIdDirectShape);
        return directShape.IsValidShape(solids);
    }
}
