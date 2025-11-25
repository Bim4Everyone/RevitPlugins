using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;

using dosymep.Bim4Everyone.SimpleServices;
using dosymep.Revit;
using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitBuildCoordVolumes.Models;
using RevitBuildCoordVolumes.Models.Enums;
using RevitBuildCoordVolumes.Models.Interfaces;
using RevitBuildCoordVolumes.Models.Services;
using RevitBuildCoordVolumes.Models.Settings;

namespace RevitBuildCoordVolumes.ViewModels;

internal class MainViewModel : BaseViewModel {
    private readonly PluginConfig _pluginConfig;
    private readonly RevitRepository _revitRepository;
    private readonly ILocalizationService _localizationService;
    private readonly IParamAvailabilityService _paramAvailabilityService;
    private readonly ICategoryAvailabilityService _categoryAvailabilityService;
    private readonly IRevitParamFactory _revitParamFactory;
    private readonly ProvidersFactory _providersFactory;
    private BuildCoordVolumesSettings _buildCoordVolumesSettings;
    private ObservableCollection<DocumentViewModel> _documents;
    private ObservableCollection<DocumentViewModel> _filteredDocuments;
    private ObservableCollection<TypeZoneViewModel> _typeZones;
    private TypeZoneViewModel _selectedTypeZone;
    private ObservableCollection<PositionViewModel> _positionUp;
    private PositionViewModel _selectedPositionUp;
    private ObservableCollection<PositionViewModel> _positionBottom;
    private PositionViewModel _selectedPositionBottom;
    private string _searchSide;
    private ObservableCollection<ParamViewModel> _params;
    private ObservableCollection<SlabViewModel> _slabs;
    private ObservableCollection<SlabViewModel> _filteredSlabs;
    private bool _hasParamWarning;
    private string _errorText;
    private string _searchTextDocs;
    private string _searchTextSlabs;

    public MainViewModel(
        PluginConfig pluginConfig,
        RevitRepository revitRepository,
        ILocalizationService localizationService,
        IProgressDialogFactory progressDialogFactory,
        IRevitParamFactory revitParamFactory) {

        _pluginConfig = pluginConfig;
        _revitRepository = revitRepository;
        _localizationService = localizationService;
        _paramAvailabilityService = new ParamAvailabilityService();
        _categoryAvailabilityService = new CategoryAvailabilityService(_revitRepository.Document);
        _revitParamFactory = revitParamFactory;
        _providersFactory = new ProvidersFactory();

        LoadViewCommand = RelayCommand.Create(LoadView);
        SearchDocsCommand = RelayCommand.Create(ApplySearchDocs);
        SearchSlabsCommand = RelayCommand.Create(ApplySearchSlabs);
        AcceptViewCommand = RelayCommand.Create(AcceptView, CanAcceptView);

        ProgressDialogFactory = progressDialogFactory
            ?? throw new System.ArgumentNullException(nameof(progressDialogFactory));
    }

    public ICommand LoadViewCommand { get; }
    public ICommand AcceptViewCommand { get; }
    public ICommand SearchDocsCommand { get; }
    public ICommand SearchSlabsCommand { get; }

    public IProgressDialogFactory ProgressDialogFactory { get; }

    public ObservableCollection<DocumentViewModel> Documents {
        get => _documents;
        set => RaiseAndSetIfChanged(ref _documents, value);
    }
    public ObservableCollection<DocumentViewModel> FilteredDocuments {
        get => _filteredDocuments;
        set => RaiseAndSetIfChanged(ref _filteredDocuments, value);
    }
    public ObservableCollection<TypeZoneViewModel> TypeZones {
        get => _typeZones;
        set => RaiseAndSetIfChanged(ref _typeZones, value);
    }
    public TypeZoneViewModel SelectedTypeZone {
        get => _selectedTypeZone;
        set => RaiseAndSetIfChanged(ref _selectedTypeZone, value);
    }
    public ObservableCollection<PositionViewModel> PositionUp {
        get => _positionUp;
        set => RaiseAndSetIfChanged(ref _positionUp, value);
    }
    public PositionViewModel SelectedPositionUp {
        get => _selectedPositionUp;
        set => RaiseAndSetIfChanged(ref _selectedPositionUp, value);
    }
    public ObservableCollection<PositionViewModel> PositionBottom {
        get => _positionBottom;
        set => RaiseAndSetIfChanged(ref _positionBottom, value);
    }
    public PositionViewModel SelectedPositionBottom {
        get => _selectedPositionBottom;
        set => RaiseAndSetIfChanged(ref _selectedPositionBottom, value);
    }
    public string SearchSide {
        get => _searchSide;
        set => RaiseAndSetIfChanged(ref _searchSide, value);
    }
    public ObservableCollection<ParamViewModel> Params {
        get => _params;
        set => RaiseAndSetIfChanged(ref _params, value);
    }
    public ObservableCollection<SlabViewModel> Slabs {
        get => _slabs;
        set => RaiseAndSetIfChanged(ref _slabs, value);
    }
    public ObservableCollection<SlabViewModel> FilteredSlabs {
        get => _filteredSlabs;
        set => RaiseAndSetIfChanged(ref _filteredSlabs, value);
    }
    public bool HasParamWarning {
        get => _hasParamWarning;
        set => RaiseAndSetIfChanged(ref _hasParamWarning, value);
    }
    public string SearchTextDocs {
        get => _searchTextDocs;
        set => RaiseAndSetIfChanged(ref _searchTextDocs, value);
    }
    public string SearchTextSlabs {
        get => _searchTextSlabs;
        set => RaiseAndSetIfChanged(ref _searchTextSlabs, value);
    }
    public string ErrorText {
        get => _errorText;
        set => RaiseAndSetIfChanged(ref _errorText, value);
    }

