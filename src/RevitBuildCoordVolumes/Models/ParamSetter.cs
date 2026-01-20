using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Revit;

using RevitBuildCoordVolumes.Models.Enums;
using RevitBuildCoordVolumes.Models.Geometry;
using RevitBuildCoordVolumes.Models.Interfaces;
using RevitBuildCoordVolumes.Models.Settings;

namespace RevitBuildCoordVolumes.Models;
internal class ParamSetter : IParamSetter {
    private readonly SystemPluginConfig _systemPluginConfig;
    public ParamSetter(SystemPluginConfig systemPluginConfig) {
        _systemPluginConfig = systemPluginConfig;
    }

    public void SetParams(SpatialElement spatialElement, List<DirectShapeObject> directShapeElements, BuildCoordVolumesSettings buildCoordVolumesSettings) {
        foreach(var directShapeElement in directShapeElements) {
            Set(spatialElement, directShapeElement, buildCoordVolumesSettings);
        }
    }

    private void Set(SpatialElement spatialElement, DirectShapeObject directShapeElement, BuildCoordVolumesSettings buildCoordVolumesSettings) {
        var directShape = directShapeElement.DirectShape;
        var paramMaps = buildCoordVolumesSettings.ParamMaps;
        var algorithm = buildCoordVolumesSettings.AlgorithmType;
        foreach(var paramMap in paramMaps) {

            if(spatialElement.IsExistsParam(paramMap.SourceParam.Name) && directShape.IsExistsParam(paramMap.TargetParam.Name)) {
                if(paramMap.Type == ParamType.FloorDEParam) {
                    SetFloorDEParam(spatialElement, directShapeElement, paramMap, algorithm);
                } else if(paramMap.Type == ParamType.FloorParam) {
                    SetFloorParam(spatialElement, directShapeElement, paramMap, algorithm);
                } else {
                    SetStringParam(spatialElement, directShapeElement, paramMap);
                }
            }
        }
    }

    private void SetFloorDEParam(SpatialElement spatialElement, DirectShapeObject directShapeElement, ParamMap paramMap, AlgorithmType algorithm) {
        if(algorithm == AlgorithmType.AdvancedAreaExtrude) {
            double value = GetFloorDEValue(directShapeElement.FloorName);
            directShapeElement.DirectShape.SetParamValue(paramMap.TargetParam.Name, value);
        } else if(algorithm == AlgorithmType.SimpleAreaExtrude) {
            SetDoubleParam(spatialElement, directShapeElement, paramMap);
        }
    }

    private void SetFloorParam(SpatialElement spatialElement, DirectShapeObject directShapeElement, ParamMap paramMap, AlgorithmType algorithm) {
        if(algorithm == AlgorithmType.AdvancedAreaExtrude) {
            string value = directShapeElement.FloorName;
            directShapeElement.DirectShape.SetParamValue(paramMap.TargetParam.Name, value);
        } else if(algorithm == AlgorithmType.SimpleAreaExtrude) {
            SetStringParam(spatialElement, directShapeElement, paramMap);
        }
    }

    private void SetStringParam(SpatialElement spatialElement, DirectShapeObject directShapeElement, ParamMap paramMap) {
        string value = spatialElement.GetParamValueOrDefault<string>(paramMap.SourceParam.Name);
        directShapeElement.DirectShape.SetParamValue(paramMap.TargetParam.Name, value);
    }

    private void SetDoubleParam(SpatialElement spatialElement, DirectShapeObject directShapeElement, ParamMap paramMap) {
        double value = spatialElement.GetParamValueOrDefault<double>(paramMap.SourceParam.Name);
        directShapeElement.DirectShape.SetParamValue(paramMap.TargetParam.Name, value);
    }


    private double GetFloorDEValue(string stringValue) {
        if(string.IsNullOrWhiteSpace(stringValue)) {
            return 0;
        }

        string value = stringValue.Trim();

        // Кровля
        if(value.StartsWith(_systemPluginConfig.DefaultStringRoof)) {
            return _systemPluginConfig.DefaultStringRoofDE;
        }

        // Паркинг (подземный этаж)
        if(value.StartsWith(_systemPluginConfig.DefaultStringParking)) {
            string numberPart = value
                .Replace(_systemPluginConfig.DefaultStringParking, string.Empty)
                .Replace(_systemPluginConfig.DefaultStringFloor, string.Empty)
                .Trim();

            return int.TryParse(numberPart, out int parkingLevel) ? -parkingLevel : 0;
        }

        // Обычный этаж
        {
            string numberPart = value
                .Replace(_systemPluginConfig.DefaultStringFloor, string.Empty)
                .Trim();

            if(int.TryParse(numberPart, out int floorLevel)) {
                return floorLevel;
            }
        }

        return 0;
    }
}
