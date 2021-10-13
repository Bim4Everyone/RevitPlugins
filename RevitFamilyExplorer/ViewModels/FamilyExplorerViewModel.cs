using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

            Sections = new ObservableCollection<SectionViewModel> {
                new SectionViewModel(_revitRepository, _familyRepository.GetAR()) { Name = "AR" },
                new SectionViewModel(_revitRepository, _familyRepository.GetKR()) { Name = "KR" },
                new SectionViewModel(_revitRepository, _familyRepository.GetOV()) { Name = "OV" },
                new SectionViewModel(_revitRepository, _familyRepository.GetVK()) { Name = "VK" },
                new SectionViewModel(_revitRepository, _familyRepository.GetSS()) { Name = "SS" }
            };
        }

        public ObservableCollection<SectionViewModel> Sections { get; }
    }
}