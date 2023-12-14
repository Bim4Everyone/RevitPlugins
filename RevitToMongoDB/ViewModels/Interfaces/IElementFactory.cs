using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using RevitToMongoDB.Model;

using static RevitToMongoDB.Model.ElementDto;

namespace RevitToMongoDB.ViewModels.Interfaces {
    internal interface IElementFactory {

        ElementDto CreateElement(Element AutodeskElement);

        void FillLocationData(ElementDto elem, BoundingBoxXYZ boundingBoxXYZ);

        ElementDto.Location.XYZ CreateXYZ(XYZ xyz);


        void FillParamsData(ElementDto ElementDto, Element autodeskElementDto);


    }
}
