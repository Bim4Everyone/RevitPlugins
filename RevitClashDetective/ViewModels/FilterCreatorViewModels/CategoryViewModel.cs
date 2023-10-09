using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.Revit;
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
            set => this.RaiseAndSetIfChanged(ref _isSelected, value);
        }
        public string Name {
            get => _name;
            set => this.RaiseAndSetIfChanged(ref _name, value);
        }

        public Category Category { get; }

        public override bool Equals(object obj) {
            return Equals(obj as CategoryViewModel);
        }

        public bool Equals(CategoryViewModel other) {
            return other != null && Name == other.Name
                && Category.Id == other.Category.Id;
        }

#if REVIT_2023_OR_LESS
        public override int GetHashCode() {
            return 539060726 + EqualityComparer<string>.Default.GetHashCode(Name)
                + EqualityComparer<int>.Default.GetHashCode(Category.Id.IntegerValue);
        }
#else
        public override int GetHashCode() {
            return 539060726 + EqualityComparer<string>.Default.GetHashCode(Name)
                + EqualityComparer<int>.Default.GetHashCode((int) Category.Id.GetIdValue());
        }
#endif
    }
}
