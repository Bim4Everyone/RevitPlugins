using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;

using dosymep.Revit;
using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitBuildCoordVolumes.Models;
using RevitBuildCoordVolumes.Models.Enums;
using RevitBuildCoordVolumes.Models.Services;
using RevitBuildCoordVolumes.Models.Settings;

namespace RevitBuildCoordVolumes.ViewModels;

internal class SlabBasedSettingViewModel : BaseViewModel {
    private readonly SystemPluginConfig _systemPluginConfig;
    private readonly RevitRepository _revitRepository;
    private readonly BuildCoordVolumeSettings _settings;
    private readonly BuildCoordVolumeServices _services;
    private readonly ILocalizationService _localizationService;
    private ObservableCollection<BuilderModeViewModel> _builderModes;
    private BuilderModeViewModel _selectedBuilderMode;
    private ObservableCollection<DocumentViewModel> _documents;
    private ObservableCollection<DocumentViewModel> _filteredDocuments;
    private string _searchTextDocs;
    private ObservableCollection<SlabViewModel> _slabs;
    private ObservableCollection<SlabViewModel> _filteredSlabs;
    private string _searchTextSlabs;
    private ObservableCollection<LevelViewModel> _levels;
    private ObservableCollection<LevelViewModel> _filteredLevels;
    private string _searchTextLevels;
    private string _squareSideMm;
    private string _squareAngleDeg;

    public SlabBasedSettingViewModel(
        SystemPluginConfig systemPluginConfig,
        RevitRepository revitRepository,
        BuildCoordVolumeSettings buildCoordVolumeSettings,
        BuildCoordVolumeServices buildCoordVolumeServices) {
        _systemPluginConfig = systemPluginConfig;
        _revitRepository = revitRepository;
        _settings = buildCoordVolumeSettings;
        _services = buildCoordVolumeServices;
        _localizationService = _services.LocalizationService;

        LoadView();

        SearchDocsCommand = RelayCommand.Create(ApplySearchDocs);
        SearchSlabsCommand = RelayCommand.Create(ApplySearchSlabs);
        SearchLevelsCommand = RelayCommand.Create(ApplySearchLevels);
        CheckAllLevelsCommand = RelayCommand.Create(CheckAllLevels);
        UncheckAllLevelsCommand = RelayCommand.Create(UncheckAllLevels);
    }
    public ICommand SearchDocsCommand { get; }
    public ICommand SearchSlabsCommand { get; }
    public ICommand SearchLevelsCommand { get; }
    public ICommand CheckAllLevelsCommand { get; }
    public ICommand UncheckAllLevelsCommand { get; }

    public ObservableCollection<BuilderModeViewModel> BuilderModes {
        get => _builderModes;
        set => RaiseAndSetIfChanged(ref _builderModes, value);
    }
    public BuilderModeViewModel SelectedBuilderMode {
        get => _selectedBuilderMode;
        set => RaiseAndSetIfChanged(ref _selectedBuilderMode, value);
    }
    public ObservableCollection<DocumentViewModel> Documents {
        get => _documents;
        set => RaiseAndSetIfChanged(ref _documents, value);
    }
    public ObservableCollection<DocumentViewModel> FilteredDocuments {
        get => _filteredDocuments;
        set => RaiseAndSetIfChanged(ref _filteredDocuments, value);
    }
    public string SearchTextDocs {
        get => _searchTextDocs;
        set => RaiseAndSetIfChanged(ref _searchTextDocs, value);
    }
    public ObservableCollection<SlabViewModel> Slabs {
        get => _slabs;
        set => RaiseAndSetIfChanged(ref _slabs, value);
    }
    public ObservableCollection<SlabViewModel> FilteredSlabs {
        get => _filteredSlabs;
        set => RaiseAndSetIfChanged(ref _filteredSlabs, value);
    }
    public string SearchTextSlabs {
        get => _searchTextSlabs;
        set => RaiseAndSetIfChanged(ref _searchTextSlabs, value);
    }
    public ObservableCollection<LevelViewModel> Levels {
        get => _levels;
        set => RaiseAndSetIfChanged(ref _levels, value);
    }
    public ObservableCollection<LevelViewModel> FilteredLevels {
        get => _filteredLevels;
        set => RaiseAndSetIfChanged(ref _filteredLevels, value);
    }
    public string SearchTextLevels {
        get => _searchTextLevels;
        set => RaiseAndSetIfChanged(ref _searchTextLevels, value);
    }
    public string SquareSideMm {
        get => _squareSideMm;
        set => RaiseAndSetIfChanged(ref _squareSideMm, value);
    }
    public string SquareAngleDeg {
        get => _squareAngleDeg;
        set => RaiseAndSetIfChanged(ref _squareAngleDeg, value);
    }