    // Метод обновления предупреждений в параметрах
    private void UpdateParamWarnings() {
        foreach(var param in Params) {
            param.UpdateWarning(_revitRepository.Document);
        }
        HasParamWarning = Params.Any(param => param.HasWarning);
    }

    // Метод подписанный на событие изменения ParamViewModel
    private void OnParamChanged(object sender, PropertyChangedEventArgs e) {
        if(sender is not ParamViewModel vm) {
            return;
        }
        switch(e.PropertyName) {
            case nameof(ParamViewModel.SourceParamName):
                UpdateParamWarnings();
                if(!vm.HasWarning) {
                    var def = _paramAvailabilityService.GetDefinitionByName(_revitRepository.Document, vm.SourceParamName);
                    var newParam = _revitParamFactory.Create(_revitRepository.Document, def.GetElementId());
                    vm.ParamMap.SourceParam = newParam;
                    if(vm.ParamMap.Type == ParamType.DescriptionParam) {
                        UpdateTypeZones();
                    }
                }
                break;

            case nameof(ParamViewModel.TargetParamName):
                UpdateParamWarnings();
                if(!vm.HasWarning) {
                    var def = _paramAvailabilityService.GetDefinitionByName(_revitRepository.Document, vm.TargetParamName);
                    var newParam = _revitParamFactory.Create(_revitRepository.Document, def.GetElementId());
                    vm.ParamMap.TargetParam = newParam;
                }
                break;

            case nameof(ParamViewModel.IsChecked):
                UpdateParamWarnings();
                break;
        }
    }

    // Метод подписанный на событие изменения ParamViewModel
    private void OnDocumentChanged(object sender, PropertyChangedEventArgs e) {
        if(sender is not DocumentViewModel vm) {
            return;
        }
        if(e.PropertyName == nameof(vm.IsChecked)) {
            UpdateTypeZones();
            UpdateFilteredSlabs();
        }
    }

    // Метод получения коллекции ParamViewModel для Params
    private IEnumerable<ParamViewModel> GetParamViewModels() {
        var defaultParamMaps = RevitConstants.GetDefaultParamMaps();
        var savedParamMaps = _buildCoordVolumesSettings.ParamMaps;

        var savedLookup = savedParamMaps.ToDictionary(paramMap => paramMap.Type, paramMap => paramMap);

        return defaultParamMaps.Select(defaultParamMap => {
            bool exists = savedLookup.TryGetValue(defaultParamMap.Type, out var savedParamMap);
            var paramMap = exists ? savedParamMap : defaultParamMap;
            bool isPair = paramMap.SourceParam != null;

            return new ParamViewModel(_localizationService, _paramAvailabilityService, _categoryAvailabilityService) {
                ParamMap = paramMap,
                Description = _localizationService.GetLocalizedString($"MainViewModel.{paramMap.Type}Description"),
                DetailDescription = _localizationService.GetLocalizedString($"MainViewModel.{paramMap.Type}DetailDescription"),
                SourceParamName = paramMap.SourceParam?.Name ?? string.Empty,
                TargetParamName = paramMap.TargetParam?.Name ?? string.Empty,
                IsChecked = exists,
                IsPair = isPair
            };
        });
    }

