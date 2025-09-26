using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using System.Windows.Threading;

using dosymep.Bim4Everyone.ProjectConfigs;
using dosymep.Revit.Geometry;
using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitClashDetective.Models;
using RevitClashDetective.Models.Clashes;
using RevitClashDetective.Models.FilterModel;
using RevitClashDetective.Models.Interfaces;
using RevitClashDetective.ViewModels.ClashDetective;
using RevitClashDetective.ViewModels.Services;

namespace RevitClashDetective.ViewModels.Navigator {
    internal class ReportViewModel : BaseViewModel, INamedEntity, IEquatable<ReportViewModel> {
        private RevitRepository _revitRepository;

        private string _name;
        private string _message;
        private DispatcherTimer _timer;
        private List<ClashViewModel> _allClashes;
        private ObservableCollection<ClashViewModel> _clashes;
        private ObservableCollection<IClashViewModel> _guiClashes;
        private double _firstIntersectionPercentage;
        private double _secondIntersectionPercentage;
        private bool _showImaginaryClashes;
        private ImaginaryFirstClashViewModel[] _imaginaryFirstClashes;
        private ImaginarySecondClashViewModel[] _imaginarySecondClashes;
        private readonly ILocalizationService _localization;

        public ReportViewModel(RevitRepository revitRepository, string name, ILocalizationService localization) {
            Initialize(revitRepository, name);
            InitializeClashesFromPluginFile();
            _localization = localization ?? throw new ArgumentNullException(nameof(localization));
        }

        public ReportViewModel(
            RevitRepository revitRepository,
            string name,
            ICollection<ClashModel> clashes,
            ILocalizationService localization) {

            Initialize(revitRepository, name);
            if(clashes != null) {
                InitializeClashes(clashes);
            }

            _localization = localization ?? throw new ArgumentNullException(nameof(localization));
        }

        public string Name {
            get => _name;
            set => this.RaiseAndSetIfChanged(ref _name, value);
        }

        public string Message {
            get => _message;
            set => this.RaiseAndSetIfChanged(ref _message, value);
        }

        private ObservableCollection<ClashViewModel> Clashes {
            get => _clashes;
            set => this.RaiseAndSetIfChanged(ref _clashes, value);
        }

        public ObservableCollection<IClashViewModel> GuiClashes {
            get => _guiClashes;
            private set => RaiseAndSetIfChanged(ref _guiClashes, value);
        }

        public double FirstIntersectionPercentage {
            get => _firstIntersectionPercentage;
            private set => RaiseAndSetIfChanged(ref _firstIntersectionPercentage, value);
        }

        public double SecondIntersectionPercentage {
            get => _secondIntersectionPercentage;
            private set => RaiseAndSetIfChanged(ref _secondIntersectionPercentage, value);
        }

        public bool ShowImaginaryClashes {
            get => _showImaginaryClashes;
            set => RaiseAndSetIfChanged(ref _showImaginaryClashes, value);
        }


        public ICommand SaveCommand { get; private set; }

        public ICommand SaveAsCommand { get; private set; }

        public ICommand ShowImaginaryClashesChangedCommand { get; private set; }


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

            SaveCommand = RelayCommand.Create(Save);
            SaveAsCommand = RelayCommand.Create(SaveAs);
            ShowImaginaryClashesChangedCommand = RelayCommand.Create(ShowImaginaryClashesChanged);
        }

        private void Save() {
            GetUpdatedConfig().SaveProjectConfig();
            Message = "Файл успешно сохранен";
            RefreshMessage();
        }

        private void SaveAs() {
            var config = GetUpdatedConfig();
            var saver = new ConfigSaverService(_revitRepository);
            saver.Save(config);
            Message = "Файл успешно сохранен";
            RefreshMessage();
        }

        private void InitializeClashesFromPluginFile() {
            if(Name != null) {
                var config = ClashesConfig.GetClashesConfig(_revitRepository.GetObjectName(), Name);
                InitializeClashes(config.Clashes.Select(item => item.SetRevitRepository(_revitRepository)));
            }
        }

        private void InitializeClashes(IEnumerable<ClashModel> clashModels) {
            _allClashes = clashModels.Select(item => new ClashViewModel(_revitRepository, item))
                                     .ToList();
            var documentNames = _revitRepository.DocInfos.Select(item => item.Doc.Title).ToList();
            Clashes = new ObservableCollection<ClashViewModel>(_allClashes.Where(item => IsValid(documentNames, item)));

            GuiClashes = new ObservableCollection<IClashViewModel>(Clashes);
            SetAdditionalParamValues(GuiClashes);
            SetIntersectionPercentage(GuiClashes);
        }

        private bool IsValid(List<string> documentNames, ClashViewModel clash) {
            return clash.Clash.IsValid(documentNames);
        }

