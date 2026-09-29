using System.Linq;
using System.Windows.Input;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitEnumerateBySpline.Models;
using RevitEnumerateBySpline.Models.Enums;

namespace RevitEnumerateBySpline.ViewModels;

/// <summary>
/// Основная ViewModel главного окна плагина.
/// </summary>
internal class MainViewModel : BaseViewModel {
    private readonly ILocalizationService _localizationService;
    private readonly PluginConfig _pluginConfig;
    private readonly RevitRepository _revitRepository;

    private string? _errorText;
    private string _saveProperty = string.Empty;
    private bool _hasCommonSettingsErrors;

    /// <summary>
    /// Создает экземпляр основной ViewModel главного окна.
    /// </summary>
    /// <param name="pluginConfig">Настройки плагина.</param>
    /// <param name="revitRepository">Класс доступа к интерфейсу Revit.</param>
    /// <param name="localizationService">Интерфейс доступа к сервису локализации.</param>/
    /// <param name="rangeViewModel"></param>
    /// <param name="spatialModelsViewModel"></param>
    /// <param name="curveModelsViewModel"></param>
    /// <param name="commonParamSettingsViewModel"></param>
    /// <param name="parkingSpaceParamSettingsViewModel"></param>
    public MainViewModel(
        PluginConfig pluginConfig,
        RevitRepository revitRepository,
        ILocalizationService localizationService,
        RangeViewModel rangeViewModel,
        SpatialModelsViewModel spatialModelsViewModel,
        CurveModelsViewModel curveModelsViewModel,
        CommonParamSettingsViewModel commonParamSettingsViewModel,
        ParkingSpaceParamSettingsViewModel parkingSpaceParamSettingsViewModel) {
        
        _pluginConfig = pluginConfig;
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

    /// <summary>
    /// Свойство для примера. (требуется удалить)
    /// </summary>
    public string SaveProperty {
        get => _saveProperty;
        set => RaiseAndSetIfChanged(ref _saveProperty, value);
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
    }

    /// <summary>
    /// Метод проверки возможности выполнения команды применения настроек.
    /// </summary>
    /// <returns>В случае когда true - команда может выполниться, в случае false - нет.</returns>
    private bool CanAcceptView() {
        switch (RangeViewModel?.SelectedRange?.ElementsProvider?.Type)
        {
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
        RevitSettings setting = _pluginConfig.GetSettings(_revitRepository.Document);
        SaveProperty = setting?.SaveProperty ?? _localizationService.GetLocalizedString("MainWindow.Hello");
    }

    /// <summary>
    /// Сохранение настроек плагина.
    /// </summary>
    private void SaveConfig() {
        RevitSettings setting = _pluginConfig.GetSettings(_revitRepository.Document)
                                ?? _pluginConfig.AddSettings(_revitRepository.Document);

        setting.SaveProperty = SaveProperty;
        _pluginConfig.SaveProjectConfig();
    }
}
