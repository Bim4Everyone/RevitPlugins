using System.Linq;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;

using dosymep.Revit.Geometry;

namespace RevitPylonLoadAreas.Models.Selection;

internal sealed class FloorSelectionFilter : ISelectionFilter {
    public bool AllowElement(Element elem) {
        if(elem is not Floor) {
            return false;
        }

        // допустимы только перекрытия, у которых есть нижняя горизонтальная грань
        var solids = elem.GetSolids()
            ?.Where(s => s?.GetVolumeOrDefault(0) > 0)
            ?.ToArray();
        if(solids?.Length != 1) {
            return false;
        }

        // должна быть нижняя горизонтальная грань
        return solids[0]
            .Faces
            .OfType<PlanarFace>()
            .Any(f => f.FaceNormal.IsAlmostEqualTo(-XYZ.BasisZ));
    }

    public bool AllowReference(Reference reference, XYZ position) {
        return true;
    }
}
