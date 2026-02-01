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
using RevitBuildCoordVolumes.Models.Services;
using RevitBuildCoordVolumes.Models.Settings;

namespace RevitBuildCoordVolumes.ViewModels;

internal class MainViewModel : BaseViewModel {
    private readonly PluginConfig _pluginConfig;
    private readonly SystemPluginConfig _systemPluginConfig;
    private readonly RevitRepository _revitRepository;
    private readonly BuildCoordVolumeServices _buildCoordVolumeServices;
    private readonly ILocalizationService _localizationService;
    private readonly IRevitParamFactory _revitParamFactory;
    private BuildCoordVolumeSettings _buildCoordVolumeSettings;
    private ObservableCollection<AlgorithmViewModel> _typeAlgorithms;
    private ObservableCollection<BuilderModeViewModel> _builderModes;
    private ObservableCollection<DocumentViewModel> _documents;
    private ObservableCollection<DocumentViewModel> _filteredDocuments;
    private ObservableCollection<TypeZoneViewModel> _typeZones;
    private ObservableCollection<LevelViewModel> _levels;
    private AlgorithmViewModel _selectedTypeAlgorithm;
    private BuilderModeViewModel _selectedBuilderMode;
    private TypeZoneViewModel _selectedTypeZone;
    private string _squareSideMm;
    private string _squareAngleDeg;
    private ObservableCollection<ParamViewModel> _params;
    private ObservableCollection<SlabViewModel> _slabs;
    private ObservableCollection<SlabViewModel> _filteredSlabs;
    private bool _hasParamWarning;
    private bool _requiredCheckArea;
    private bool _isSlabBasedAlgorithm;
    private string _errorText;
    private string _searchTextDocs;
    private string _searchTextSlabs;

