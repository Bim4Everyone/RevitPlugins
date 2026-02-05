using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;

using dosymep.Revit;
using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitBuildCoordVolumes.Models;
using RevitBuildCoordVolumes.Models.Enums;
using RevitBuildCoordVolumes.Models.Geometry;
using RevitBuildCoordVolumes.Models.Services;
using RevitBuildCoordVolumes.Models.Settings;

namespace RevitBuildCoordVolumes.ViewModels;

internal class MainViewModel : BaseViewModel {
    private readonly PluginConfig _pluginConfig;
    private readonly SystemPluginConfig _systemPluginConfig;
    private readonly RevitRepository _revitRepository;
    private readonly BuildCoordVolumeServices _services;
    private BuildCoordVolumeSettings _settings;
    private CommonSettingViewModel _commonSettingViewModel;
    private SlabBasedSettingViewModel _slabBasedSettingViewModel;
    private bool _requiredCheckArea;
    private bool _isSlabBasedAlgorithm;
    private string _errorText;

    public MainViewModel(
        PluginConfig pluginConfig,
        SystemPluginConfig systemPluginConfig,
        RevitRepository revitRepository,
        BuildCoordVolumeServices buildCoordVolumeServices,
        IProgressDialogFactory progressDialogFactory,
        IMessageBoxService messageBoxService) {
        _pluginConfig = pluginConfig;
        _systemPluginConfig = systemPluginConfig;
        _revitRepository = revitRepository;
        _services = buildCoordVolumeServices;

        MessageBoxService = messageBoxService
            ?? throw new ArgumentNullException(nameof(messageBoxService));

        ProgressDialogFactory = progressDialogFactory
            ?? throw new System.ArgumentNullException(nameof(progressDialogFactory));

        LoadViewCommand = RelayCommand.Create(LoadView);
        CheckAreaCommand = RelayCommand.Create(CheckArea, CanCheckArea);
        AcceptViewCommand = RelayCommand.Create(AcceptView, CanAcceptView);
    }

    public ICommand LoadViewCommand { get; }
    public ICommand CheckAreaCommand { get; }
    public ICommand AcceptViewCommand { get; }

    public IProgressDialogFactory ProgressDialogFactory { get; }
    public IMessageBoxService MessageBoxService { get; }

    public CommonSettingViewModel CommonSettingViewModel {
        get => _commonSettingViewModel;
        set => RaiseAndSetIfChanged(ref _commonSettingViewModel, value);
    }
    public SlabBasedSettingViewModel SlabBasedSettingViewModel {
        get => _slabBasedSettingViewModel;
        set => RaiseAndSetIfChanged(ref _slabBasedSettingViewModel, value);
    }
    public bool RequiredCheckArea {
        get => _requiredCheckArea;
        set => RaiseAndSetIfChanged(ref _requiredCheckArea, value);
    }
    public bool IsSlabBasedAlgorithm {
        get => _isSlabBasedAlgorithm;
        set => RaiseAndSetIfChanged(ref _isSlabBasedAlgorithm, value);
    }
    public string ErrorText {
        get => _errorText;
        set => RaiseAndSetIfChanged(ref _errorText, value);
    }

    // Метод обновляющий свойство для скрытия/отображения дополнительных настроек основного окна
    private void UpdateVisibilitySettings() {
        IsSlabBasedAlgorithm = CommonSettingViewModel.SelectedTypeAlgorithm.AlgorithmType == AlgorithmType.SlabBasedAlgorithm;
    }

    // Метод обновления свойства необходимости проверки зоны
    private void UpdateRequiredCheckArea() {
        RequiredCheckArea = true;
    }

