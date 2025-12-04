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
    private BuildCoordVolumesSettings _buildCoordVolumesSettings;
    private ObservableCollection<AlgorithmViewModel> _typeAlgorithms;
    private ObservableCollection<DocumentViewModel> _documents;
    private ObservableCollection<DocumentViewModel> _filteredDocuments;
    private ObservableCollection<TypeZoneViewModel> _typeZones;
    private AlgorithmViewModel _selectedTypeAlgorithm;
    private TypeZoneViewModel _selectedTypeZone;
    private string _searchSide;
    private ObservableCollection<ParamViewModel> _params;
    private ObservableCollection<SlabViewModel> _slabs;
    private ObservableCollection<SlabViewModel> _filteredSlabs;
    private bool _hasParamWarning;
    private bool _requiredCheckArea;
    private bool _isAdvancedAlgorithm;
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

        LoadViewCommand = RelayCommand.Create(LoadView);
        CheckAreaCommand = RelayCommand.Create(CheckArea, CanCheckArea);
        SearchDocsCommand = RelayCommand.Create(ApplySearchDocs);
        SearchSlabsCommand = RelayCommand.Create(ApplySearchSlabs);
        AcceptViewCommand = RelayCommand.Create(AcceptView, CanAcceptView);

        ProgressDialogFactory = progressDialogFactory
            ?? throw new System.ArgumentNullException(nameof(progressDialogFactory));
    }

    public ICommand LoadViewCommand { get; }
    public ICommand CheckAreaCommand { get; }
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
    public ObservableCollection<AlgorithmViewModel> TypeAlgorithms {
        get => _typeAlgorithms;
        set => RaiseAndSetIfChanged(ref _typeAlgorithms, value);
    }
    public AlgorithmViewModel SelectedTypeAlgorithm {
        get => _selectedTypeAlgorithm;
        set => RaiseAndSetIfChanged(ref _selectedTypeAlgorithm, value);
    }
    public ObservableCollection<TypeZoneViewModel> TypeZones {
        get => _typeZones;
        set => RaiseAndSetIfChanged(ref _typeZones, value);
    }
    public TypeZoneViewModel SelectedTypeZone {
        get => _selectedTypeZone;
        set => RaiseAndSetIfChanged(ref _selectedTypeZone, value);
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
    public bool RequiredCheckArea {
        get => _requiredCheckArea;
        set => RaiseAndSetIfChanged(ref _requiredCheckArea, value);
    }
    public bool IsAdvancedAlgorithm {
        get => _isAdvancedAlgorithm;
        set => RaiseAndSetIfChanged(ref _isAdvancedAlgorithm, value);
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

    // Метод обновления параметра в TypeZones
    private void UpdateTypeZones() {
        TypeZones = new ObservableCollection<TypeZoneViewModel>(GetTypeZoneViewModels());
        SelectedTypeZone = TypeZones.FirstOrDefault();
        UpdateRequiredCheckArea();
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

    // Метод обновления параметра в FilteredSlabs
    private void UpdateFilteredSlabs() {
        Slabs = new ObservableCollection<SlabViewModel>(GetSlabViewModels());
        FilteredSlabs = new ObservableCollection<SlabViewModel>(Slabs);
    }

    // Метод для реализации поиска в плитах перекрытия
    private void ApplySearchSlabs() {
        FilteredSlabs = string.IsNullOrEmpty(SearchTextSlabs)
            ? new ObservableCollection<SlabViewModel>(Slabs)
            : new ObservableCollection<SlabViewModel>(Slabs
                .Where(item => item.Name
                .IndexOf(SearchTextSlabs, StringComparison.OrdinalIgnoreCase) >= 0));
    }

    // Метод получения коллекции SlabViewModel для Slabs
    private IEnumerable<SlabViewModel> GetSlabViewModels() {
        var documents = FilteredDocuments.Where(vm => vm.IsChecked).Select(vm => vm.Document);
        var allSlabs = _revitRepository.GetTypeSlabsByDocs(documents);
        var savedSlabs = _buildCoordVolumesSettings.TypeSlabs;
        return !allSlabs.Any()
            ? []
            : allSlabs
                .Select(slabType => new SlabViewModel {
                    Name = slabType,
                    IsChecked = savedSlabs.Contains(slabType),
                })
                .OrderBy(vm => vm.Name);
    }

    // Метод для реализации поиска в документах
    private void ApplySearchDocs() {
        FilteredDocuments = string.IsNullOrEmpty(SearchTextDocs)
            ? new ObservableCollection<DocumentViewModel>(Documents)
            : new ObservableCollection<DocumentViewModel>(Documents
                .Where(item => item.Name
                .IndexOf(SearchTextDocs, StringComparison.OrdinalIgnoreCase) >= 0));
    }

    // Метод подписанный на событие изменения DocumentViewModel
    private void OnDocumentChanged(object sender, PropertyChangedEventArgs e) {
        if(sender is not DocumentViewModel vm) {
            return;
        }
        if(e.PropertyName == nameof(vm.IsChecked)) {
            UpdateFilteredSlabs();
        }
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

    // Метод обновления предупреждений в параметрах
    private void UpdateParamWarnings() {
        foreach(var param in Params) {
            param.UpdateWarning(_revitRepository.Document);
        }
        HasParamWarning = Params.Any(param => param.HasWarning);
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

    // Метод обновляющий состояние свойства для скрытия/отображения дополнительных настроек основного окна
    private void UpdateVisibilitySettings() {
        IsAdvancedAlgorithm = SelectedTypeAlgorithm.AlgorithmType == AlgorithmType.AdvancedAreaExtrude;
    }

    // Метод подписанный на события
    private void OnPropertyChanged(object sender, PropertyChangedEventArgs e) {
        if(e.PropertyName == nameof(SelectedTypeAlgorithm)) {
            UpdateVisibilitySettings();
        }
        if(e.PropertyName == nameof(SelectedTypeZone)) {
            UpdateRequiredCheckArea();
        }

    }

    // Метод получения коллекции AlgorithmViewModel для TypeAlgorithms
    private IEnumerable<AlgorithmViewModel> GetTypeAlgorithmsViewModels() {
        var currentAlgorithmType = _buildCoordVolumesSettings.AlgorithmType;
        var algorithmTypes = Enum.GetValues(typeof(AlgorithmType)).Cast<AlgorithmType>();
        return algorithmTypes
            .Select(algorithmType => new AlgorithmViewModel {
                Name = _localizationService.GetLocalizedString($"MainViewModel.{algorithmType}"),
                AlgorithmType = algorithmType
            })
            .OrderByDescending(vm => vm.AlgorithmType == currentAlgorithmType)
            .ThenBy(vm => vm.Name);
    }

    private void UpdateRequiredCheckArea() {
        RequiredCheckArea = true;
    }

    // Метод загрузки окна
    private void LoadView() {
        LoadConfig();
        TypeAlgorithms = new ObservableCollection<AlgorithmViewModel>(GetTypeAlgorithmsViewModels());
        SelectedTypeAlgorithm = TypeAlgorithms.FirstOrDefault();

        UpdateVisibilitySettings();

        // Подписка на события для обновления IsAdvancedAlgorithm
        PropertyChanged += OnPropertyChanged;

        Params = new ObservableCollection<ParamViewModel>(GetParamViewModels());
        // Подписка на события в ParamViewModel
        foreach(var param in Params) {
            param.PropertyChanged += OnParamChanged;
        }
        UpdateParamWarnings();

        Documents = new ObservableCollection<DocumentViewModel>(GetDocumentViewModels());
        FilteredDocuments = new ObservableCollection<DocumentViewModel>(Documents);
        // Подписка на события в DocumentViewModel
        foreach(var document in FilteredDocuments) {
            document.PropertyChanged += OnDocumentChanged;
        }

        TypeZones = new ObservableCollection<TypeZoneViewModel>(GetTypeZoneViewModels());
        SelectedTypeZone = TypeZones.FirstOrDefault();

        Slabs = new ObservableCollection<SlabViewModel>(GetSlabViewModels());
        FilteredSlabs = new ObservableCollection<SlabViewModel>(Slabs);

        SearchSide = Convert.ToString(_buildCoordVolumesSettings.SearchSide);

        UpdateRequiredCheckArea();
    }

    // Метод проверки зон
    private void CheckArea() {
        // реализация проверки зон
        RequiredCheckArea = false;
    }

    // Метод проверки возможности выполнения метода проверки зон
    private bool CanCheckArea() {
        return SelectedTypeZone != null;
    }

    // Основной метод
    private void AcceptView() {
        SaveConfig();

        var processor = new BuildCoordVolumesProcessor(_localizationService, _revitRepository, _buildCoordVolumesSettings);

        using var progressDialogService = ProgressDialogFactory.CreateDialog();
        progressDialogService.MaxValue = processor.Areas.Count();
        progressDialogService.StepValue = 1;
        progressDialogService.DisplayTitleFormat = _localizationService.GetLocalizedString("MainViewModel.ProgressTitle");
        var progress = progressDialogService.CreateProgress();

        var ct = progressDialogService.CreateCancellationToken();
        progressDialogService.Show();

        processor.Run(progress, ct);
    }

    // Метод проверки возможности выполнения основного метода
    private bool CanAcceptView() {
        if(RequiredCheckArea) {
            ErrorText = _localizationService.GetLocalizedString("MainViewModel.RequiredCheckArea");
            return false;
        }
        if(FilteredDocuments != null) {
            var checkedDocs = FilteredDocuments.Where(p => p.IsChecked).ToList();
            if(checkedDocs.Count == 0) {
                ErrorText = _localizationService.GetLocalizedString("MainViewModel.NoDocuments");
                return false;
            }
        }
        if(Params != null) {
            var firstParamMap = Params.FirstOrDefault();
            if(firstParamMap != null && !firstParamMap.IsChecked && firstParamMap.ParamMap.Type == ParamType.DescriptionParam) {
                ErrorText = _localizationService.GetLocalizedString("MainViewModel.NoMainParam");
                return false;
            }
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

    // Метод загрузки конфигурации пользователя
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

    // Метод сохранения конфигурации пользователя
    private void SaveConfig() {
        _buildCoordVolumesSettings.AlgorithmType = SelectedTypeAlgorithm.AlgorithmType;
        _buildCoordVolumesSettings.Documents = FilteredDocuments.Where(vm => vm.IsChecked).Select(d => d.Document).ToList();
        _buildCoordVolumesSettings.TypeSlabs = FilteredSlabs.Where(vm => vm.IsChecked).Select(vm => vm.Name).ToList();
        _buildCoordVolumesSettings.TypeZone = SelectedTypeZone.Name;
        _buildCoordVolumesSettings.ParamMaps = Params.Where(vm => vm.IsChecked).Select(vm => vm.ParamMap).ToList();
        _buildCoordVolumesSettings.SearchSide = Convert.ToDouble(SearchSide);
        _buildCoordVolumesSettings.UpdateConfigSettings();

        var setting = _pluginConfig.GetSettings(_revitRepository.Document)
            ?? _pluginConfig.AddSettings(_revitRepository.Document);
        setting.ConfigSettings = _buildCoordVolumesSettings.ConfigSettings;
        _pluginConfig.SaveProjectConfig();
    }
}
