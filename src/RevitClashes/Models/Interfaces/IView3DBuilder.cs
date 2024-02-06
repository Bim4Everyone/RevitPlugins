
using Autodesk.Revit.DB;

namespace RevitClashDetective.Models.Interfaces {
    internal interface IView3DBuilder {
        IView3DBuilder SetName(string name);
        IView3DBuilder SetTemplate(View3D template);
        IView3DBuilder SetViewSettings(params IView3DSetting[] settings);
        View3D Build(Document doc);
    }
}
