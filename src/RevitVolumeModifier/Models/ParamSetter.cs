using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Revit;

using RevitVolumeModifier.Enums;

namespace RevitVolumeModifier.Models;
internal class ParamSetter {
    public void SetParams(Element element, List<DirectShapeObject> directShapeObjects, IEnumerable<ParamModel> paramModels) {
        foreach(var directShapeObject in directShapeObjects) {
            Set(element, directShapeObject, paramModels);
        }
    }

    // Метод назначения параметров DirectShapeObject
    private void Set(Element element, DirectShapeObject directShapeObject, IEnumerable<ParamModel> paramModels) {
        var directShape = directShapeObject.DirectShape;
        foreach(var paramModel in paramModels) {
            if(paramModel.ParamType == ParamType.VolumeParam) {
                SetVolumeParam(directShapeObject, paramModel);
            } else if(paramModel.ParamType == ParamType.FloorDEParam) {
                SetDoubleParam(element, directShapeObject, paramModel);
            } else {
                SetStringParam(element, directShapeObject, paramModel);
            }
        }
    }

    // Метод назначения параметра объема DirectShapeObject
    private void SetVolumeParam(DirectShapeObject directShapeObject, ParamModel paramModel) {
        double value = directShapeObject.Volume;
        directShapeObject.DirectShape.SetParamValue(paramModel.RevitParam, value);
    }

    // Метод назначения числового параметра DirectShapeObject
    private void SetDoubleParam(Element element, DirectShapeObject directShapeObject, ParamModel paramModel) {
        double value = element.GetParamValueOrDefault<double>(paramModel.RevitParam);
        directShapeObject.DirectShape.SetParamValue(paramModel.RevitParam, value);
    }

    // Метод назначения текстового параметра DirectShapeObject
    private void SetStringParam(Element element, DirectShapeObject directShapeObject, ParamModel paramModel) {
        string value = element.GetParamValueOrDefault<string>(paramModel.RevitParam);
        directShapeObject.DirectShape.SetParamValue(paramModel.RevitParam, value);
    }
}
