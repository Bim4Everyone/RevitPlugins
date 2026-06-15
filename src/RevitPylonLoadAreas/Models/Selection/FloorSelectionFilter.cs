using System.Linq;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;

using dosymep.Revit.Geometry;

namespace RevitPylonLoadAreas.Models.Selection;

internal sealed class FloorSelectionFilter : ISelectionFilter {
    public bool AllowElement(Element elem) {
        // допустимы только перекрытия с 1 телом
        return elem is Floor && elem.GetSolids()?.Where(s => s.GetVolumeOrDefault(0) > 0)?.Count() == 1;
    }

    public bool AllowReference(Reference reference, XYZ position) {
        return true;
    }
}
