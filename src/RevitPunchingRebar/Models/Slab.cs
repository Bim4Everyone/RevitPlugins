using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;

namespace RevitPunchingRebar.Models;
internal class Slab {
    public Floor SlabInstance { get; }
    public double RebarCoverTop { get; }
    public double RebarCoverBottom { get; }
    public double Thickness { get; }

    internal Slab(Element element) {
        if(element != null) {
            SlabInstance = element as Floor;
            Thickness = element.get_Parameter(BuiltInParameter.FLOOR_ATTR_THICKNESS_PARAM).AsDouble();

            RebarCoverTop = GetRebarCoverTop();
            RebarCoverBottom = GetRebarCoverBottom();
        }
    }

    private double GetRebarCoverTop() {
        Document doc = SlabInstance.Document;

        RebarCoverType rebarCoverTopType = doc.GetElement(SlabInstance.get_Parameter(BuiltInParameter.CLEAR_COVER_TOP).AsElementId()) as RebarCoverType;
        double rebarCoverUp = rebarCoverTopType.CoverDistance;

        return rebarCoverUp;
    }

    private double GetRebarCoverBottom() {
        Document doc = SlabInstance.Document;

        RebarCoverType rebarCoverBottomType = doc.GetElement(SlabInstance.get_Parameter(BuiltInParameter.CLEAR_COVER_BOTTOM).AsElementId()) as RebarCoverType;
        double rebarCoverBottom = rebarCoverBottomType.CoverDistance;

        return rebarCoverBottom;
    }
}