    // Метод подписанный на обновление CommonSettingViewModel
    private void OnCommonSettingsChanged(object sender, PropertyChangedEventArgs e) {
        switch(e.PropertyName) {
            case nameof(CommonSettingViewModel.TypeAlgorithms):
            case nameof(CommonSettingViewModel.SelectedTypeAlgorithm):
                UpdateVisibilitySettings();
                break;
            case nameof(CommonSettingViewModel.TypeZones):
            case nameof(CommonSettingViewModel.SelectedTypeZone):
                UpdateRequiredCheckArea();
                CanAcceptView();
                break;
            case nameof(CommonSettingViewModel.Params):
                CanAcceptView();
                break;
        }
    }

    // Метод подписанный на обновление SlabBasedSettingViewModel
    private void OnSlabBasedSettingsChanged(object sender, PropertyChangedEventArgs e) {
        if(!IsSlabBasedAlgorithm) {
            return;
        }
        switch(e.PropertyName) {
            case nameof(SlabBasedSettingViewModel.FilteredDocuments):
            case nameof(SlabBasedSettingViewModel.FilteredSlabs):
            case nameof(SlabBasedSettingViewModel.SquareSideMm):
            case nameof(SlabBasedSettingViewModel.SquareAngleDeg):
            case nameof(SlabBasedSettingViewModel.Levels):
                CanAcceptView();
                break;
        }
    }

    // Метод загрузки окна
    private void LoadView() {
        LoadConfig();
        CommonSettingViewModel = new CommonSettingViewModel(_systemPluginConfig, _revitRepository, _settings, _services);
        CommonSettingViewModel.PropertyChanged += OnCommonSettingsChanged;

        UpdateVisibilitySettings();

        SlabBasedSettingViewModel = new SlabBasedSettingViewModel(_systemPluginConfig, _revitRepository, _settings, _services);
        SlabBasedSettingViewModel.PropertyChanged += OnSlabBasedSettingsChanged;

        UpdateRequiredCheckArea();
    }

    // Метод получения всех зон
    private List<SpatialObject> GetSpatialObjects() {
        string areaType = _settings.TypeZone;
        var areaTypeParam = _settings.ParamMaps.First().SourceParam;
        return _revitRepository.GetSpatialObjects(areaType, areaTypeParam).ToList();
    }

    // Метод проверки зон
    private void CheckArea() {
        SaveSettings();
        _settings.SpatialObjects = GetSpatialObjects();
        var warningElements = _services.SpatialElementCheckService.CheckSpatialObjects(_settings, _revitRepository);
        if(!warningElements.Any()) {
            string o = _services.LocalizationService.GetLocalizedString("MainViewModel.MessageBoxTitle");
            MessageBoxService.Show(
                _services.LocalizationService.GetLocalizedString("MainViewModel.MessageBoxText"),
                _services.LocalizationService.GetLocalizedString("MainViewModel.MessageBoxTitle"));
            RequiredCheckArea = false;
            return;
        }
        SaveConfig();
        _services.WindowService.CloseMainWindow();
        _services.WindowService.ShowWarningWindow(warningElements);
    }

    // Метод проверки возможности выполнения метода проверки зон
    private bool CanCheckArea() {
        return CommonSettingViewModel != null
            && CommonSettingViewModel.SelectedTypeZone != null
            && RequiredCheckArea;
    }

    //Основной метод
    private void AcceptView() {

        SaveSettings();
        SaveConfig();

        var processor = new BuildCoordVolumesProcessor(_revitRepository, _settings, _services);

        int count = _settings.SpatialObjects.Count;
        using var progressDialogService = ProgressDialogFactory.CreateDialog();
        var progress = progressDialogService.CreateProgress();
        var ct = progressDialogService.CreateCancellationToken();

        progressDialogService.MaxValue = count;
        progressDialogService.StepValue = count / 10;
        progressDialogService.DisplayTitleFormat = _services.LocalizationService.GetLocalizedString("MainViewModel.ProgressTitle");

        progressDialogService.Show();

        // Основной метод построения объемов
        processor.Run(progress, ct);
    }

