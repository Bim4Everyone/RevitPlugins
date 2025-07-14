using Autodesk.Revit.DB;

namespace RevitFinishing.Models.Finishing;
internal abstract class FinishingFactory {
    public abstract FinishingElement Create(Element element);
}
