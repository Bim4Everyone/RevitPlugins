using Autodesk.Revit.DB;

namespace RevitMarkAllDocuments.ViewModels;

internal class CategoryViewModel {
    private readonly string _name;
    private readonly Category _category;

    public CategoryViewModel(Category category) {
        _category = category;
        _name = category.Name;
    }

    public string Name => _name;
    public Category Category => _category;
}
