using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using System.Windows.Threading;

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

using Wpf.Ui;

namespace RevitClashDetective.ViewModels.Navigator;
internal class ReportViewModel : BaseViewModel, INamedEntity, IEquatable<ReportViewModel> {
    private RevitRepository _revitRepository;

    private string _name;
    private string _message;
    private DispatcherTimer _timer;
    private List<ClashViewModel> _allClashes;
    private ObservableCollection<ClashViewModel> _clashes;
    private IClashViewModel _selectedGuiClash;
    private ObservableCollection<IClashViewModel> _guiClashes;
    private ObservableCollection<IClashViewModel> _selectedGuiClashes = [];
    private double _firstIntersectionPercentage;
    private double _secondIntersectionPercentage;
    private bool _showImaginaryClashes;
    private ImaginaryFirstClashViewModel[] _imaginaryFirstClashes;
    private ImaginarySecondClashViewModel[] _imaginarySecondClashes;
    private readonly ILocalizationService _localization;
    private readonly IContentDialogService _contentDialogService;
    private readonly SettingsConfig _settingsConfig;

    public ReportViewModel(RevitRepository revitRepository,
        string name,
        ILocalizationService localization,
        IOpenFileDialogService openFileDialogService,
        ISaveFileDialogService saveFileDialogService,
        IMessageBoxService messageBoxService,
        IContentDialogService contentDialogService,
        SettingsConfig settingsConfig,
        ICollection<ClashModel> clashes = null) {

        _localization = localization ?? throw new ArgumentNullException(nameof(localization));
        OpenFileDialogService = openFileDialogService ?? throw new ArgumentNullException(nameof(openFileDialogService));
        SaveFileDialogService = saveFileDialogService ?? throw new ArgumentNullException(nameof(saveFileDialogService));
        MessageBoxService = messageBoxService ?? throw new ArgumentNullException(nameof(messageBoxService));
        _contentDialogService = contentDialogService ?? throw new ArgumentNullException(nameof(contentDialogService));
        _settingsConfig = settingsConfig ?? throw new ArgumentNullException(nameof(settingsConfig));

        Initialize(revitRepository, name);
        if(clashes is not null) {
            InitializeClashes(clashes);
        } else {
            InitializeClashesFromPluginFile();
        }
    }


    public string Name {
        get => _name;
        set => RaiseAndSetIfChanged(ref _name, value);
    }

    public string Message {
        get => _message;
        set => RaiseAndSetIfChanged(ref _message, value);
    }

    public IClashViewModel SelectedGuiClash {
        get => _selectedGuiClash;
        set {
            if(!(_selectedGuiClash?.Equals(value) ?? false)) {
                if(_selectedGuiClash is not null) {
                    _selectedGuiClash.PropertyChanged -= OnSelectedGuiClashChanged;
                }
                RaiseAndSetIfChanged(ref _selectedGuiClash, value);
                if(_selectedGuiClash is not null) {
                    _selectedGuiClash.PropertyChanged += OnSelectedGuiClashChanged;
                }
            }
        }
    }

    public ObservableCollection<IClashViewModel> GuiClashes {
        get => _guiClashes;
        private set => RaiseAndSetIfChanged(ref _guiClashes, value);
    }

    public ObservableCollection<IClashViewModel> SelectedGuiClashes {
        get => _selectedGuiClashes;
        set => RaiseAndSetIfChanged(ref _selectedGuiClashes, value);
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

    public ICommand PickFirstElementsCommand { get; private set; }

    public ICommand PickSecondElementsCommand { get; private set; }

    public IOpenFileDialogService OpenFileDialogService { get; }

    public ISaveFileDialogService SaveFileDialogService { get; }

    public IMessageBoxService MessageBoxService { get; }


    private ObservableCollection<ClashViewModel> Clashes {
        get => _clashes;
        set => RaiseAndSetIfChanged(ref _clashes, value);
    }

    public ClashesConfig GetUpdatedConfig() {
        var config = ClashesConfig.GetClashesConfig(_revitRepository.GetObjectName(), Name);

        var notValidClashes = _allClashes.Except(Clashes)
                                         .Select(item => item.GetClashModel());

        config.Clashes = Clashes.Select(item => item.GetClashModel())
            .Union(notValidClashes)
            .ToList();

        return config;
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

    private void Initialize(RevitRepository revitRepository, string name) {
        _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
        Name = name ?? throw new ArgumentNullException(nameof(name));

        InitializeTimer();

        SaveCommand = RelayCommand.Create(Save);
        SaveAsCommand = RelayCommand.Create(SaveAs);
        ShowImaginaryClashesChangedCommand = RelayCommand.Create(ShowImaginaryClashesChanged);
        PickFirstElementsCommand = RelayCommand.Create(
            PickFirstElements, CanPickElements);
        PickSecondElementsCommand = RelayCommand.Create(
            PickSecondElements, CanPickElements);
    }

    private void Save() {
        GetUpdatedConfig().SaveProjectConfig();
        Message = "Файл успешно сохранен";
        RefreshMessage();
    }

    private void SaveAs() {
        var config = GetUpdatedConfig();
        var saver = new ConfigSaverService(_revitRepository, SaveFileDialogService);
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
        string[] paramsNames = _settingsConfig.ParamNames;
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
            .Select(c => new CheckViewModel(_revitRepository,
            _localization,
            OpenFileDialogService,
            SaveFileDialogService,
            MessageBoxService,
            _settingsConfig,
            filters,
            _contentDialogService,
            c))
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

    private void OnSelectedGuiClashChanged(object sender, PropertyChangedEventArgs e) {
        if(string.IsNullOrWhiteSpace(e.PropertyName)) {
            return;
        }
        var prop = typeof(IClashViewModel).GetProperty(e.PropertyName);
        if(prop is null) {
            return;
        }
        if(prop.Name == nameof(IClashViewModel.ClashStatus)) {
            // мультиредактирование только для статуса коллизии
            object newValue = prop.GetValue(SelectedGuiClash);

            IClashViewModel[] sheets = [.. SelectedGuiClashes];
            foreach(var item in sheets) {
                if(!item.Equals(SelectedGuiClash)) {
                    prop.SetValue(item, newValue);
                }
            }
        }
    }

    private void PickFirstElements() {
        List<ElementModel> elements = [];
        foreach(var clash in SelectedGuiClashes) {
            try {
                elements.Add(clash.GetFirstElement());
            } catch(NotSupportedException) {
                continue;
            }
        }
        _revitRepository.SelectElements(elements);
    }

    private void PickSecondElements() {
        List<ElementModel> elements = [];
        foreach(var clash in SelectedGuiClashes) {
            try {
                elements.Add(clash.GetSecondElement());
            } catch(NotSupportedException) {
                continue;
            }
        }
        _revitRepository.SelectElements(elements);
    }

    private bool CanPickElements() {
        return SelectedGuiClashes?.Count > 0;
    }
}