    // Метод получения коллекции PositionViewModel для Positions
    private IEnumerable<PositionViewModel> GetPositionViewModels(bool upPosition) {
        var currentProvider = upPosition
            ? _buildCoordVolumesSettings.UpPositionProvider
            : _buildCoordVolumesSettings.BottomPositionProvider;
        var providers = Enum.GetValues(typeof(PositionProviderType)).Cast<PositionProviderType>();
        return providers
            .Select(provider => new PositionViewModel {
                Name = _localizationService.GetLocalizedString($"MainViewModel.{provider}"),
                PositionProvider = (provider == currentProvider.Type)
                    ? currentProvider
                    : _providersFactory.GetPositionProvider(_revitRepository, provider)
            })
            .OrderByDescending(vm => vm.PositionProvider.Type == currentProvider.Type)
            .ThenBy(vm => vm.Name);
    }

    // Метод получения коллекции TypeModelViewModel для TypeModels
    private IEnumerable<TypeZoneViewModel> GetTypeZoneViewModels() {
        var documents = FilteredDocuments.Where(vm => vm.IsChecked).Select(vm => vm.Document);
        var param = Params.FirstOrDefault()?.ParamMap.SourceParam;
        var typeZones = _revitRepository.GetTypeZones(param);
        return !typeZones.Any()
            ? []
            : typeZones
                .Select(value => new TypeZoneViewModel { Name = value })
                .OrderByDescending(vm => vm.Name.Equals(_buildCoordVolumesSettings.TypeZone))
                .ThenBy(vm => vm.Name);
    }

    // Метод обновления параметра в TypeZones
    private void UpdateFilteredSlabs() {
        FilteredSlabs.Clear();
        Slabs = new ObservableCollection<SlabViewModel>(GetSlabViewModels());
        FilteredSlabs = new ObservableCollection<SlabViewModel>(Slabs);
    }

    // Метод обновления параметра в FilteredSlabs
    private void UpdateTypeZones() {
        TypeZones.Clear();
        TypeZones = new ObservableCollection<TypeZoneViewModel>(GetTypeZoneViewModels());
        SelectedTypeZone = TypeZones.FirstOrDefault();
    }

    // Метод получения коллекции DocumentViewModel для Documents
    private IEnumerable<DocumentViewModel> GetDocumentViewModels() {
        var allDocuments = _revitRepository.GetAllDocuments();
        var savedDocumentsNames = _buildCoordVolumesSettings.Documents
            .Select(doc => doc.GetUniqId());

        return !allDocuments.Any()
            ? []
            : allDocuments
                .Select(document => new DocumentViewModel(_localizationService, document) {
                    IsChecked = savedDocumentsNames.Contains(document.GetUniqId())
                })
                .OrderBy(vm => vm.Name);
    }

    // Метод получения коллекции SlabViewModel для Slabs
    private IEnumerable<SlabViewModel> GetSlabViewModels() {
        var documents = FilteredDocuments.Where(vm => vm.IsChecked).Select(vm => vm.Document);
        var allSlabs = _revitRepository.GetTypeSlabs(documents);
        var savedSlabs = _buildCoordVolumesSettings.TypeSlabs;
        return !allSlabs.Any()
            ? []
            : allSlabs
                .Select(slabType => new SlabViewModel {
                    Name = slabType,
                    IsChecked = savedSlabs.Contains(slabType)
                })
                .OrderBy(vm => vm.Name);
    }

    private void ApplySearchSlabs() {
        FilteredSlabs = string.IsNullOrEmpty(SearchTextSlabs)
            ? new ObservableCollection<SlabViewModel>(Slabs)
            : new ObservableCollection<SlabViewModel>(Slabs
                .Where(item => item.Name
                .IndexOf(SearchTextSlabs, StringComparison.OrdinalIgnoreCase) >= 0));
    }

    private void ApplySearchDocs() {
        FilteredDocuments = string.IsNullOrEmpty(SearchTextDocs)
            ? new ObservableCollection<DocumentViewModel>(Documents)
            : new ObservableCollection<DocumentViewModel>(Documents
                .Where(item => item.Name
                .IndexOf(SearchTextDocs, StringComparison.OrdinalIgnoreCase) >= 0));
    }

