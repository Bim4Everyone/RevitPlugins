using RevitAreaBoundaries.Models.Enums;

namespace RevitAreaBoundaries.Models;

public class RevitElementType : RevitElement {
     
    public SectionType SectionType { get; set; }
    public string CategoryName { get; set; }
}
