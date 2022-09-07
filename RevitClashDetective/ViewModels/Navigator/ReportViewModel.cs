using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitClashDetective.Models;
using RevitClashDetective.Models.Clashes;
using RevitClashDetective.Models.Interfaces;

namespace RevitClashDetective.ViewModels.Navigator {
    internal class ReportViewModel : BaseViewModel, INamedEntity {
        private readonly RevitRepository _revitRepository;

        private string _name;
        private List<ClashViewModel> _allClashes;
        private List<ClashViewModel> _clashes;

        public ReportViewModel(RevitRepository revitRepository, string name) {
            _revitRepository = revitRepository;
            Name = name;
            InitializeClashesFromPluginFile();

            SelectClashCommand = new RelayCommand(SelectClash, CanSelectClash);
        }

        public ReportViewModel(RevitRepository revitRepository, string name, ICollection<ClashModel> clashes) {
            _revitRepository = revitRepository;
            Name = name;
            if(clashes != null) {
                InitializeClashes(clashes);
            }

            SelectClashCommand = new RelayCommand(SelectClash, CanSelectClash);
        }

        public string Name {
            get => _name;
            set => this.RaiseAndSetIfChanged(ref _name, value);
        }

        public List<ClashViewModel> Clashes {
            get => _clashes;
            set => this.RaiseAndSetIfChanged(ref _clashes, value);
        }

        public ICommand SelectClashCommand { get; }

        public void Save() {
            var config = ClashesConfig.GetClashesConfig(_revitRepository.GetObjectName(), Name);

            var notValidClashes = _allClashes.Except(Clashes)
                                             .Select(item => item.GetClashModel());

            config.Clashes = Clashes.Select(item => item.GetClashModel())
                .Union(notValidClashes)
                .ToList();

            config.SaveProjectConfig();
        }

        private void InitializeClashesFromPluginFile() {
            if(Name != null) {
                var config = ClashesConfig.GetClashesConfig(_revitRepository.GetObjectName(), Name);
                InitializeClashes(config.Clashes);
            }
        }

        private void InitializeClashes(ICollection<ClashModel> clashModels) {
            _allClashes = clashModels.Select(item => new ClashViewModel(_revitRepository, item))
                                     .ToList();
            var documentNames = _revitRepository.GetDocuments().Select(item => item.Title).ToList();
            Clashes = _allClashes.Where(item => IsValid(documentNames, item))
                                 .ToList();
        }

        private bool IsValid(List<string> documentNames, ClashViewModel clash) {
            var clashDocuments = new[] { clash.FirstDocumentName, clash.SecondDocumentName };
            var clashElements = new[] {_revitRepository.GetElement(clash.Clash.MainElement.DocumentName, clash.Clash.MainElement.Id),
                                       _revitRepository.GetElement(clash.Clash.OtherElement.DocumentName, clash.Clash.OtherElement.Id)};

            return clashDocuments.All(item => documentNames.Any(d => d.Contains(item))) && clashElements.Any(item => item != null);
        }

        private void SelectClash(object p) {
            var clash = (ClashViewModel) p;

            var elements = new[] {_revitRepository.GetElement(clash.Clash.MainElement.DocumentName, clash.Clash.MainElement.Id),
                                  _revitRepository.GetElement(clash.Clash.OtherElement.DocumentName, clash.Clash.OtherElement.Id)};
            _revitRepository.SelectAndShowElement(elements.Where(item => item != null));
        }

        private bool CanSelectClash(object p) {
            return p != null && p is ClashViewModel;
        }
    }
}
