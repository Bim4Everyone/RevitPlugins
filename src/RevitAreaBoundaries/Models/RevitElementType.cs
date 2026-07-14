using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitAreaBoundaries.Models.Enums;

namespace RevitAreaBoundaries.Models;

public class RevitElementType : RevitElement {
    public ProjectionType ProjectionType { get; set; }
    public string CategoryName { get; set; }
    public string FamilyName { get; set; }
}
