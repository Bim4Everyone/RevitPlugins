using Autodesk.Revit.DB;

namespace RevitMarkAllDocuments.Models;

internal class CategoryContext {
    public Category SelectedCategory { get; set; }
    public bool IsMarkForTypes { get; set; }

    public IMarkStrategy GetMarkStrategy() {
        if(IsMarkForTypes) {
            return new MarkTypeStrategy();
        }

        return new MarkInstanceStrategy();
    }
}
