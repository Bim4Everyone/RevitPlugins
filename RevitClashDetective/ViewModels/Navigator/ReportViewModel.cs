using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using System.Windows.Threading;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitClashDetective.Models;
using RevitClashDetective.Models.Clashes;
using RevitClashDetective.Models.Interfaces;
using RevitClashDetective.ViewModels.Services;

namespace RevitClashDetective.ViewModels.Navigator {
    internal class ReportViewModel : BaseViewModel, INamedEntity, IEquatable<ReportViewModel> {
        private RevitRepository _revitRepository;

        private string _name;
        private string _message;
        private DispatcherTimer _timer;
        private List<ClashViewModel> _allClashes;
        private List<ClashViewModel> _clashes;

        public ReportViewModel(RevitRepository revitRepository, string name) {
            Initialize(revitRepository, name);
            InitializeClashesFromPluginFile();
        }

        public ReportViewModel(RevitRepository revitRepository, string name, ICollection<ClashModel> clashes) {
            Initialize(revitRepository, name);
            if(clashes != null) {
                InitializeClashes(clashes);
            }
        }

        public string Name {
            get => _name;
            set => this.RaiseAndSetIfChanged(ref _name, value);
        }

        public string Message {
            get => _message;
            set => this.RaiseAndSetIfChanged(ref _message, value);
        }

        public List<ClashViewModel> Clashes {
            get => _clashes;
            set => this.RaiseAndSetIfChanged(ref _clashes, value);
        }

        public ICommand SelectClashCommand { get; set; }

        public ICommand SaveCommand { get; set; }

        public ICommand SaveAsCommand { get; set; }

        public ClashesConfig GetUpdatedConfig() {
            var config = ClashesConfig.GetClashesConfig(_revitRepository.GetObjectName(), Name);

            var notValidClashes = _allClashes.Except(Clashes)
                                             .Select(item => item.GetClashModel());

            config.Clashes = Clashes.Select(item => item.GetClashModel())
                .Union(notValidClashes)
                .ToList();

            return config;
        }

        private void Initialize(RevitRepository revitRepository, string name) {
            _revitRepository = revitRepository;
            Name = name;

            InitializeTimer();

            SelectClashCommand = new RelayCommand(SelectClash, CanSelectClash);
            SaveCommand = new RelayCommand(Save);
            SaveAsCommand = new RelayCommand(SaveAs);
        }

        private void Save(object p) {
            GetUpdatedConfig().SaveProjectConfig();
            Message = "Файл успешно сохранен";
            RefreshMessage();
        }

        private void SaveAs(object p) {
            var config = GetUpdatedConfig();
            var saver = new ConfigSaverService();
            saver.Save(config);
            Message = "Файл успешно сохранен";
            RefreshMessage();
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

        private void InitializeTimer() {
            _timer = new DispatcherTimer {
                Interval = new TimeSpan(0, 0, 0, 3)
            };
            _timer.Tick += (s, a) => { Message = null; _timer.Stop(); };
        }

        private void RefreshMessage() {
            _timer.Start();
        }

        public override bool Equals(object obj) {
            return Equals(obj as ReportViewModel);
        }

        public override int GetHashCode() {
            int hashCode = 1681366416;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            hashCode = hashCode * -1521134295 + EqualityComparer<List<ClashViewModel>>.Default.GetHashCode(Clashes);
            return hashCode;
        }

        public bool Equals(ReportViewModel other) {
            return other != null
                && Name == other.Name
                && EqualityComparer<List<ClashViewModel>>.Default.Equals(Clashes, other.Clashes);
        }
    }
}