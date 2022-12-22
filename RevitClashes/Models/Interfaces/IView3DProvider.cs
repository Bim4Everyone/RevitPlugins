
using Autodesk.Revit.DB;

namespace RevitClashDetective.Models.Interfaces {
    internal interface IView3DProvider {
        View3D GetView(Document doc, string name);
    }
}
