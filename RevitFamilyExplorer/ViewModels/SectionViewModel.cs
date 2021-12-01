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
        private readonly RevitRepository _revitRepository;
        private readonly FamilyRepository _familyRepository;

        private readonly FileInfo _sectionFile;
        private readonly CategoriesView _categoriesView;
        private ObservableCollection<CategoryViewModel> _categories;

        public SectionViewModel(RevitRepository revitRepository, FamilyRepository familyRepository, FileInfo sectionFile) {
            _revitRepository = revitRepository;
            _familyRepository = familyRepository;

            _sectionFile = sectionFile;
            _categoriesView = new CategoriesView() { DataContext = this };
        }

        public string Name { get; set; }
        public ObservableCollection<CategoryViewModel> Categories {
            get => _categories;
            set => this.RaiseAndSetIfChanged(ref _categories, value);
        }

        public CategoriesView CategoriesView {
            get { return _categoriesView; }
        }

        public void LoadCategories() {
            if(_categories == null) {
                Categories = new ObservableCollection<CategoryViewModel>(GetCategories());
            }
        }

        private IEnumerable<CategoryViewModel> GetCategories() {
            return _familyRepository.GetSectionInternal(_sectionFile.FullName)
                .Select(item => new CategoryViewModel(_revitRepository, item));
        }
    }
}