using Autodesk.Revit.DB;

namespace RevitFinishing.Models.Finishing;
internal interface IFinishingFactory {
    public FinishingElement Create(Element element);
}
