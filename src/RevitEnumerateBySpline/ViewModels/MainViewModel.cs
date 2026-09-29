using System.Linq;
using System.Windows.Input;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitEnumerateBySpline.Models;
using RevitEnumerateBySpline.Models.Enums;
using RevitEnumerateBySpline.Models.Factories;
using RevitEnumerateBySpline.Models.Services;

namespace RevitEnumerateBySpline.ViewModels;

/// <summary>
/// Основная ViewModel главного окна плагина.
/// </summary>
internal class MainViewModel : BaseViewModel {
    private readonly ILocalizationService _localizationService;
    private readonly ParamService _paramService;
    private readonly ProvidersFactory _providersFactory;
    private readonly PluginConfig _pluginConfig;
    private readonly RevitRepository _revitRepository;

    private string? _errorText;
    private string _saveProperty = string.Empty;
    private bool _hasCommonSettingsErrors = false;

    /// <summary>
    /// Создает экземпляр основной ViewModel главного окна.
    /// </summary>
    /// <param name="pluginConfig">Настройки плагина.</param>
    /// <param name="revitRepository">Класс доступа к интерфейсу Revit.</param>
    /// <param name="localizationService">Интерфейс доступа к сервису локализации.</param>
    /// <param name="commonSettingsViewModel"></param>
    /// <param name="providersFactory"></param>
    /// <param name="paramService"></param>
    public MainViewModel(
        PluginConfig pluginConfig,
        RevitRepository revitRepository,
        ILocalizationService localizationService,
        ProvidersFactory providersFactory,
        ParamService paramService,
        CommonSettingsViewModel commonSettingsViewModel) {
        
        _pluginConfig = pluginConfig;
        _revitRepository = revitRepository;
        _localizationService = localizationService;
        _providersFactory = providersFactory;
        _paramService = paramService;

        CommonSettingsViewModel = commonSettingsViewModel;

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
    /// <remarks>В случаях, когда используется немодальное окно, требуется данную команду удалять.</remarks>
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

    public CommonSettingsViewModel CommonSettingsViewModel { get; set; }

    /// <summary>
    /// Метод загрузки главного окна.
    /// </summary>
    /// <remarks>В данном методе должна происходить загрузка настроек окна, а так же инициализация полей окна.</remarks>
    private void LoadView() {
        LoadConfig();
        
        CommonSettingsViewModel.LoadView();
    }

    /// <summary>
    /// Метод применения настроек главного окна. (выполнение плагина)
    /// </summary>
    /// <remarks>
    /// В данном методе должны браться настройки пользователя и сохраняться в конфиг, а так же быть основной код плагина.
    /// </remarks>
    private void AcceptView() {
        SaveConfig();
    }

    /// <summary>
    /// Метод проверки возможности выполнения команды применения настроек.
    /// </summary>
    /// <returns>В случае когда true - команда может выполниться, в случае false - нет.</returns>
    /// <remarks>
    /// В данном методе происходит валидация ввода пользователя и уведомление его о неверных значениях.
    /// В методе проверяемые свойства окна должны быть отсортированы в таком же порядке как в окне (сверху-вниз)
    /// </remarks>
    private bool CanAcceptView() {
        if(CommonSettingsViewModel.RangeViewModel?.SelectedRange?.ElementsProvider?.Type == ElementsProviderType.SelectedElementsProvider
           && !_revitRepository.HasSelectedRooms()) {
            ErrorText = _localizationService.GetLocalizedString("MainViewModel.NotSelected");
            HasCommonSettingsErrors = true;
            return false;
        }
        if(CommonSettingsViewModel.RangeViewModel?.SelectedRange?.ElementsProvider?.Type == ElementsProviderType.CurrentViewProvider
           && !_revitRepository.HasRoomsOnCurrentView()) {
            ErrorText = _localizationService.GetLocalizedString("MainViewModel.NoRoomsOnView");
            HasCommonSettingsErrors = true;
            return false;
        }
        if(CommonSettingsViewModel.SpatialModelsViewModel?.FilteredSpatialModelViewModels?
               .Any(x => x.IsChecked) != true) {
            ErrorText = _localizationService.GetLocalizedString("MainViewModel.NoSelection");
            HasCommonSettingsErrors = true;
            return false;
        }
        if(CommonSettingsViewModel.CurveModelsViewModel?.CurveModelViewModels.Count == 0) {
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
