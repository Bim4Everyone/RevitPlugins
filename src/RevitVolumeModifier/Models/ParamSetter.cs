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
            } else {
                SetParam(element, directShapeObject, paramModel);
            }
        }
    }

    // Метод назначения параметра объема DirectShapeObject
    private void SetVolumeParam(DirectShapeObject directShapeObject, ParamModel paramModel) {
        double value = directShapeObject.Volume;
        directShapeObject.DirectShape.SetParamValue(paramModel.RevitParam, value);
    }

    // Метод назначения числового параметра DirectShapeObject
    private void SetParam(Element element, DirectShapeObject directShapeObject, ParamModel paramModel) {
        try {
            var elementParam = element.GetParam(paramModel.RevitParam);
            var directShapeParam = directShapeObject.DirectShape.GetParam(paramModel.RevitParam);
            if(elementParam == null || directShapeParam == null || directShapeParam.IsReadOnly) {
                return;
            }
            directShapeParam.Set(elementParam);
            SetWorkset(element, directShapeObject.DirectShape);
        } catch {
            return;
        }
    }
    
    private static void SetWorkset(Element element, DirectShape directShape) {
        if(!directShape.Document.IsWorkshared) {
            return;
        }
        var sourceWorksetId = element.WorksetId;
        var worksetParam = directShape.GetParam(BuiltInParameter.ELEM_PARTITION_PARAM);
        if(worksetParam == null || worksetParam.IsReadOnly) {
            return;
        }
        worksetParam.Set(sourceWorksetId.IntegerValue);
    }
}
