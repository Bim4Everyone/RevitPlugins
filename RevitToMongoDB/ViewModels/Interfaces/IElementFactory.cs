using Autodesk.Revit.DB;

using RevitToMongoDB.Model;

namespace RevitToMongoDB.ViewModels.Interfaces {
    internal interface IElementFactory {
        ElementDto CreateElement(Element autodeskElement);
        Model.XYZ CreateXYZ(Autodesk.Revit.DB.XYZ xyz);
        void FillLocationData(ElementDto elem, BoundingBoxXYZ boundingBoxXYZ);
        void FillParamsData(ElementDto elem, Element autodeskElementDto);
    }
}
