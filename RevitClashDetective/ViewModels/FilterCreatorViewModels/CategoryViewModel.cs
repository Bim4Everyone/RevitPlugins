using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

using RevitClashDetective.Models;

namespace RevitClashDetective.ViewModels.FilterCreatorViewModels {
    internal class CategoryViewModel : BaseViewModel, IEquatable<CategoryViewModel> {
        private string _name;

        public CategoryViewModel(Category category) {
            Category = category;
            Name = Category.Name;
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

        public override int GetHashCode() {
            return 539060726 + EqualityComparer<string>.Default.GetHashCode(Name)
                + EqualityComparer<int>.Default.GetHashCode(Category.Id.IntegerValue);
        }
    }
}
