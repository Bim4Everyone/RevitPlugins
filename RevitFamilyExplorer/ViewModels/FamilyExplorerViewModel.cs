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
        private readonly FamilyRepository _familyRepository;

        public FamilyExplorerViewModel(FamilyRepository familyRepository) {
            _familyRepository = familyRepository;
            Sections = new ObservableCollection<SectionViewModel> {
                new SectionViewModel(_familyRepository.GetAR()) { Name = "AR" },
                new SectionViewModel(_familyRepository.GetKR()) { Name = "AR" },
                new SectionViewModel(_familyRepository.GetOV()) { Name = "AR" },
                new SectionViewModel(_familyRepository.GetVK()) { Name = "AR" },
                new SectionViewModel(_familyRepository.GetSS()) { Name = "AR" }
            };
        }

        public ObservableCollection<SectionViewModel> Sections { get; }
    }
}