using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

using RevitClashDetective.Models;

namespace RevitClashDetective.ViewModels.FilterCreatorViewModels {
    internal class CategoryViewModel : BaseViewModel {
        private string _name;
        private bool _isSelected;

        public CategoryViewModel(Category category) {
            Category = category;
            Name = Category.Name;
        }

        public string Name {
            get => _name;
            set => this.RaiseAndSetIfChanged(ref _name, value);
        }

        public bool IsSelected {
            get => _isSelected;
            set => this.RaiseAndSetIfChanged(ref _isSelected, value);
        }

        public Category Category { get; }
    }
}
