using System.Linq;

using Autodesk.Revit.DB;

using RevitClashDetective.Models.Interfaces;
using RevitClashDetective.Models.RevitViews;

namespace RevitOpeningPlacement.Models.RevitViews {
    internal class View3DProvider : IView3DProvider {
        public View3D GetView(Document doc, string name) {
            var view = new FilteredElementCollector(doc)
                .OfClass(typeof(View3D))
                .OfType<View3D>()
                .FirstOrDefault(item => !item.IsTemplate && item.Name.Equals(name))
                ?? new View3DBuilder()
                .SetName(name)
                .SetViewSettings(ViewSettingsInitializer.GetView3DSettings(doc).ToArray())
                .Build(doc);

            return view;
        }
    }
}
