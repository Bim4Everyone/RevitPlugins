using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using RevitClashDetective.Models.Value;

namespace RevitOpeningPlacement.Models.Interfaces {
    internal interface IValueGetter<T> where T : ParamValue {
        T GetValue();
    }

    internal interface IView3DProvider {
        View3D GetView(Document doc, string name);
    }

    internal interface IView3DBuilder {
        IView3DBuilder SetName(string name);
        IView3DBuilder SetTemplate(View3D template);
        IView3DBuilder SetViewSettings(params IView3DSetting[] settings);
        View3D Build(Document doc);
    }

    internal interface IView3DSetting {
        void Apply(View3D view3D);
    }
}
