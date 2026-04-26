using Autodesk.Revit.DB;

namespace RevitMarkAllDocuments.Models;

internal class CategoryContext {
    public Category SelectedCategory { get; set; }
    public bool IsMarkForTypes { get; set; }
}
