using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using dosymep.WPF.ViewModels;

namespace RevitFamilyExplorer.ViewModels {
    internal class CategoryViewModel : BaseViewModel {
        private readonly DirectoryInfo _categoryFolder;

        public CategoryViewModel(DirectoryInfo categoryFolder) {
            _categoryFolder = categoryFolder;

            Name = _categoryFolder.Name;
            FamilyFiles = new ObservableCollection<FamilyFileViewModel>(GetFamilyFiles());
        }

        public string Name { get; }
        public ObservableCollection<FamilyFileViewModel> FamilyFiles { get; }

        private IEnumerable<FamilyFileViewModel> GetFamilyFiles() {
            return _categoryFolder.GetFiles("*.rfa").Select(item => new FamilyFileViewModel(item));
        }
    }
}
