using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitBuildCoordVolumes.Models.Geometry;
using RevitBuildCoordVolumes.Models.Settings;

namespace RevitBuildCoordVolumes.Models.Interfaces;

internal interface IParamSetter {
    void SetParams(SpatialElement spatialElement, List<DirectShapeObject> directShapeElements, BuildCoordVolumeSettings buildCoordVolumesSettings);
}
