using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitFamilyExplorer.ViewModels {
    internal class SectionViewModel {
        public SectionViewModel(IEnumerable<DirectoryInfo> sectionFolders) {
            FamilyFiles = new ObservableCollection<FamilyFileViewModel>(GetFamilyFiles(sectionFolders));
        }

        public string Name { get; set; }
        public ObservableCollection<FamilyFileViewModel> FamilyFiles { get; }

        private IEnumerable<FamilyFileViewModel> GetFamilyFiles(IEnumerable<DirectoryInfo> sectionFolders) {
            return sectionFolders
                .SelectMany(item => item.GetFiles("*.rfa"))
                .Select(item => new FamilyFileViewModel(item))
                .OrderBy(item => item.Name);
        }
    }
}