        private void SetIntersectionPercentage(ICollection<IClashViewModel> clashes) {
            double firstTotalVolume = clashes
                .Select(GetFirstElementViewModel)
                .Where(v => v is not null)
                .Distinct()
                .Sum(e => e.ElementVolume);
            double secondTotalVolume = clashes
                .Select(GetSecondElementViewModel)
                .Where(v => v is not null)
                .Distinct()
                .Sum(e => e.ElementVolume);
            double collisionTotalVolume = clashes.Select(c => c.IntersectionVolume).Sum();

            FirstIntersectionPercentage = Math.Round(collisionTotalVolume / firstTotalVolume * 100, 2);
            SecondIntersectionPercentage = Math.Round(collisionTotalVolume / secondTotalVolume * 100, 2);
        }

        private void SetAdditionalParamValues(ICollection<IClashViewModel> clashes) {
            string[] paramsNames = SettingsConfig.GetSettingsConfig(GetPlatformService<IConfigSerializer>()).ParamNames;
            if(paramsNames.Length > 0) {
                foreach(var clash in clashes) {
                    clash.SetElementParams(paramsNames);
                }
            }
        }

        private ElementViewModel GetFirstElementViewModel(IClashViewModel clash) {
            try {
                return new ElementViewModel(clash.GetFirstElement(), clash.FirstElementVolume);
            } catch(NotSupportedException) {
                return null;
            }
        }

        private ElementViewModel GetSecondElementViewModel(IClashViewModel clash) {
            try {
                return new ElementViewModel(clash.GetSecondElement(), clash.SecondElementVolume);
            } catch(NotSupportedException) {
                return null;
            }
        }

        private void ShowImaginaryClashesChanged() {
            if(ShowImaginaryClashes) {
                FindImaginaryClashes();
                GuiClashes = new ObservableCollection<IClashViewModel>(
                    [.. Clashes, .. _imaginaryFirstClashes, .. _imaginarySecondClashes]);
            } else {
                GuiClashes = new ObservableCollection<IClashViewModel>(Clashes);
            }
            SetIntersectionPercentage(GuiClashes);
        }

        private CheckViewModel GetCheckViewModel() {
            string path = Path.Combine(_revitRepository.GetObjectName(), _revitRepository.GetDocumentName());
            var checks = ChecksConfig.GetChecksConfig(path, _revitRepository.Doc);
            var filters = FiltersConfig.GetFiltersConfig(path, _revitRepository.Doc);

            return checks.Checks
                .Select(c => new CheckViewModel(_revitRepository, _localization, filters, c))
                .FirstOrDefault(c => c.ReportName.Equals(Name));
        }

        private void FindImaginaryClashes() {
            if(_imaginaryFirstClashes is null || _imaginarySecondClashes is null) {
                var checkViewModel = GetCheckViewModel();
                if(checkViewModel is not null) {
                    (var firstProviders, var secondProviders) = checkViewModel.GetProviders();
                    _imaginaryFirstClashes = FilterElements(Clashes, GetElementViewModels(firstProviders))
                        .Select(e => new ImaginaryFirstClashViewModel(_revitRepository, e))
                        .ToArray();
                    _imaginarySecondClashes = FilterElements(Clashes, GetElementViewModels(secondProviders))
                        .Select(e => new ImaginarySecondClashViewModel(_revitRepository, e))
                        .ToArray();
                    SetAdditionalParamValues(_imaginaryFirstClashes);
                    SetAdditionalParamValues(_imaginarySecondClashes);
                } else {
                    _imaginaryFirstClashes = [];
                    _imaginarySecondClashes = [];
                }
            }
        }

        /// <summary>
        /// Находит элементы, которые не участвуют в коллизиях
        /// </summary>
        /// <param name="clashes">Коллизии</param>
        /// <param name="elementsToFilter">Элементы для проверки</param>
        /// <returns>Элементы из заданной коллекции, которые не участвуют в заданных коллизиях</returns>
        private ICollection<ElementViewModel> FilterElements(
            ICollection<ClashViewModel> clashes,
            ICollection<ElementViewModel> elementsToFilter) {

            var clashElements = clashes.SelectMany(c => new[] { c.Clash.MainElement, c.Clash.OtherElement })
                .ToHashSet();

            return [.. elementsToFilter.Where(e => !clashElements.Contains(e.Element))];
        }

        private ICollection<ElementViewModel> GetElementViewModels(ICollection<IProvider> providers) {
            return [.. providers.SelectMany(GetElementViewModels)];
        }

        private ICollection<ElementViewModel> GetElementViewModels(IProvider provider) {
            return [.. provider.GetElements()
                .Select(e => new ElementViewModel(
                    new ElementModel(e, provider.MainTransform),
                    _revitRepository.ConvertToM3(e.GetSolids().Sum(s => s.GetVolumeOrDefault(0) ?? 0))))];
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
            return hashCode;
        }

        public bool Equals(ReportViewModel other) {
            return other != null
                && Name == other.Name;
        }
    }
}
