
using Autodesk.Revit.DB;

namespace RevitClashDetective.Models.Interfaces {
    internal interface IView3DSetting {
        void Apply(View3D view3D);
    }
}
