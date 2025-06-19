using Autodesk.Revit.DB;

namespace RevitRoughFinishingDesign.Models;
internal class PairModel {
    public PairModel(ElementId wallTypeId, ElementId lineStyleId) {
        WallTypeId = wallTypeId;
        LineStyleId = lineStyleId;
    }
    public ElementId WallTypeId { get; set; }
    public ElementId LineStyleId { get; set; }
}
