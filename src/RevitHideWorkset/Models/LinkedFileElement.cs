using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitHideWorkset.Models;
internal class LinkedFileElement {
    public RevitLinkInstance LinkedFile { get; set; }
    public List<WorksetElement> AllWorksets { get; set; } = [];
    public List<WorksetElement> FilteredWorksets { get; set; } = [];
}
