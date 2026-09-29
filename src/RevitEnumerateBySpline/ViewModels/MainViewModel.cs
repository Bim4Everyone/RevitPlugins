using System.Linq;
using System.Windows.Input;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitEnumerateBySpline.Models;
using RevitEnumerateBySpline.Models.Enums;
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

    private string? _errorText;
    private string _saveProperty = string.Empty;
    private bool _hasCommonSettingsErrors;
    
    private ConfigSettings? _configSettings;
    private EnumeratorBySplineSettings? _enumeratorBySplineSettings;

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
    public MainViewModel(
        PluginConfig pluginConfig,
        SystemPluginConfig systemPluginConfig,
        RevitRepository revitRepository,
        ILocalizationService localizationService,
        RangeViewModel rangeViewModel,
        SpatialModelsViewModel spatialModelsViewModel,
        CurveModelsViewModel curveModelsViewModel,
        CommonParamSettingsViewModel commonParamSettingsViewModel,
        ParkingSpaceParamSettingsViewModel parkingSpaceParamSettingsViewModel) {
        
        _pluginConfig = pluginConfig;
        _systemPluginConfig = systemPluginConfig;
        _revitRepository = revitRepository;
        _localizationService = localizationService;
        
        SpatialModelsViewModel = spatialModelsViewModel;
        CurveModelsViewModel = curveModelsViewModel;
        CommonParamSettingsViewModel = commonParamSettingsViewModel;
        ParkingSpaceParamSettingsViewModel = parkingSpaceParamSettingsViewModel;
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
        get => _hasCommonSettingsErrors;
        set => RaiseAndSetIfChanged(ref _hasCommonSettingsErrors, value);
    }

    /// <summary>
    /// Текст ошибки, который отображается при неверном вводе пользователя.
    /// </summary>
    public string? ErrorText {
        get => _errorText;
        set => RaiseAndSetIfChanged(ref _errorText, value);
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
        LoadConfig();
        RangeViewModel.LoadView();
    }

    /// <summary>
    /// Метод применения настроек главного окна. (выполнение плагина)
    /// </summary>
    private void AcceptView() {
        SaveConfig();
        SaveSettings();
        
        

    }

    /// <summary>
    /// Метод проверки возможности выполнения команды применения настроек.
    /// </summary>
    /// <returns>В случае когда true - команда может выполниться, в случае false - нет.</returns>
    private bool CanAcceptView() {
        switch (RangeViewModel?.SelectedRange?.ElementsProvider?.Type) {
            case ElementsProviderType.SelectedElementsProvider
                when !_revitRepository.HasSelectedRooms():
                ErrorText = _localizationService.GetLocalizedString("MainViewModel.NotSelected");
                HasCommonSettingsErrors = true;
                return false;
            case ElementsProviderType.CurrentViewProvider
                when !_revitRepository.HasRoomsOnCurrentView():
                ErrorText = _localizationService.GetLocalizedString("MainViewModel.NoRoomsOnView");
                HasCommonSettingsErrors = true;
                return false;
        }
        if(SpatialModelsViewModel?.FilteredSpatialModelViewModels?
               .Any(x => x.IsChecked) != true) {
            ErrorText = _localizationService.GetLocalizedString("MainViewModel.NoSelection");
            HasCommonSettingsErrors = true;
            return false;
        }
        if(CurveModelsViewModel?.CurveModelViewModels.Count == 0) {
            ErrorText = _localizationService.GetLocalizedString("MainViewModel.NoCurves");
            HasCommonSettingsErrors = true;
            return false;
        }
        ErrorText = null;
        HasCommonSettingsErrors = false;
        return true;
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
        _configSettings?.StartNumber ??= _systemPluginConfig.DefaultStartNumber;
        _configSettings?.Prefix ??= _systemPluginConfig.DefaultPrefix;
        _configSettings?.Suffix ??= _systemPluginConfig.DefaultSuffix;
        _configSettings?.SearchKey ??= _systemPluginConfig.DefaultSearchKey;
        _configSettings?.DependentParam ??= _systemPluginConfig.DefaultDependentParam;
        _configSettings?.DependentSearchParam ??= _systemPluginConfig.DefaultDependentSearchParam;
    }

    /// <summary>
    /// Сохранение настроек плагина.
    /// </summary>
    private void SaveConfig() {
        var setting = _pluginConfig.GetSettings(_revitRepository.Document)
                      ?? _pluginConfig.AddSettings(_revitRepository.Document);
        setting.ConfigSettings = new ConfigSettings {
            StartNumber = CommonParamSettingsViewModel.StartNumber,
            Prefix = CommonParamSettingsViewModel.Prefix,
            Suffix = CommonParamSettingsViewModel.Suffix,
            SearchKey = ParkingSpaceParamSettingsViewModel.SearchKey,
            DependentParam = ParkingSpaceParamSettingsViewModel.SelectedDependentParam?.RevitParam,
            DependentSearchParam = ParkingSpaceParamSettingsViewModel.SelectedDependentSearchParam?.RevitParam
        };
        _pluginConfig.SaveProjectConfig();
    }
    
    /// <summary>
    /// Сохранение настроек плагина.
    /// </summary>
    private void SaveSettings() {
        _enumeratorBySplineSettings = new EnumeratorBySplineSettings { 
            StartNumber = CommonParamSettingsViewModel.StartNumber,
            Prefix = CommonParamSettingsViewModel.Prefix,
            Suffix = CommonParamSettingsViewModel.Suffix,
            SearchKey = ParkingSpaceParamSettingsViewModel.SearchKey,
            DependentParam = ParkingSpaceParamSettingsViewModel.SelectedDependentParam?.RevitParam,
            DependentSearchParam = ParkingSpaceParamSettingsViewModel.SelectedDependentSearchParam?.RevitParam,
            SpatialModels = SpatialModelsViewModel.FilteredSpatialModelViewModels?
                .Where(x => x.IsChecked)
                .Select(x => x.SpatialModel)
                .ToList(),
            CurveModels = CurveModelsViewModel.CurveModelViewModels
                .Select(x => x.CurveModel)
                .ToList()
        };
    }
}
