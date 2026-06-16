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

        // допустимы только перекрытия с 1 телом, у которых есть нижняя горизонтальная грань
        var solid = elem.GetSolids()
            .SelectMany(SolidUtils.SplitVolumes)
            ?.Where(s => s.GetVolumeOrDefault(0) > 0)
            ?.FirstOrDefault();
        return solid != null
               && solid.Faces
                   .OfType<PlanarFace>()
                   .FirstOrDefault(f => f.FaceNormal.IsAlmostEqualTo(-XYZ.BasisZ))
               != null;
    }

    public bool AllowReference(Reference reference, XYZ position) {
        return true;
    }
}
