using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitDocumenter.Models.Dimensions;
internal class RebarElement {
    public RebarElement(FamilyInstance rebar, List<Reference> verticalRefs, List<Reference> horizontalRefs) {
        Rebar = rebar;
        VerticalRefs = verticalRefs;
        HorizontalRefs = horizontalRefs;
    }
    public FamilyInstance Rebar { get; set; }
    public List<Reference> VerticalRefs { get; set; }
    public List<Reference> HorizontalRefs { get; set; }
}