    private void LoadView() {
        LoadConfig();

        Documents = new ObservableCollection<DocumentViewModel>(GetDocumentViewModels());
        FilteredDocuments = new ObservableCollection<DocumentViewModel>(Documents);
        // Подписка на события в DocumentViewModel
        foreach(var document in FilteredDocuments) {
            document.PropertyChanged += OnDocumentChanged;
        }

        Params = new ObservableCollection<ParamViewModel>(GetParamViewModels());
        // Подписка на события в ParamViewModel
        foreach(var param in Params) {
            param.PropertyChanged += OnParamChanged;
        }

        UpdateParamWarnings();

        TypeZones = new ObservableCollection<TypeZoneViewModel>(GetTypeZoneViewModels());
        SelectedTypeZone = TypeZones.FirstOrDefault();

        Slabs = new ObservableCollection<SlabViewModel>(GetSlabViewModels());
        FilteredSlabs = new ObservableCollection<SlabViewModel>(Slabs);

        PositionUp = new ObservableCollection<PositionViewModel>(GetPositionViewModels(true));
        SelectedPositionUp = PositionUp.FirstOrDefault();
        PositionBottom = new ObservableCollection<PositionViewModel>(GetPositionViewModels(false));
        SelectedPositionBottom = PositionBottom.FirstOrDefault();
        SearchSide = Convert.ToString(_buildCoordVolumesSettings.SearchSide);

    }
    private void AcceptView() {
        SaveConfig();
        //_revitRepository.Process();
    }

    private bool CanAcceptView() {
        if(FilteredDocuments != null) {
            var checkedDocs = FilteredDocuments.Where(p => p.IsChecked).ToList();
            if(checkedDocs.Count == 0) {
                ErrorText = _localizationService.GetLocalizedString("MainViewModel.NoDocuments");
                return false;
            }
        }
        if(Params != null) {
            var checkedParams = Params.Where(p => p.IsChecked).ToList();
            if(checkedParams.Count == 0) {
                ErrorText = _localizationService.GetLocalizedString("MainViewModel.NoParams");
                return false;
            }
        }
        if(FilteredSlabs != null) {
            var checkedSlabs = FilteredSlabs.Where(p => p.IsChecked).ToList();
            if(checkedSlabs.Count == 0) {
                ErrorText = _localizationService.GetLocalizedString("MainViewModel.NoSlabs");
                return false;
            }
        }
        if(SelectedTypeZone == null) {
            ErrorText = _localizationService.GetLocalizedString("MainViewModel.NoTypeZone");
            return false;
        }
        if(HasParamWarning) {
            ErrorText = _localizationService.GetLocalizedString("MainViewModel.ErrorParams");
            return false;
        }
        if(!double.TryParse(SearchSide, out double result)) {
            ErrorText = _localizationService.GetLocalizedString("MainViewModel.SearchSideNoDouble");
            return false;
        }
        if(result < 0) {
            ErrorText = _localizationService.GetLocalizedString("MainViewModel.SearchSideNoNegate");
            return false;
        } else if(result == 0) {
            ErrorText = _localizationService.GetLocalizedString("MainViewModel.SearchSideNoZero");
            return false;
        } else if(result > 500) {
            ErrorText = _localizationService.GetLocalizedString("MainViewModel.SearchSideBig");
            return false;
        } else if(result < 10) {
            ErrorText = _localizationService.GetLocalizedString("MainViewModel.SearchSideSmall");
            return false;
        }
        ErrorText = null;
        return true;
    }

    private void LoadConfig() {
        var projectConfig = _pluginConfig.GetSettings(_revitRepository.Document);
        ConfigSettings configSettings;
        if(projectConfig == null) {
            configSettings = new ConfigSettings();
            configSettings.ApplyDefaultValues();
        } else {
            configSettings = projectConfig.ConfigSettings;
        }
        _buildCoordVolumesSettings = new BuildCoordVolumesSettings(_revitRepository, configSettings);
        _buildCoordVolumesSettings.LoadConfigSettings();
    }

    private void SaveConfig() {
        _buildCoordVolumesSettings.Documents = FilteredDocuments.Where(vm => vm.IsChecked).Select(d => d.Document).ToList();
        _buildCoordVolumesSettings.TypeSlabs = FilteredSlabs.Where(vm => vm.IsChecked).Select(vm => vm.Name).ToList();
        _buildCoordVolumesSettings.TypeZone = SelectedTypeZone.Name;
        _buildCoordVolumesSettings.ParamMaps = Params.Where(vm => vm.IsChecked).Select(vm => vm.ParamMap).ToList();
        _buildCoordVolumesSettings.UpPositionProvider = SelectedPositionUp.PositionProvider;
        _buildCoordVolumesSettings.BottomPositionProvider = SelectedPositionBottom.PositionProvider;
        _buildCoordVolumesSettings.SearchSide = Convert.ToDouble(SearchSide);
        _buildCoordVolumesSettings.UpdateConfigSettings();

        var setting = _pluginConfig.GetSettings(_revitRepository.Document)
            ?? _pluginConfig.AddSettings(_revitRepository.Document);
        setting.ConfigSettings = _buildCoordVolumesSettings.ConfigSettings;
        _pluginConfig.SaveProjectConfig();
    }
}
