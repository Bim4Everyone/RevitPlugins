using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

namespace RevitClashDetective.ViewModels.FilterCreatorViewModels;
internal class CategoryViewModel : BaseViewModel, IEquatable<CategoryViewModel> {
    private bool _isSelected;

    public CategoryViewModel(Category category) {
        Category = category ?? throw new ArgumentNullException(nameof(category));
        Name = Category.Name;
    }

    public bool IsSelected {
        get => _isSelected;
        set => RaiseAndSetIfChanged(ref _isSelected, value);
    }

    public string Name { get; }

    public Category Category { get; }

    public override bool Equals(object obj) {
        return Equals(obj as CategoryViewModel);
    }

    public bool Equals(CategoryViewModel other) {
        if(other is null) { return false; }
        return ReferenceEquals(this, other) || Category.Id == other.Category.Id;
    }

    public override int GetHashCode() {
        int hashCode = -808104057;
        hashCode = hashCode * -1521134295 + EqualityComparer<ElementId>.Default.GetHashCode(Category.Id);
        return hashCode;
    }
}
