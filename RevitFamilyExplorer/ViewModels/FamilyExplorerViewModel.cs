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

        private SectionViewModel _section;
        private ObservableCollection<SectionViewModel> _sections;

        public FamilyExplorerViewModel(RevitRepository revitRepository, FamilyRepository familyRepository) {
            _revitRepository = revitRepository;
            _familyRepository = familyRepository;
        }

        public SectionViewModel Section {
            get => _section;
            set {
                this.RaiseAndSetIfChanged(ref _section, value);
                _section?.LoadCategories();
            }
        }

        public ObservableCollection<SectionViewModel> Sections {
            get => _sections;
            private set => this.RaiseAndSetIfChanged(ref _sections, value);
        }

        public void LoadSections() {
            Sections = new ObservableCollection<SectionViewModel>(GetSections());
            Section = Sections.FirstOrDefault();
        }

        private IEnumerable<SectionViewModel> GetSections() {
            return _familyRepository.GetSectionsInternal()
                .Select(item => new SectionViewModel(_revitRepository, _familyRepository, item) { Name = Path.GetFileNameWithoutExtension(item.Name) });
        }
    }
}