    public MainViewModel(
        PluginConfig pluginConfig,
        SystemPluginConfig systemPluginConfig,
        RevitRepository revitRepository,
        BuildCoordVolumeServices buildCoordVolumeServices,
        ILocalizationService localizationService,
        IProgressDialogFactory progressDialogFactory,
        IRevitParamFactory revitParamFactory) {

        _pluginConfig = pluginConfig;
        _systemPluginConfig = systemPluginConfig;
        _revitRepository = revitRepository;
        _buildCoordVolumeServices = buildCoordVolumeServices;
        _localizationService = localizationService;
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
    public ObservableCollection<BuilderModeViewModel> BuilderModes {
        get => _builderModes;
        set => RaiseAndSetIfChanged(ref _builderModes, value);
    }
    public BuilderModeViewModel SelectedBuilderMode {
        get => _selectedBuilderMode;
        set => RaiseAndSetIfChanged(ref _selectedBuilderMode, value);
    }
    public ObservableCollection<TypeZoneViewModel> TypeZones {
        get => _typeZones;
        set => RaiseAndSetIfChanged(ref _typeZones, value);
    }
    public TypeZoneViewModel SelectedTypeZone {
        get => _selectedTypeZone;
        set => RaiseAndSetIfChanged(ref _selectedTypeZone, value);
    }
    public ObservableCollection<LevelViewModel> Levels {
        get => _levels;
        set => RaiseAndSetIfChanged(ref _levels, value);
    }
    public string SquareSideMm {
        get => _squareSideMm;
        set => RaiseAndSetIfChanged(ref _squareSideMm, value);
    }
    public string SquareAngleDeg {
        get => _squareAngleDeg;
        set => RaiseAndSetIfChanged(ref _squareAngleDeg, value);
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
    public bool IsSlabBasedAlgorithm {
        get => _isSlabBasedAlgorithm;
        set => RaiseAndSetIfChanged(ref _isSlabBasedAlgorithm, value);
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

    // Метод обновления UpLevels и BottomLevels
    private void UpdateLevels() {
        Levels = new ObservableCollection<LevelViewModel>(GetLevelViewModels());
    }

    // Метод получения коллекции LevelViewModel для UpLevels и BottomLevels
    public IEnumerable<LevelViewModel> GetLevelViewModels() {
        var typeSlabs = FilteredSlabs.Where(vm => vm.IsChecked).Select(vm => vm.Name);
        var documents = FilteredDocuments.Where(vm => vm.IsChecked).Select(vm => vm.Document);

        if(!typeSlabs.Any() || !documents.Any()) {
            return [];
        }
        var slabs = _revitRepository.GetSlabsByTypesAndDocs(typeSlabs, documents);
        var levels = slabs
            .Select(slab => slab.Level)
            .GroupBy(lvl => lvl.Id)
            .Select(g => g.First())
            .OrderBy(lvl => lvl.Elevation);
        return levels
            .Select(lvl => new LevelViewModel { Level = lvl, Name = lvl.Name, IsChecked = true });
    }

    // Метод обновления TypeZones
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
                .OrderByDescending(vm => vm.Name.Equals(_buildCoordVolumeSettings.TypeZone))
                .ThenBy(vm => vm.Name);
    }

    // Метод обновления FilteredSlabs
    private void UpdateFilteredSlabs() {
        Slabs = new ObservableCollection<SlabViewModel>(GetSlabViewModels());
        FilteredSlabs = new ObservableCollection<SlabViewModel>(Slabs);
        // Подписка на события в SlabViewModel
        foreach(var slabVM in FilteredSlabs) {
            slabVM.PropertyChanged += OnSlabChanged;
        }
    }

    // Метод для реализации поиска в плитах перекрытия
    private void ApplySearchSlabs() {
        FilteredSlabs = string.IsNullOrEmpty(SearchTextSlabs)
            ? new ObservableCollection<SlabViewModel>(Slabs)
            : new ObservableCollection<SlabViewModel>(Slabs
                .Where(item => item.Name
                .IndexOf(SearchTextSlabs, StringComparison.OrdinalIgnoreCase) >= 0));
    }

    // Метод подписанный на событие изменения SlabViewModel
    private void OnSlabChanged(object sender, PropertyChangedEventArgs e) {
        if(sender is not SlabViewModel vm) {
            return;
        }
        if(e.PropertyName == nameof(vm.IsChecked)) {
            UpdateLevels();
        }
    }

    // Метод получения коллекции SlabViewModel для Slabs
    private IEnumerable<SlabViewModel> GetSlabViewModels() {
        var documents = FilteredDocuments.Where(vm => vm.IsChecked).Select(vm => vm.Document);
        var allSlabs = _revitRepository.GetTypeSlabsByDocs(documents);
        var savedSlabs = _buildCoordVolumeSettings.TypeSlabs;
        return !allSlabs.Any()
            ? []
            : allSlabs
                .Select(slabType => new SlabViewModel {
                    Name = slabType,
                    IsChecked = savedSlabs.Contains(slabType),
                })
                .OrderByDescending(vm => _systemPluginConfig.DefaultSlabTypeNames.Any(part => vm.Name.Contains(part)))
                .ThenBy(vm => vm.Name);
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
            UpdateLevels();
        }
    }

    // Метод получения коллекции DocumentViewModel для Documents
    private IEnumerable<DocumentViewModel> GetDocumentViewModels() {
        var allDocuments = _revitRepository.GetAllDocuments();
        var savedDocuments = _buildCoordVolumeSettings.Documents;
        var savedDocumentsNames = savedDocuments.Count == 0
            ? []
            : savedDocuments
                .Select(doc => doc.GetUniqId());

        return !allDocuments.Any()
            ? []
            : allDocuments
                .Select(document => new DocumentViewModel(_localizationService, document) {
                    IsChecked = savedDocumentsNames.Contains(document.GetUniqId())
                })
                .OrderBy(vm => vm.Name);
    }

    // Метод обновления Params
    private void UpdateParams() {
        Params = new ObservableCollection<ParamViewModel>(GetParamViewModels());
        // Подписка на события в ParamViewModel
        foreach(var param in Params) {
            param.PropertyChanged += OnParamChanged;
        }
        UpdateParamWarnings();
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
                    var def = _buildCoordVolumeServices.ParamAvailabilityService.GetDefinitionByName(_revitRepository.Document, vm.SourceParamName);
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
                    var def = _buildCoordVolumeServices.ParamAvailabilityService.GetDefinitionByName(_revitRepository.Document, vm.TargetParamName);
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
        var defaultParamMaps = SelectedTypeAlgorithm.AlgorithmType == AlgorithmType.SlabBasedAlgorithm
            ? _systemPluginConfig.GetAdvancedParamMaps()
            : _systemPluginConfig.GetSimpleParamMaps();
        var savedParamMaps = _buildCoordVolumeSettings.ParamMaps;
        var savedLookup = savedParamMaps.ToDictionary(paramMap => paramMap.Type, paramMap => paramMap);

        return defaultParamMaps.Select(defaultParamMap => {
            bool exists = savedLookup.TryGetValue(defaultParamMap.Type, out var savedParamMap);
            var paramMap = exists ? savedParamMap : defaultParamMap;
            bool hasSourceParam = paramMap.SourceParam != null;
            bool hasTargetParam = paramMap.TargetParam != null;

            return new ParamViewModel(_localizationService, _buildCoordVolumeServices) {
                ParamMap = paramMap,
                Description = _localizationService.GetLocalizedString($"MainViewModel.{paramMap.Type}Description"),
                DetailDescription = _localizationService.GetLocalizedString($"MainViewModel.{paramMap.Type}DetailDescription"),
                SourceParamName = paramMap.SourceParam?.Name ?? string.Empty,
                TargetParamName = paramMap.TargetParam?.Name ?? string.Empty,
                IsChecked = exists,
                HasSourceParam = hasSourceParam,
                HasTargetParam = hasTargetParam,
            };
        });
    }

    // Метод обновляющий состояние свойства для скрытия/отображения дополнительных настроек основного окна
    private void UpdateVisibilitySettings() {
        IsSlabBasedAlgorithm = SelectedTypeAlgorithm.AlgorithmType == AlgorithmType.SlabBasedAlgorithm;
    }

    // Метод подписанный на события MainViewModel
    private void OnPropertyChanged(object sender, PropertyChangedEventArgs e) {
        if(e.PropertyName == nameof(SelectedTypeAlgorithm)) {
            UpdateVisibilitySettings();
            UpdateParams();
        }
        if(e.PropertyName == nameof(SelectedTypeZone)) {
            UpdateRequiredCheckArea();
        }
    }

    // Метод получения коллекции BuilderModeViewModel для BuilderModes
    private IEnumerable<BuilderModeViewModel> GetTypeBuilderModeViewModels() {
        var currentBuilderMode = _buildCoordVolumeSettings.BuilderMode;
        var builderModes = Enum.GetValues(typeof(BuilderMode)).Cast<BuilderMode>();
        return builderModes
            .Select(builderMode => new BuilderModeViewModel {
                Name = _localizationService.GetLocalizedString($"MainViewModel.{builderMode}"),
                BuilderMode = builderMode
            })
            .OrderByDescending(vm => vm.BuilderMode == currentBuilderMode);
    }

    // Метод получения коллекции AlgorithmViewModel для TypeAlgorithms
    private IEnumerable<AlgorithmViewModel> GetTypeAlgorithmsViewModels() {
        var currentAlgorithmType = _buildCoordVolumeSettings.AlgorithmType;
        var algorithmTypes = Enum.GetValues(typeof(AlgorithmType)).Cast<AlgorithmType>();
        return algorithmTypes
            .Select(algorithmType => new AlgorithmViewModel {
                Name = _localizationService.GetLocalizedString($"MainViewModel.{algorithmType}"),
                AlgorithmType = algorithmType
            })
            .OrderByDescending(vm => vm.AlgorithmType == currentAlgorithmType)
            .ThenBy(vm => vm.Name);
    }

    // Метод обновления свойства необходимости проверки зоны
    private void UpdateRequiredCheckArea() {
        RequiredCheckArea = true;
    }

    // Метод загрузки окна
    private void LoadView() {
        LoadConfig();
        TypeAlgorithms = new ObservableCollection<AlgorithmViewModel>(GetTypeAlgorithmsViewModels());
        SelectedTypeAlgorithm = TypeAlgorithms.FirstOrDefault();

        UpdateVisibilitySettings();

        BuilderModes = new ObservableCollection<BuilderModeViewModel>(GetTypeBuilderModeViewModels());
        SelectedBuilderMode = BuilderModes.FirstOrDefault();

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
        foreach(var documentVM in FilteredDocuments) {
            documentVM.PropertyChanged += OnDocumentChanged;
        }

        TypeZones = new ObservableCollection<TypeZoneViewModel>(GetTypeZoneViewModels());
        SelectedTypeZone = TypeZones.FirstOrDefault();

        Slabs = new ObservableCollection<SlabViewModel>(GetSlabViewModels());
        FilteredSlabs = new ObservableCollection<SlabViewModel>(Slabs);
        // Подписка на события в SlabViewModel
        foreach(var slabVM in FilteredSlabs) {
            slabVM.PropertyChanged += OnSlabChanged;
        }

        Levels = new ObservableCollection<LevelViewModel>(GetLevelViewModels());

        SquareSideMm = Convert.ToString(_buildCoordVolumeSettings.SquareSideMm);
        SquareAngleDeg = Convert.ToString(_buildCoordVolumeSettings.SquareAngleDeg);

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

    //Основной метод
    private void AcceptView() {
        SaveConfig();

        // Экземпляр процессора
        var processor = new BuildCoordVolumesProcessor(
            _localizationService,
            _revitRepository,
            _buildCoordVolumeSettings,
            _buildCoordVolumeServices);

        int count = processor.SpatialObjects.Count;
        using var progressDialogService = ProgressDialogFactory.CreateDialog();
        var progress = progressDialogService.CreateProgress();
        var ct = progressDialogService.CreateCancellationToken();

        progressDialogService.MaxValue = count;
        progressDialogService.StepValue = count / 10;
        progressDialogService.DisplayTitleFormat = _localizationService.GetLocalizedString("MainViewModel.ProgressTitle");

        progressDialogService.Show();

        // Основной метод построения объемов
        processor.Run(progress, ct);
    }

    // Метод проверки возможности выполнения основного метода
    private bool CanAcceptView() {
        if(Params != null) {
            var descriptionParamVM = Params.Where(pvm => pvm.ParamMap.Type == ParamType.DescriptionParam).FirstOrDefault();
            if(descriptionParamVM != null && !descriptionParamVM.IsChecked) {
                ErrorText = _localizationService.GetLocalizedString("MainViewModel.NoMainParam");
                return false;
            }
            var topZoneParamVM = Params.Where(pvm => pvm.ParamMap.Type == ParamType.TopZoneParam).FirstOrDefault();
            if(topZoneParamVM != null && !topZoneParamVM.IsChecked && !IsSlabBasedAlgorithm) {
                ErrorText = _localizationService.GetLocalizedString("MainViewModel.NoTopZoneParam");
                return false;
            }
            var bottomZoneParamVM = Params.Where(pvm => pvm.ParamMap.Type == ParamType.BottomZoneParam).FirstOrDefault();
            if(bottomZoneParamVM != null && !bottomZoneParamVM.IsChecked && !IsSlabBasedAlgorithm) {
                ErrorText = _localizationService.GetLocalizedString("MainViewModel.NoBottomZoneParam");
                return false;
            }
            var volumeParamVM = Params.Where(pvm => pvm.ParamMap.Type == ParamType.VolumeParam).FirstOrDefault();
            if(volumeParamVM != null && !volumeParamVM.IsChecked) {
                ErrorText = _localizationService.GetLocalizedString("MainViewModel.NoVolumeParam");
                return false;
            }
            var checkedParams = Params.Where(p => p.IsChecked).ToList();
            if(checkedParams.Count == 0) {
                ErrorText = _localizationService.GetLocalizedString("MainViewModel.NoParams");
                return false;
            }
        }
        if(HasParamWarning) {
            ErrorText = _localizationService.GetLocalizedString("MainViewModel.ErrorParams");
            return false;
        }
        if(SelectedTypeZone == null) {
            ErrorText = _localizationService.GetLocalizedString("MainViewModel.NoTypeZone");
            return false;
        }
        if(FilteredDocuments != null && IsSlabBasedAlgorithm) {
            var checkedDocs = FilteredDocuments.Where(p => p.IsChecked).ToList();
            if(checkedDocs.Count == 0) {
                ErrorText = _localizationService.GetLocalizedString("MainViewModel.NoDocuments");
                return false;
            }
        }
        if(FilteredSlabs != null && IsSlabBasedAlgorithm) {
            var checkedSlabs = FilteredSlabs.Where(p => p.IsChecked).ToList();
            if(checkedSlabs.Count == 0) {
                ErrorText = _localizationService.GetLocalizedString("MainViewModel.NoSlabs");
                return false;
            }
        }
        if(!double.TryParse(SquareAngleDeg, out double resultAngleDeg) && IsSlabBasedAlgorithm) {
            ErrorText = _localizationService.GetLocalizedString("MainViewModel.SquareValueNoDouble");
            return false;
        }
        if(!double.TryParse(SquareSideMm, out double resultSideMm) && IsSlabBasedAlgorithm) {
            ErrorText = _localizationService.GetLocalizedString("MainViewModel.SquareValueNoDouble");
            return false;
        }
        if(resultSideMm < 0 && IsSlabBasedAlgorithm) {
            ErrorText = _localizationService.GetLocalizedString("MainViewModel.SquareSideNoNegate");
            return false;
        } else if(resultSideMm == 0 && IsSlabBasedAlgorithm) {
            ErrorText = _localizationService.GetLocalizedString("MainViewModel.SquareSideNoZero");
            return false;
        } else if(resultSideMm > 500 && IsSlabBasedAlgorithm) {
            ErrorText = _localizationService.GetLocalizedString("MainViewModel.SquareSideBig");
            return false;
        } else if(resultSideMm < 10 && IsSlabBasedAlgorithm) {
            ErrorText = _localizationService.GetLocalizedString("MainViewModel.SquareSideSmall");
            return false;
        }
        if(RequiredCheckArea) {
            ErrorText = _localizationService.GetLocalizedString("MainViewModel.RequiredCheckArea");
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
            configSettings.ApplyDefaultValues(_systemPluginConfig);
        } else {
            configSettings = projectConfig.ConfigSettings;
        }
        var documents = configSettings.Documents.Count == 0
            ? _revitRepository.GetAllDocuments().ToList()
            : configSettings.Documents
                .Select(_revitRepository.FindDocumentsByName)
                .Where(doc => doc != null)
                .ToList();

        var typeSlabs = configSettings.TypeSlabs.Count == 0
            ? _revitRepository.GetTypeSlabsByDocs(documents)
                .Where(name => _systemPluginConfig.DefaultSlabTypeNames.Any(type => name.Contains(type)))
                .ToList()
            : configSettings.TypeSlabs;

        _buildCoordVolumeSettings = new BuildCoordVolumeSettings {
            AlgorithmType = configSettings.AlgorithmType,
            BuilderMode = configSettings.BuilderMode,
            TypeZone = configSettings.TypeZone,
            ParamMaps = configSettings.ParamMaps,
            Documents = documents,
            TypeSlabs = typeSlabs,
            SquareSideMm = configSettings.SquareSideMm,
            SquareAngleDeg = configSettings.SquareAngleDeg
        };
    }

    // Метод сохранения конфигурации пользователя и основных настроек программы
    private void SaveConfig() {
        var algorithmType = SelectedTypeAlgorithm.AlgorithmType;
        string typeZone = SelectedTypeZone.Name;
        var paramMaps = Params.Where(vm => vm.IsChecked).Select(vm => vm.ParamMap).ToList();
        var builderMode = SelectedBuilderMode.BuilderMode;
        var documents = FilteredDocuments.Where(vm => vm.IsChecked).Select(d => d.Document).ToList();
        var typeSlabs = FilteredSlabs.Where(vm => vm.IsChecked).Select(vm => vm.Name).ToList();
        var levels = Levels.Where(vm => vm.IsChecked).Select(vm => vm.Level).ToList();
        double squareSide = Convert.ToDouble(SquareSideMm);
        double squareAngle = Convert.ToDouble(SquareAngleDeg);

        _buildCoordVolumeSettings.AlgorithmType = algorithmType;
        _buildCoordVolumeSettings.BuilderMode = builderMode;
        _buildCoordVolumeSettings.Documents = documents;
        _buildCoordVolumeSettings.TypeSlabs = typeSlabs;
        _buildCoordVolumeSettings.TypeZone = typeZone;
        _buildCoordVolumeSettings.ParamMaps = paramMaps;
        _buildCoordVolumeSettings.Levels = levels;
        _buildCoordVolumeSettings.SquareSideMm = squareSide;
        _buildCoordVolumeSettings.SquareAngleDeg = squareAngle;

        var configSettings = new ConfigSettings {
            AlgorithmType = algorithmType,
            BuilderMode = builderMode,
            Documents = documents.Select(doc => doc.GetUniqId()).ToList(),
            TypeZone = typeZone,
            ParamMaps = paramMaps,
            TypeSlabs = typeSlabs,
            SquareSideMm = squareSide,
            SquareAngleDeg = squareAngle
        };

        var setting = _pluginConfig.GetSettings(_revitRepository.Document)
            ?? _pluginConfig.AddSettings(_revitRepository.Document);
        setting.ConfigSettings = configSettings;
        _pluginConfig.SaveProjectConfig();
    }
}
