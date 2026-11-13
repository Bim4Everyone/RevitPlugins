using Autodesk.Revit.DB;

namespace RevitUnmodelingMep.ViewModels;

internal class CategoryOption {
    public string Name { get; set; }
    public BuiltInCategory BuiltInCategory { get; set; }
    public int Id { get; set; }
}
