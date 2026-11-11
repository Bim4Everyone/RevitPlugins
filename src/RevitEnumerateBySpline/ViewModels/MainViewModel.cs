using System.Linq;
using System.Windows.Input;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitEnumerateBySpline.Models;
using RevitEnumerateBySpline.Models.Enums;
using RevitEnumerateBySpline.Models.Interfaces;
using RevitEnumerateBySpline.Models.Services;
using RevitEnumerateBySpline.Models.Settings;

namespace RevitEnumerateBySpline.ViewModels;

/// <summary>
/// Основная ViewModel главного окна плагина.
/// </summary>
internal class MainViewModel : BaseViewModel {
    private readonly ILocalizationService _localizationService;
    private readonly PluginConfig _pluginConfig;
    private readonly SystemPluginConfig _systemPluginConfig;
    private readonly RevitRepository _revitRepository;
    private readonly ParamService _paramService;
    private readonly IEnumerateBySpline _enumerateBySpline;
    private readonly SpatialElementCheckService _spatialElementCheckService;
    private readonly WindowService _windowService;
    private readonly string _saveProperty = string.Empty;

    private ConfigSettings? _configSettings;
    private EnumerateBySplineSettings? _enumerateBySplineSettings;
    private bool _isViewInitialized;
    

    /// <summary>
    /// Создает экземпляр основной ViewModel главного окна.
    /// </summary>
    /// <param name="pluginConfig">Настройки плагина.</param>
    /// <param name="systemPluginConfig"></param>
    /// <param name="revitRepository">Класс доступа к интерфейсу Revit.</param>
    /// <param name="localizationService">Интерфейс доступа к сервису локализации.</param>/
    /// <param name="rangeViewModel"></param>
    /// <param name="spatialModelsViewModel"></param>
    /// <param name="curveModelsViewModel"></param>
    /// <param name="commonParamSettingsViewModel"></param>
    /// <param name="parkingSpaceParamSettingsViewModel"></param>
    /// <param name="paramService"></param>
    /// <param name="enumerateBySpline"></param>
    /// <param name="spatialElementCheckService"></param>
    /// <param name="windowService"></param>
    public MainViewModel(
        PluginConfig pluginConfig,
        SystemPluginConfig systemPluginConfig,
        RevitRepository revitRepository,
        ILocalizationService localizationService,
        RangeViewModel rangeViewModel,
        SpatialModelsViewModel spatialModelsViewModel,
        CurveModelsViewModel curveModelsViewModel,
        CommonParamSettingsViewModel commonParamSettingsViewModel,
        ParkingSpaceParamSettingsViewModel parkingSpaceParamSettingsViewModel,
        ParamService paramService,
        IEnumerateBySpline enumerateBySpline,
        SpatialElementCheckService spatialElementCheckService,
        WindowService windowService) {

        _pluginConfig = pluginConfig;
        _systemPluginConfig = systemPluginConfig;
        _revitRepository = revitRepository;
        _localizationService = localizationService;

        SpatialModelsViewModel = spatialModelsViewModel;
        CurveModelsViewModel = curveModelsViewModel;
        CommonParamSettingsViewModel = commonParamSettingsViewModel;
        ParkingSpaceParamSettingsViewModel = parkingSpaceParamSettingsViewModel;
        _paramService = paramService;
        _enumerateBySpline = enumerateBySpline;
        _spatialElementCheckService = spatialElementCheckService;
        _windowService = windowService;
        RangeViewModel = rangeViewModel;

        LoadViewCommand = RelayCommand.Create(LoadView);
        AcceptViewCommand = RelayCommand.Create(AcceptView, CanAcceptView);
    }

    /// <summary>
    /// Команда загрузки главного окна.
    /// </summary>
    public ICommand LoadViewCommand { get; }

    /// <summary>
    /// Команда применения настроек главного окна. (запуск плагина)
    /// </summary>
    public ICommand AcceptViewCommand { get; }

    public bool HasCommonSettingsErrors {
        get;
        set => RaiseAndSetIfChanged(ref field, value);
    }

    public bool HasParamSettingsErrors {
        get;
        set => RaiseAndSetIfChanged(ref field, value);
    }

    /// <summary>
    /// Текст ошибки, который отображается при неверном вводе пользователя.
    /// </summary>
    public string? ErrorText {
        get;
        set => RaiseAndSetIfChanged(ref field, value);
    }

    public RangeViewModel RangeViewModel { get; set; }
    public SpatialModelsViewModel SpatialModelsViewModel { get; set; }
    public CurveModelsViewModel CurveModelsViewModel { get; set; }
    public CommonParamSettingsViewModel CommonParamSettingsViewModel { get; set; }
    public ParkingSpaceParamSettingsViewModel ParkingSpaceParamSettingsViewModel { get; set; }

    /// <summary>
    /// Метод загрузки главного окна.
    /// </summary>
    private void LoadView() {
        // Окно пересоздаётся после выбора линий, но ViewModel остаётся той же.
        // Повторная загрузка сбрасывает выбранный диапазон, помещения и параметры.
        if(_isViewInitialized) {
            return;
        }

        LoadConfig();
        RangeViewModel.LoadView();
        CommonParamSettingsViewModel.LoadView(_configSettings);
        ParkingSpaceParamSettingsViewModel.LoadView(_configSettings);
        _isViewInitialized = true;
    }

    /// <summary>
    /// Метод применения настроек главного окна. (выполнение плагина)
    /// </summary>
    private void AcceptView() {
        var settings = BuildSettings();
        var warnings = _spatialElementCheckService.CheckSpatialElements(settings.SpatialModels ?? []);
        if(warnings.Count != 0) {
            _windowService.RequestWarnings(warnings);
            return;
        }

        _enumerateBySpline.Enumerate(settings);
        SaveConfig();
    }

