using Autodesk.Revit.DB;

using dosymep.Revit;

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

    public IParamProvider GetParamProvider(RevitRepository revitRepository) {
        if(IsMarkForTypes) {
            return new TypeParamProvider(revitRepository, SelectedCategory);
        }

        return new InstanceParamProvider(revitRepository, SelectedCategory);
    }
}
