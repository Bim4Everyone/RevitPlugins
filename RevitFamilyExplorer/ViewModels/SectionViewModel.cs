using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using dosymep.WPF.ViewModels;

namespace RevitFamilyExplorer.ViewModels {
    internal class SectionViewModel : BaseViewModel {
        public SectionViewModel(IEnumerable<DirectoryInfo> sectionFolders) {
            Categories = new ObservableCollection<CategoryViewModel>(GetCategories(sectionFolders));
        }

        public string Name { get; set; }
        public ObservableCollection<CategoryViewModel> Categories { get; }

        private IEnumerable<CategoryViewModel> GetCategories(IEnumerable<DirectoryInfo> sectionFolders) {
            return sectionFolders.Select(item => new CategoryViewModel(item));
        }
    }
}
