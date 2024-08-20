using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

namespace RevitClashDetective.ViewModels.FilterCreatorViewModels {
    internal class CategoryViewModel : BaseViewModel, IEquatable<CategoryViewModel> {
        private string _name;
        private bool _isSelected;

        public CategoryViewModel(Category category) {
            Category = category;
            Name = Category.Name;
        }

        public bool IsSelected {
            get => _isSelected;
            set => RaiseAndSetIfChanged(ref _isSelected, value);
        }
        public string Name {
            get => _name;
            set => RaiseAndSetIfChanged(ref _name, value);
        }

        public Category Category { get; }

        public override bool Equals(object obj) {
            return Equals(obj as CategoryViewModel);
        }

        public bool Equals(CategoryViewModel other) {
            if(ReferenceEquals(null, other)) { return false; }
            if(ReferenceEquals(this, other)) { return true; }
            return Name == other.Name
                && Category.Id == other.Category.Id;
        }

        public override int GetHashCode() {
            int hashCode = -808104057;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            hashCode = hashCode * -1521134295 + EqualityComparer<ElementId>.Default.GetHashCode(Category.Id);
            return hashCode;
        }
    }
}
