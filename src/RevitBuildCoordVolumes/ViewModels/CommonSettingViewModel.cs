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

internal class CommonSettingViewModel : BaseViewModel {
    private readonly SystemPluginConfig _systemPluginConfig;
    private readonly RevitRepository _revitRepository;
    private readonly BuildCoordVolumeSettings _settings;
    private readonly BuildCoordVolumeServices _services;
    private readonly IRevitParamFactory _revitParamFactory;
    private readonly ILocalizationService _localizationService;

    private ObservableCollection<AlgorithmViewModel> _typeAlgorithms;
    private AlgorithmViewModel _selectedTypeAlgorithm;
    private ObservableCollection<TypeZoneViewModel> _typeZones;
    private ObservableCollection<TypeZoneViewModel> _filteredTypeZones;
    private ObservableCollection<TypeZoneViewModel> _selectedTypeZones;
    private string _searchTextTypeZones;
    private ObservableCollection<ParamViewModel> _params;
    private bool _hasParamWarning;

    public CommonSettingViewModel(
        SystemPluginConfig systemPluginConfig,
        RevitRepository revitRepository,
        BuildCoordVolumeSettings buildCoordVolumeSettings,
        BuildCoordVolumeServices buildCoordVolumeServices) {

        _systemPluginConfig = systemPluginConfig;
        _revitRepository = revitRepository;
        _settings = buildCoordVolumeSettings;
        _services = buildCoordVolumeServices;
        _localizationService = _services.LocalizationService;
        _revitParamFactory = _services.RevitParamFactory;

        LoadView();

        SearchTypeZonesCommand = RelayCommand.Create(ApplySearchTypeZones);
        CheckAllTypeZonesCommand = RelayCommand.Create(CheckAllTypeZones);
        UncheckAllTypeZonesCommand = RelayCommand.Create(UncheckAllTypeZones);
    }

    public ICommand SearchTypeZonesCommand { get; }
    public ICommand CheckAllTypeZonesCommand { get; }
    public ICommand UncheckAllTypeZonesCommand { get; }

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
    public ObservableCollection<TypeZoneViewModel> FilteredTypeZones {
        get => _filteredTypeZones;
        set => RaiseAndSetIfChanged(ref _filteredTypeZones, value);
    }
    public ObservableCollection<TypeZoneViewModel> SelectedTypeZones {
        get => _selectedTypeZones;
        set => RaiseAndSetIfChanged(ref _selectedTypeZones, value);
    }
    public string SearchTextTypeZones {
        get => _searchTextTypeZones;
        set => RaiseAndSetIfChanged(ref _searchTextTypeZones, value);
    }
    public ObservableCollection<ParamViewModel> Params {
        get => _params;
        set => RaiseAndSetIfChanged(ref _params, value);
    }
    public bool HasParamWarning {
        get => _hasParamWarning;
        set => RaiseAndSetIfChanged(ref _hasParamWarning, value);
    }

    // Метод для реализации поиска типов зон
    private void ApplySearchTypeZones() {
        FilteredTypeZones = string.IsNullOrEmpty(SearchTextTypeZones)
            ? new ObservableCollection<TypeZoneViewModel>(TypeZones)
            : new ObservableCollection<TypeZoneViewModel>(TypeZones
                .Where(item => item.Name
                .IndexOf(SearchTextTypeZones, StringComparison.OrdinalIgnoreCase) >= 0));
    }

    // Метод команды на выделение всех типов зон
    private void CheckAllTypeZones() {
        foreach(var catVM in FilteredTypeZones) {
            catVM.IsChecked = true;
        }
    }

    // Метод команды на снятие выделения всех типов зон
    private void UncheckAllTypeZones() {
        foreach(var catVM in FilteredTypeZones) {
            catVM.IsChecked = false;
        }
    }

    // Метод обновления TypeZones
    private void UpdateTypeZones() {
        TypeZones = new ObservableCollection<TypeZoneViewModel>(GetTypeZoneViewModels());
        FilteredTypeZones = new ObservableCollection<TypeZoneViewModel>(TypeZones);
        SelectedTypeZones = new ObservableCollection<TypeZoneViewModel>(FilteredTypeZones.Where(zone => zone.IsChecked));
    }

