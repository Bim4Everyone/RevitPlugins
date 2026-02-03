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

    public void SetParams(SpatialElement spatialElement, List<DirectShapeObject> directShapeObjects, BuildCoordVolumeSettings buildCoordVolumeSettings) {
        foreach(var directShapeObject in directShapeObjects) {
            Set(spatialElement, directShapeObject, buildCoordVolumeSettings);
        }
    }

    // Метод назначения параметров DirectShapeObject
    private void Set(SpatialElement spatialElement, DirectShapeObject directShapeObject, BuildCoordVolumeSettings buildCoordVolumeSettings) {
        var directShape = directShapeObject.DirectShape;
        var paramMaps = buildCoordVolumeSettings.ParamMaps;
        var algorithm = buildCoordVolumeSettings.AlgorithmType;
        foreach(var paramMap in paramMaps) {
            if(paramMap.Type == ParamType.VolumeParam) {
                SetVolumeParam(directShapeObject, paramMap);
            }
            if(paramMap.SourceParam != null && paramMap.TargetParam != null) {
                if(spatialElement.IsExistsParam(paramMap.SourceParam.Name) && directShape.IsExistsParam(paramMap.TargetParam.Name)) {
                    if(paramMap.Type == ParamType.FloorDEParam) {
                        SetFloorDEParam(spatialElement, directShapeObject, paramMap, algorithm);
                    } else if(paramMap.Type == ParamType.FloorParam) {
                        SetFloorParam(spatialElement, directShapeObject, paramMap, algorithm);
                    } else {
                        SetStringParam(spatialElement, directShapeObject, paramMap);
                    }
                }
            }
        }
    }

    // Метод назначения параметра этажа "Денежная единица" DirectShapeObject
    private void SetFloorDEParam(SpatialElement spatialElement, DirectShapeObject directShapeObject, ParamMap paramMap, AlgorithmType algorithm) {
        if(algorithm == AlgorithmType.SlabBasedAlgorithm) {
            double value = GetFloorDEValue(directShapeObject.FloorName);
            directShapeObject.DirectShape.SetParamValue(paramMap.TargetParam.Name, value);
        } else if(algorithm == AlgorithmType.ParamBasedAlgorithm) {
            SetDoubleParam(spatialElement, directShapeObject, paramMap);
        }
    }

    // Метод назначения параметра этажа DirectShapeObject
    private void SetFloorParam(SpatialElement spatialElement, DirectShapeObject directShapeObject, ParamMap paramMap, AlgorithmType algorithm) {
        if(algorithm == AlgorithmType.SlabBasedAlgorithm) {
            string value = directShapeObject.FloorName;
            directShapeObject.DirectShape.SetParamValue(paramMap.TargetParam.Name, value);
        } else if(algorithm == AlgorithmType.ParamBasedAlgorithm) {
            SetStringParam(spatialElement, directShapeObject, paramMap);
        }
    }

    // Метод назначения текстового параметра DirectShapeObject
    private void SetStringParam(SpatialElement spatialElement, DirectShapeObject directShapeObject, ParamMap paramMap) {
        string value = spatialElement.GetParamValueOrDefault<string>(paramMap.SourceParam.Name);
        directShapeObject.DirectShape.SetParamValue(paramMap.TargetParam.Name, value);
    }

    // Метод назначения числового параметра DirectShapeObject
    private void SetDoubleParam(SpatialElement spatialElement, DirectShapeObject directShapeObject, ParamMap paramMap) {
        double value = spatialElement.GetParamValueOrDefault<double>(paramMap.SourceParam.Name);
        directShapeObject.DirectShape.SetParamValue(paramMap.TargetParam.Name, value);
    }

    // Метод назначения параметра объема DirectShapeObject
    private void SetVolumeParam(DirectShapeObject directShapeObject, ParamMap paramMap) {
        double value = directShapeObject.Volume;
        directShapeObject.DirectShape.SetParamValue(paramMap.TargetParam.Name, value);
    }

    // Метод получения значения для параметра этажа "Денежная единица"
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
