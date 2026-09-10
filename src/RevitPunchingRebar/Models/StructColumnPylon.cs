using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SharedParams;
using dosymep.Revit;

using RevitPunchingRebar.Models.Interfaces;

namespace RevitPunchingRebar.Models;
internal class StructColumnPylon : IPylon {
    public Element PylonInstance { get; set; } = null;
    public XYZ FacingOrientation { get; set; } = null;

    public double Length { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }

    internal StructColumnPylon(Element element) {
        PylonInstance = element;
        FacingOrientation = ((FamilyInstance) element).FacingOrientation;

        Element elementType = element.Document.GetElement(element.get_Parameter(BuiltInParameter.ELEM_TYPE_PARAM).AsElementId());

        Length = elementType.GetParamValueOrDefault<double>(SharedParamsConfig.Instance.SizeLength, 0);
        Width = elementType.GetParamValueOrDefault<double>(SharedParamsConfig.Instance.SizeWidth, 0);
        Height = element.GetParamValueOrDefault<double>(SharedParamsConfig.Instance.SizeHeight, 0);
    }

    /// <summary>
    /// Возвращает нижнюю центральную точку
    /// </summary>
    /// <returns></returns>
    public XYZ GetLocation() {
        if(PylonInstance != null) {
            XYZ locationPoint = ((LocationPoint) PylonInstance.Location).Point;

            Level downLevel = PylonInstance.Document.GetElement(PylonInstance.get_Parameter(BuiltInParameter.FAMILY_BASE_LEVEL_PARAM).AsElementId()) as Level;

            double downLevelMark = downLevel.ProjectElevation;
            double downOffset = PylonInstance.get_Parameter(BuiltInParameter.FAMILY_BASE_LEVEL_OFFSET_PARAM).AsDouble();

            XYZ columnLocation = new XYZ
                (
                    locationPoint.X,
                    locationPoint.Y,
                    downLevelMark + downOffset);

            return columnLocation;
        }
        else {
            return null;
        }
    }
}
