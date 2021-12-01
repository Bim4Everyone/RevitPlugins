using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using dosymep.WPF.ViewModels;

using RevitFamilyExplorer.Models;
using RevitFamilyExplorer.Views;

namespace RevitFamilyExplorer.ViewModels {
    internal class SectionViewModel : BaseViewModel {
        private readonly CategoriesView _categoriesView;
        private readonly RevitRepository _revitRepository;
        private readonly List<DirectoryInfo> _sectionFolders;
        
        private ObservableCollection<CategoryViewModel> _categories;

        public SectionViewModel(RevitRepository revitRepository, IEnumerable<DirectoryInfo> sectionFolders) {
            _revitRepository = revitRepository;
            _sectionFolders = new List<DirectoryInfo>(sectionFolders);
            _categoriesView = new CategoriesView() { DataContext = this };
        }

        public string Name { get; set; }
        public ObservableCollection<CategoryViewModel> Categories { 
            get { 
                if(_categories == null) {
                    _categories = new ObservableCollection<CategoryViewModel>(GetCategories(_sectionFolders));
                }

                return _categories;
            }
        }

        private IEnumerable<CategoryViewModel> GetCategories(IEnumerable<DirectoryInfo> sectionFolders) {
            return sectionFolders.Select(item => new CategoryViewModel(_revitRepository, item));
        }

        public CategoriesView CategoriesView {
            get { return _categoriesView; }
        }
    }
}
