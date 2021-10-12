using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using dosymep.WPF.ViewModels;

using RevitFamilyExplorer.Views;

namespace RevitFamilyExplorer.ViewModels {
    internal class SectionViewModel : BaseViewModel {
        private readonly CategoriesView _categoriesView;

        public SectionViewModel(IEnumerable<DirectoryInfo> sectionFolders) {
            _categoriesView = new CategoriesView() { DataContext = this };
            Categories = new ObservableCollection<CategoryViewModel>(GetCategories(sectionFolders));
        }

        public string Name { get; set; }
        public ObservableCollection<CategoryViewModel> Categories { get; }

        private IEnumerable<CategoryViewModel> GetCategories(IEnumerable<DirectoryInfo> sectionFolders) {
            return sectionFolders.Select(item => new CategoryViewModel(item));
        }

        public CategoriesView CategoriesView {
            get { return _categoriesView; }
        }
    }
}
