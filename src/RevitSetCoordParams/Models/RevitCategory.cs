using Autodesk.Revit.DB;

namespace RevitSetCoordParams.Models;
internal class RevitCategory {
    public BuiltInCategory BuiltInCategory { get; set; }
    public bool IsChecked { get; set; }
}