    // Метод обновления предупреждений в параметрах
    private void OnTypeZoneChanged(object sender, PropertyChangedEventArgs e) {
        if(sender is not TypeZoneViewModel vm) {
            return;
        }
        SelectedTypeZones = new ObservableCollection<TypeZoneViewModel>(FilteredTypeZones.Where(zone => zone.IsChecked));
    }

    // Метод получения коллекции TypeZoneViewModel для TypeModels
    private IEnumerable<TypeZoneViewModel> GetTypeZoneViewModels() {
        var param = Params.FirstOrDefault()?.ParamMap.SourceParam;
        var allTypeZones = _revitRepository.GetTypeZones(param);
        var savedTypeModels = _settings.TypeZones ?? [];
        return !allTypeZones.Any()
            ? []
            : allTypeZones
                .Select(value => new TypeZoneViewModel {
                    Name = value,
                    IsChecked = savedTypeModels.Contains(value)
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
                    var def = _services.ParamAvailabilityService.GetDefinitionByName(_revitRepository.Document, vm.SourceParamName);
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
                    var def = _services.ParamAvailabilityService.GetDefinitionByName(_revitRepository.Document, vm.TargetParamName);
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
        var savedParamMaps = _settings.ParamMaps;
        var savedLookup = savedParamMaps.ToDictionary(paramMap => paramMap.Type, paramMap => paramMap);

        return defaultParamMaps.Select(defaultParamMap => {
            bool exists = savedLookup.TryGetValue(defaultParamMap.Type, out var savedParamMap);
            var paramMap = exists ? savedParamMap : defaultParamMap;
            bool hasSourceParam = paramMap.SourceParam != null;
            bool hasTargetParam = paramMap.TargetParam != null;

            return new ParamViewModel(_localizationService, _services) {
                ParamMap = paramMap,
                Description = _localizationService.GetLocalizedString($"CommonSettingViewModel.{paramMap.Type}Description"),
                DetailDescription = _localizationService.GetLocalizedString($"CommonSettingViewModel.{paramMap.Type}DetailDescription"),
                SourceParamName = paramMap.SourceParam?.Name ?? string.Empty,
                TargetParamName = paramMap.TargetParam?.Name ?? string.Empty,
                IsChecked = exists,
                HasSourceParam = hasSourceParam,
                HasTargetParam = hasTargetParam,
            };
        });
    }

    // Метод подписанный на события MainViewModel
    private void OnPropertyChanged(object sender, PropertyChangedEventArgs e) {
        if(e.PropertyName == nameof(SelectedTypeAlgorithm)) {
            UpdateParams();
        }
    }

    // Метод получения коллекции AlgorithmViewModel для TypeAlgorithms
    private IEnumerable<AlgorithmViewModel> GetTypeAlgorithmsViewModels() {
        var currentAlgorithmType = _settings.AlgorithmType;
        var algorithmTypes = Enum.GetValues(typeof(AlgorithmType)).Cast<AlgorithmType>();
        return algorithmTypes
            .Select(algorithmType => new AlgorithmViewModel {
                Name = _localizationService.GetLocalizedString($"CommonSettingViewModel.{algorithmType}"),
                AlgorithmType = algorithmType
            })
            .OrderByDescending(vm => vm.AlgorithmType == currentAlgorithmType)
            .ThenBy(vm => vm.Name);
    }

    // Метод загрузки окна
    private void LoadView() {
        TypeAlgorithms = new ObservableCollection<AlgorithmViewModel>(GetTypeAlgorithmsViewModels());
        SelectedTypeAlgorithm = TypeAlgorithms.FirstOrDefault();

        // Подписка на события для обновления IsAdvancedAlgorithm
        PropertyChanged += OnPropertyChanged;

        Params = new ObservableCollection<ParamViewModel>(GetParamViewModels());
        // Подписка на события в ParamViewModel
        foreach(var param in Params) {
            param.PropertyChanged += OnParamChanged;
        }
        UpdateParamWarnings();

        TypeZones = new ObservableCollection<TypeZoneViewModel>(GetTypeZoneViewModels());
        // Подписка на события в ParamViewModel
        foreach(var typeZone in TypeZones) {
            typeZone.PropertyChanged += OnTypeZoneChanged;
        }
        FilteredTypeZones = new ObservableCollection<TypeZoneViewModel>(TypeZones);
        SelectedTypeZones = new ObservableCollection<TypeZoneViewModel>(FilteredTypeZones.Where(zone => zone.IsChecked));
    }
}