    // Метод проверки возможности выполнения основного метода
    private bool CanAcceptView() {
        if(CommonSettingViewModel != null) {
            if(CommonSettingViewModel.Params != null) {
                var descriptionParamVM = CommonSettingViewModel.Params.Where(pvm => pvm.ParamMap.Type == ParamType.DescriptionParam).FirstOrDefault();
                if(descriptionParamVM != null && !descriptionParamVM.IsChecked) {
                    ErrorText = _services.LocalizationService.GetLocalizedString("MainViewModel.NoMainParam");
                    return false;
                }
                var topZoneParamVM = CommonSettingViewModel.Params.Where(pvm => pvm.ParamMap.Type == ParamType.TopZoneParam).FirstOrDefault();
                if(topZoneParamVM != null && !topZoneParamVM.IsChecked && !IsSlabBasedAlgorithm) {
                    ErrorText = _services.LocalizationService.GetLocalizedString("MainViewModel.NoTopZoneParam");
                    return false;
                }
                var bottomZoneParamVM = CommonSettingViewModel.Params.Where(pvm => pvm.ParamMap.Type == ParamType.BottomZoneParam).FirstOrDefault();
                if(bottomZoneParamVM != null && !bottomZoneParamVM.IsChecked && !IsSlabBasedAlgorithm) {
                    ErrorText = _services.LocalizationService.GetLocalizedString("MainViewModel.NoBottomZoneParam");
                    return false;
                }
                var volumeParamVM = CommonSettingViewModel.Params.Where(pvm => pvm.ParamMap.Type == ParamType.VolumeParam).FirstOrDefault();
                if(volumeParamVM != null && !volumeParamVM.IsChecked) {
                    ErrorText = _services.LocalizationService.GetLocalizedString("MainViewModel.NoVolumeParam");
                    return false;
                }
                var checkedParams = CommonSettingViewModel.Params.Where(p => p.IsChecked).ToList();
                if(checkedParams.Count == 0) {
                    ErrorText = _services.LocalizationService.GetLocalizedString("MainViewModel.NoParams");
                    return false;
                }
            }
            if(CommonSettingViewModel.HasParamWarning) {
                ErrorText = _services.LocalizationService.GetLocalizedString("MainViewModel.ErrorParams");
                return false;
            }
            if(CommonSettingViewModel.SelectedTypeZone == null) {
                ErrorText = _services.LocalizationService.GetLocalizedString("MainViewModel.NoTypeZone");
                return false;
            }
        }
        if(SlabBasedSettingViewModel != null && IsSlabBasedAlgorithm) {
            if(SlabBasedSettingViewModel.FilteredDocuments != null) {
                var checkedDocs = SlabBasedSettingViewModel.FilteredDocuments.Where(p => p.IsChecked).ToList();
                if(checkedDocs.Count == 0) {
                    ErrorText = _services.LocalizationService.GetLocalizedString("MainViewModel.NoDocuments");
                    return false;
                }
            }
            if(SlabBasedSettingViewModel.FilteredSlabs != null) {
                var checkedSlabs = SlabBasedSettingViewModel.FilteredSlabs.Where(p => p.IsChecked).ToList();
                if(checkedSlabs.Count == 0) {
                    ErrorText = _services.LocalizationService.GetLocalizedString("MainViewModel.NoSlabs");
                    return false;
                }
            }
            if(SlabBasedSettingViewModel.FilteredLevels != null) {
                var checkedLevels = SlabBasedSettingViewModel.FilteredLevels.Where(p => p.IsChecked).ToList();
                if(checkedLevels.Count == 0) {
                    ErrorText = _services.LocalizationService.GetLocalizedString("MainViewModel.NoLevels");
                    return false;
                }
            }
            if(!double.TryParse(SlabBasedSettingViewModel.SquareAngleDeg, out double resultAngleDeg)) {
                ErrorText = _services.LocalizationService.GetLocalizedString("MainViewModel.SquareValueNoDouble");
                return false;
            }
            if(!double.TryParse(SlabBasedSettingViewModel.SquareSideMm, out double resultSideMm)) {
                ErrorText = _services.LocalizationService.GetLocalizedString("MainViewModel.SquareValueNoDouble");
                return false;
            }
            if(resultSideMm <= 0) {
                ErrorText = _services.LocalizationService.GetLocalizedString("MainViewModel.SquareSideNoNegate");
                return false;
            } else if(resultSideMm > 1000) {
                ErrorText = _services.LocalizationService.GetLocalizedString("MainViewModel.SquareSideBig");
                return false;
            } else if(resultSideMm < 10) {
                ErrorText = _services.LocalizationService.GetLocalizedString("MainViewModel.SquareSideSmall");
                return false;
            }
        }
        if(RequiredCheckArea) {
            ErrorText = _services.LocalizationService.GetLocalizedString("MainViewModel.RequiredCheckArea");
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
            ? []
            : configSettings.Documents
                .Select(_revitRepository.FindDocumentsByName)
                .Where(doc => doc != null)
                .ToList();

        var typeSlabs = configSettings.TypeSlabs.Count == 0
            ? _revitRepository.GetTypeSlabsByDocs(documents)
                .Where(name => _systemPluginConfig.DefaultSlabTypeNames.Any(type => name.Contains(type)))
                .ToList()
            : configSettings.TypeSlabs;

        _settings = new BuildCoordVolumeSettings {
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

    // Метод сохранения настроек
    private void SaveSettings() {
        var algorithmType = CommonSettingViewModel.SelectedTypeAlgorithm.AlgorithmType;
        string typeZone = CommonSettingViewModel.SelectedTypeZone.Name;
        var paramMaps = CommonSettingViewModel.Params.Where(vm => vm.IsChecked).Select(vm => vm.ParamMap).ToList();
        var builderMode = SlabBasedSettingViewModel.SelectedBuilderMode.BuilderMode;
        var documents = SlabBasedSettingViewModel.FilteredDocuments.Where(vm => vm.IsChecked).Select(d => d.Document).ToList();
        var typeSlabs = SlabBasedSettingViewModel.FilteredSlabs.Where(vm => vm.IsChecked).Select(vm => vm.Name).ToList();
        var levels = SlabBasedSettingViewModel.FilteredLevels.Where(vm => vm.IsChecked).Select(vm => vm.Level).ToList();
        double squareSide = Convert.ToDouble(SlabBasedSettingViewModel.SquareSideMm);
        double squareAngle = Convert.ToDouble(SlabBasedSettingViewModel.SquareAngleDeg);

        _settings.AlgorithmType = algorithmType;
        _settings.BuilderMode = builderMode;
        _settings.Documents = documents;
        _settings.TypeSlabs = typeSlabs;
        _settings.TypeZone = typeZone;
        _settings.ParamMaps = paramMaps;
        _settings.Levels = levels;
        _settings.SquareSideMm = squareSide;
        _settings.SquareAngleDeg = squareAngle;
    }

    // Метод сохранения конфигурации пользователя
    private void SaveConfig() {
        var configSettings = new ConfigSettings {
            AlgorithmType = _settings.AlgorithmType,
            BuilderMode = _settings.BuilderMode,
            Documents = _settings.Documents.Select(doc => doc.GetUniqId()).ToList(),
            TypeZone = _settings.TypeZone,
            ParamMaps = _settings.ParamMaps,
            TypeSlabs = _settings.TypeSlabs,
            SquareSideMm = _settings.SquareSideMm,
            SquareAngleDeg = _settings.SquareAngleDeg
        };

        var setting = _pluginConfig.GetSettings(_revitRepository.Document)
            ?? _pluginConfig.AddSettings(_revitRepository.Document);
        setting.ConfigSettings = configSettings;
        _pluginConfig.SaveProjectConfig();
    }
}
