using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.Revit.Geometry;

using RevitToMongoDB.Model;
using RevitToMongoDB.ViewModels.Interfaces;

namespace RevitToMongoDB.ViewModels {
    internal class ElementCreator : IElementFactory {

        public void FillLocationData(ElementDto elem, BoundingBoxXYZ boundingBoxXYZ) {
            elem.Location = new Model.Location();
            elem.Location.Min = CreateXYZ(boundingBoxXYZ?.Min);
            elem.Location.Max = CreateXYZ(boundingBoxXYZ?.Max);
            elem.Location.Mid = CreateXYZ(boundingBoxXYZ?.GetMidPoint());
        }

        public Model.XYZ CreateXYZ(Autodesk.Revit.DB.XYZ xyz) {
            if(xyz == null)
                return null;
            return new Model.XYZ() { X = xyz.X, Y = xyz.Y, Z = xyz.Z };
        }

        public ElementDto CreateElement(Element autodeskElement) {
            ElementDto ElementDto = new ElementDto();
            ElementDto.Id = autodeskElement.Id.GetIdValue();
            ElementDto.TypeId = autodeskElement.GetTypeId().GetIdValue();
            ElementDto.UniqueId = autodeskElement.UniqueId.ToString();
            ElementDto.Category = autodeskElement.Category;
            ElementDto.Name = autodeskElement.Name;
            FillLocationData(ElementDto, autodeskElement.GetBoundingBox());
            FillParamsData(ElementDto, autodeskElement);
            return ElementDto;
        }

        public void FillParamsData(ElementDto elementDto, Element autodeskElementDto) {
            List<Param> paramList = new List<Param>();
            foreach(Parameter par in autodeskElementDto.Parameters.Cast<Parameter>()) {
                if(par == null || !par.HasValue) {
                    continue;
                }
                Param param = new Param();
                param.Id = par.Element.Id.GetIdValue();
                param.Name = par.Definition.Name;
                param.Value = par.AsValueString();
                param.UnitType = par.GetType().ToString();
                param.StorageType = par.StorageType.ToString();
                if(par.IsShared) {
                    param.Guid = par.GUID;
                }
                param.SystemName = par.Definition.GetBuiltInParameter().ToString();
                paramList.Add(param);
            }
            elementDto.ParamList = paramList;
        }
    }
}