    /// <summary>
    /// Метод проверки возможности выполнения команды применения настроек.
    /// </summary>
    /// <returns>В случае когда true - команда может выполниться, в случае false - нет.</returns>
    private bool CanAcceptView() {
        if(RangeViewModel.SelectedRange?.ElementsProvider is null) {
            return SetValidationError("MainViewModel.RangeRequired", hasCommonSettingsErrors: true);
        }

        switch(RangeViewModel.SelectedRange?.ElementsProvider?.Type) {
            case ElementsProviderType.SelectedElementsProvider
                when !_revitRepository.HasSelectedRooms():
                return SetValidationError("MainViewModel.NotSelected", hasCommonSettingsErrors: true);
            case ElementsProviderType.CurrentViewProvider
                when !_revitRepository.HasRoomsOnCurrentView():
                return SetValidationError("MainViewModel.NoRoomsOnView", hasCommonSettingsErrors: true);
        }

        if(SpatialModelsViewModel.FilteredSpatialModelViewModels?
               .Any(x => x.IsChecked) != true) {
            return SetValidationError("MainViewModel.NoSelection", hasCommonSettingsErrors: true);
        }

        if(CurveModelsViewModel.CurveModelViewModels.Count == 0) {
            return SetValidationError("MainViewModel.NoCurves", hasCommonSettingsErrors: true);
        }

        if(CommonParamSettingsViewModel.SelectedNumberParam?.RevitParam is null) {
            return SetValidationError("MainViewModel.NumberParamRequired", hasCommonSettingsErrors: false);
        }

        if(CommonParamSettingsViewModel.StartNumber is not double startNumber
           || double.IsNaN(startNumber)
           || double.IsInfinity(startNumber)) {
            return SetValidationError("MainViewModel.StartNumberInvalid", hasCommonSettingsErrors: false);
        }

        if(ParkingSpaceParamSettingsViewModel.SelectedDependentParam?.RevitParam is null) {
            return SetValidationError("MainViewModel.DependentParamRequired", hasCommonSettingsErrors: false);
        }

        if(string.IsNullOrWhiteSpace(ParkingSpaceParamSettingsViewModel.SearchKey)) {
            return SetValidationError("MainViewModel.SearchKeyRequired", hasCommonSettingsErrors: false);
        }

        if(ParkingSpaceParamSettingsViewModel.SelectedDependentSearchParam?.RevitParam is null) {
            return SetValidationError("MainViewModel.DependentSearchParamRequired", hasCommonSettingsErrors: false);
        }

        ErrorText = null;
        HasCommonSettingsErrors = false;
        HasParamSettingsErrors = false;
        return true;
    }

    private bool SetValidationError(string errorKey, bool hasCommonSettingsErrors) {
        ErrorText = _localizationService.GetLocalizedString(errorKey);
        HasCommonSettingsErrors = hasCommonSettingsErrors;
        HasParamSettingsErrors = !hasCommonSettingsErrors;
        return false;
    }

    /// <summary>
    /// Загрузка настроек плагина.
    /// </summary>
    private void LoadConfig() {
        var setting = _pluginConfig.GetSettings(_revitRepository.Document);
        _configSettings = setting?.ConfigSettings ?? new ConfigSettings();
        ApplyDefaultsConfig();
    }

    // Метод применения дефолтных значений настроек.
    private void ApplyDefaultsConfig() {
        _configSettings?.NumberParam ??= _paramService.DefaultSpatialNumberParam;
        _configSettings?.StartNumber ??= _systemPluginConfig.DefaultStartNumber;
        _configSettings?.Prefix ??= _systemPluginConfig.DefaultPrefix;
        _configSettings?.Suffix ??= _systemPluginConfig.DefaultSuffix;
        _configSettings?.DependentParam ??= _paramService.DefaultDependentParam;
        _configSettings?.SearchKey ??= _systemPluginConfig.DefaultSearchKey;
        _configSettings?.DependentSearchParam ??= _paramService.DefaultDependentSearchParam;
    }

    /// <summary>
    /// Сохранение основных настроек плагина.
    /// </summary>
    private EnumerateBySplineSettings BuildSettings() {
        _enumerateBySplineSettings = new EnumerateBySplineSettings {
            ConfigSettings = new ConfigSettings {
                NumberParam = CommonParamSettingsViewModel.SelectedNumberParam?.RevitParam,
                StartNumber = CommonParamSettingsViewModel.StartNumber,
                Prefix = CommonParamSettingsViewModel.Prefix,
                Suffix = CommonParamSettingsViewModel.Suffix,
                DependentParam = ParkingSpaceParamSettingsViewModel.SelectedDependentParam?.RevitParam,
                SearchKey = ParkingSpaceParamSettingsViewModel.SearchKey,
                DependentSearchParam = ParkingSpaceParamSettingsViewModel.SelectedDependentSearchParam?.RevitParam
            },
            SpatialModels = SpatialModelsViewModel.FilteredSpatialModelViewModels?
                .Where(x => x.IsChecked)
                .Select(x => x.SpatialModel)
                .ToList(),
            CurveModels = CurveModelsViewModel.CurveModelViewModels
                .Select(x => x.CurveModel)
                .ToList()
        };
        return _enumerateBySplineSettings;
    }

    /// <summary>
    /// Сохранение сохраняемых настроек плагина.
    /// </summary>
    private void SaveConfig() {
        var setting = _pluginConfig.GetSettings(_revitRepository.Document)
                      ?? _pluginConfig.AddSettings(_revitRepository.Document);
        setting.ConfigSettings = _enumerateBySplineSettings?.ConfigSettings;
        _pluginConfig.SaveProjectConfig();
    }


}
