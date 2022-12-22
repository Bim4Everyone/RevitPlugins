using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.RevitViews {
    internal class View3DProvider : IView3DProvider {
        public View3D GetView(Document doc, string name) {
            var view = new FilteredElementCollector(doc)
                .OfClass(typeof(View3D))
                .OfType<View3D>()
                .FirstOrDefault(item => !item.IsTemplate && item.Name.Equals(name));
            if(view == null) {
                view = new View3DBuilder()
                    .SetName(name)
                    .SetViewSettings(ViewSettingsInitializer.GetView3DSettings(doc).ToArray())
                    .Build(doc);
            }

            return view;
        }
    }
}
