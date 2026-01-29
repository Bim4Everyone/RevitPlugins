using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

namespace RevitTagAllCategories.ViewModels {
    internal class CategoryViewModel : BaseViewModel {
        private readonly Category _category;
        private readonly string _name;

        public CategoryViewModel(Category category) {
            _category = category;
            _name = category.Name;
        }

        public string Name => _name;
        public Category Category => _category;
    }
}
