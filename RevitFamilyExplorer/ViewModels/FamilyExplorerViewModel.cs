using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using dosymep.WPF.ViewModels;

using RevitFamilyExplorer.Models;

namespace RevitFamilyExplorer.ViewModels {
    internal class FamilyExplorerViewModel : BaseViewModel {
        private readonly RevitRepository _revitRepository;
        private readonly FamilyRepository _familyRepository;

        public FamilyExplorerViewModel(RevitRepository revitRepository, FamilyRepository familyRepository) {
            _revitRepository = revitRepository;
            _familyRepository = familyRepository;

            Sections = new ObservableCollection<SectionViewModel>(GetSections());
        }

        public ObservableCollection<SectionViewModel> Sections { get; }

        private IEnumerable<SectionViewModel> GetSections() {
            return _familyRepository.GetSections()
                .Select(item => new SectionViewModel(_revitRepository, _familyRepository.GetSection(item.FullName)) { Name = Path.GetFileNameWithoutExtension(item.Name) });
        }
    }
}