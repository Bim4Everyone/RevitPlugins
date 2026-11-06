using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;

namespace RevitPunchingRebar.Models;
internal class Slab {
    internal Slab(Element element) {
        if(element != null) {
            SlabInstance = element as Floor;
            Thickness = element.get_Parameter(BuiltInParameter.FLOOR_ATTR_THICKNESS_PARAM).AsDouble();

            RebarCoverTop = GetRebarCover(BuiltInParameter.CLEAR_COVER_TOP);
            RebarCoverBottom = GetRebarCover(BuiltInParameter.CLEAR_COVER_BOTTOM);
        }
    }

    public Floor SlabInstance { get; }

    /// <summary>
    /// Защитный слой сверху из типа защитного слоя плиты. null, если тип защитного слоя не назначен.
    /// </summary>
    public double? RebarCoverTop { get; }

    /// <summary>
    /// Защитный слой снизу из типа защитного слоя плиты. null, если тип защитного слоя не назначен.
    /// </summary>
    public double? RebarCoverBottom { get; }

    public double Thickness { get; }

    /// <summary>
    /// Назначены ли плите типы защитного слоя сверху и снизу.
    /// </summary>
    public bool HasRebarCover => RebarCoverTop.HasValue && RebarCoverBottom.HasValue;

    private double? GetRebarCover(BuiltInParameter coverParam) {
        ElementId coverTypeId = SlabInstance?.get_Parameter(coverParam)?.AsElementId();
        if(coverTypeId == null) {
            return null;
        }

        RebarCoverType coverType = SlabInstance.Document.GetElement(coverTypeId) as RebarCoverType;
        return coverType?.CoverDistance;
    }
}
