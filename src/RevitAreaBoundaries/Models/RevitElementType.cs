using RevitAreaBoundaries.Models.Enums;

namespace RevitAreaBoundaries.Models;

internal class RevitElementType : RevitElement {
    public ProjectionType ProjectionType { get; set; }
    public string CategoryName { get; set; }
    public string FamilyName { get; set; }
}
