using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.Revit.Geometry;

using RevitToMongoDB.Model;
using RevitToMongoDB.ViewModels.Interfaces;

using static RevitToMongoDB.Model.ElementDto;

namespace RevitToMongoDB.ViewModels {
    internal class ElementCreator : IElementFactory {

        public void FillLocationData(ElementDto elem, BoundingBoxXYZ boundingBoxXYZ) {
            elem.Locat = new ElementDto.Location();
            elem.Locat.Min = CreateXYZ(boundingBoxXYZ?.Min);
            elem.Locat.Max = CreateXYZ(boundingBoxXYZ?.Max);
            elem.Locat.Mid = CreateXYZ(boundingBoxXYZ?.GetMidPoint());
        }

        public ElementDto.Location.XYZ CreateXYZ(XYZ xyz) {
            if(xyz == null)
                return null;
            return new ElementDto.Location.XYZ() { X = xyz.X, Y = xyz.Y, Z = xyz.Z };
        }

        public ElementDto CreateElement(Element AutodeskElement) {
            ElementDto ElementDto = new ElementDto();
            ElementDto.Id = AutodeskElement.Id.GetIdValue();
            ElementDto.TypeId = AutodeskElement.GetTypeId().GetIdValue();
            ElementDto.UniqueId = AutodeskElement.UniqueId.ToString();
            //this.VersionGuid = ElementDto.guid VersionGuid;
            ElementDto.Name = AutodeskElement.Name;
            //ElementDto.ModelName = docTitle;
            //ElementDto.title = docTitle;
            FillLocationData(ElementDto, AutodeskElement.GetBoundingBox());
            FillParamsData(ElementDto, AutodeskElement);
            return ElementDto;
        }

        public void FillParamsData(ElementDto ElementDto, Element autodeskElementDto) {
            List<Param> paramList = new List<Param>();
            foreach(Parameter par in autodeskElementDto.Parameters.Cast<Parameter>()) {
                if(par == null || !par.HasValue) {
                    continue;
                }
                Param param = new Param(); 
                param.Name = par.Definition.Name;
                //param.Value = par.AsValueString();
                //param.UnitType = par. GetUnitTypeId().ToString();
                //param.StorageType = par.StorageType;
                //param.Guid = par.GUID;
                //param.SystemName = par.Definition.Name;

                paramList.Add(param);
            }
            ElementDto.ParamList = paramList;
        }
    }
}
