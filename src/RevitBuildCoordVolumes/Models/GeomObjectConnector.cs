using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitBuildCoordVolumes.Models.Geometry;
using RevitBuildCoordVolumes.Models.Interfaces;
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

    public List<GeomObject> UnionGeomObjects(List<GeomObject> geomObjects) {
        if(geomObjects.Count == 0) {
            return [];
        }

        var groups = geomObjects
            .GroupBy(geomObject => geomObject.FloorName);

        var unitedGeomObjects = new List<GeomObject>();

        foreach(var group in groups) {
            var solids = group
                .Select(geomObject => geomObject.GeometryObjects)
                .Cast<Solid>();

            var unitedSolids = SolidUtility.CreateUnitedSolids([.. solids]);

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

    // Валидация солидов для DirectShape
    private bool ValidateSolids(IList<GeometryObject> solids) {
        var directShape = DirectShape.CreateElement(_revitRepository.Document, _systemPluginConfig.ElementIdDirectShape);
        return directShape.IsValidShape(solids);
    }
}