    // Метод команды на выделение всех уровней
    private void CheckAllLevels() {
        foreach(var vm in FilteredLevels) {
            vm.IsChecked = true;
        }
    }

    // Метод команды на снятие выделения всех уровней
    private void UncheckAllLevels() {
        foreach(var vm in FilteredLevels) {
            vm.IsChecked = false;
        }
    }

    // Метод обновления UpLevels и BottomLevels
    private void UpdateLevels() {
        Levels = new ObservableCollection<LevelViewModel>(GetLevelViewModels());
        FilteredLevels = new ObservableCollection<LevelViewModel>(Levels);
    }

    // Метод для реализации поиска уровнях
    private void ApplySearchLevels() {
        FilteredLevels = string.IsNullOrEmpty(SearchTextLevels)
            ? new ObservableCollection<LevelViewModel>(Levels)
            : new ObservableCollection<LevelViewModel>(Levels
                .Where(item => item.Name
                .IndexOf(SearchTextLevels, StringComparison.OrdinalIgnoreCase) >= 0));
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
        var allSlabs = _revitRepository.GetTypeSlabsByDocs(documents)?.ToList() ?? [];
        if(!allSlabs.Any()) {
            return [];
        }
        var savedSlabs = _settings.TypeSlabs?.ToHashSet() ?? [];
        var defaultNames = _systemPluginConfig.DefaultSlabTypeNames ?? [];

        bool hasSaved = savedSlabs.Any();

        return allSlabs
            .Select(slabType => new SlabViewModel {
                Name = slabType,
                IsChecked = hasSaved
                    ? savedSlabs.Contains(slabType)
                    : defaultNames.Any(def => slabType.Contains(def))
            })
            .OrderByDescending(vm => defaultNames.Any(def => vm.Name.Contains(def)))
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
        var savedDocuments = _settings.Documents;
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

    // Метод получения коллекции BuilderModeViewModel для BuilderModes
    private IEnumerable<BuilderModeViewModel> GetTypeBuilderModeViewModels() {
        var currentBuilderMode = _settings.BuilderMode;
        var builderModes = Enum.GetValues(typeof(BuilderMode)).Cast<BuilderMode>();
        return builderModes
            .Select(builderMode => new BuilderModeViewModel {
                Name = _localizationService.GetLocalizedString($"SlabBasedSettingViewModel.{builderMode}"),
                BuilderMode = builderMode
            })
            .OrderByDescending(vm => vm.BuilderMode == currentBuilderMode);
    }

    // Метод загрузки окна
    private void LoadView() {
        BuilderModes = new ObservableCollection<BuilderModeViewModel>(GetTypeBuilderModeViewModels());
        SelectedBuilderMode = BuilderModes.FirstOrDefault();

        Documents = new ObservableCollection<DocumentViewModel>(GetDocumentViewModels());
        FilteredDocuments = new ObservableCollection<DocumentViewModel>(Documents);
        // Подписка на события в DocumentViewModel
        foreach(var documentVM in FilteredDocuments) {
            documentVM.PropertyChanged += OnDocumentChanged;
        }

        Slabs = new ObservableCollection<SlabViewModel>(GetSlabViewModels());
        FilteredSlabs = new ObservableCollection<SlabViewModel>(Slabs);
        // Подписка на события в SlabViewModel
        foreach(var slabVM in FilteredSlabs) {
            slabVM.PropertyChanged += OnSlabChanged;
        }

        Levels = new ObservableCollection<LevelViewModel>(GetLevelViewModels());
        FilteredLevels = new ObservableCollection<LevelViewModel>(Levels);

        SquareSideMm = Convert.ToString(_settings.SquareSideMm);
        SquareAngleDeg = Convert.ToString(_settings.SquareAngleDeg);
    }
}
