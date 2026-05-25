using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitRoundingOfAreas.Models;

namespace RevitRoundingOfAreas.ViewModels;

/// <summary>
/// Основная ViewModel главного окна плагина.
/// </summary>
internal class MainViewModel : BaseViewModel {
    private readonly PluginConfig _pluginConfig;
    private readonly SystemPluginConfig _systemPluginConfig;
    private readonly RevitRepository _revitRepository;
    private readonly ParamService _paramService;
    private readonly ILocalizationService _localizationService;

    private ConfigSettings _configSettings;

    private ObservableCollection<PhaseViewModel> _phases;
    private PhaseViewModel _selectedPhase;

    private ObservableCollection<ParamViewModel> _sourceParams;
    private ParamViewModel _selectedSourceParam;

    private ObservableCollection<ParamViewModel> _targetParams;
    private ParamViewModel _selectedTargetParam;

    private ObservableCollection<DigitViewModel> _digitCount;
    private DigitViewModel _selectedDigitCount;


    private ObservableCollection<LevelViewModel> _levels;

    private string _errorText;

    /// <summary>
    /// Создает экземпляр основной ViewModel главного окна.
    /// </summary>
    /// <param name="pluginConfig">Настройки плагина.</param>
    /// <param name="revitRepository">Класс доступа к интерфейсу Revit.</param>
    /// <param name="localizationService">Интерфейс доступа к сервису локализации.</param>
    public MainViewModel(
        PluginConfig pluginConfig,
        SystemPluginConfig systemPluginConfig,
        RevitRepository revitRepository,
        ParamService paramService,
        ILocalizationService localizationService) {

        _pluginConfig = pluginConfig;
        _systemPluginConfig = systemPluginConfig;
        _revitRepository = revitRepository;
        _paramService = paramService;
        _localizationService = localizationService;

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

    public ObservableCollection<PhaseViewModel> Phases {
        get => _phases;
        set => RaiseAndSetIfChanged(ref _phases, value);
    }

    public PhaseViewModel SelectedPhase {
        get => _selectedPhase;
        set => RaiseAndSetIfChanged(ref _selectedPhase, value);
    }

    public ObservableCollection<ParamViewModel> SourceParams {
        get => _sourceParams;
        set => RaiseAndSetIfChanged(ref _sourceParams, value);
    }

    public ParamViewModel SelectedSourceParam {
        get => _selectedSourceParam;
        set => RaiseAndSetIfChanged(ref _selectedSourceParam, value);
    }

    public ObservableCollection<ParamViewModel> TargetParams {
        get => _targetParams;
        set => RaiseAndSetIfChanged(ref _targetParams, value);
    }

    public ParamViewModel SelectedTargetParam {
        get => _selectedTargetParam;
        set => RaiseAndSetIfChanged(ref _selectedTargetParam, value);
    }

    public ObservableCollection<DigitViewModel> DigitCount {
        get => _digitCount;
        set => RaiseAndSetIfChanged(ref _digitCount, value);
    }

    public DigitViewModel SelectedDigitCount {
        get => _selectedDigitCount;
        set => RaiseAndSetIfChanged(ref _selectedDigitCount, value);
    }

    public ObservableCollection<LevelViewModel> Levels {
        get => _levels;
        set => RaiseAndSetIfChanged(ref _levels, value);
    }


    /// <summary>
    /// Текст ошибки, который отображается при неверном вводе пользователя.
    /// </summary>
    public string ErrorText {
        get => _errorText;
        set => RaiseAndSetIfChanged(ref _errorText, value);
    }


    // Метод получения коллекции DigitViewModel для DigitCount
    private IEnumerable<DigitViewModel> GetDigitViewModels() {
        for(int i = 1; i <= 3; i++) {
            yield return new DigitViewModel {
                DigitCount = i,
                Name = i.ToString(),
            };
        }
    }

    // Метод получения коллекции ParamViewModel для TargetParams
    private IEnumerable<ParamViewModel> GetTargetParamViewModels() {
        return _paramService.AllRevitParams
            .Select(param => new ParamViewModel {
                Name = param.Name,
                RevitParam = param
            })
            .Where(param => param.RevitParam.StorageType is StorageType.String);
    }

    // Метод получения коллекции ParamViewModel для SourceParams
    private IEnumerable<ParamViewModel> GetSourceParamViewModels() {
        return _paramService.AllRevitParams
            .Select(param => new ParamViewModel {
                Name = param.Name,
                RevitParam = param
            })
            .Where(param => param.RevitParam.StorageType is StorageType.Double);
    }

    // Метод получения коллекции PhaseViewModel для Phases
    private IEnumerable<PhaseViewModel> GetPhaseViewModels() {
        return _revitRepository.GetPhaseModels()
            .Select(phase => new PhaseViewModel {
                Name = phase.Name,
                ElementId = phase.ElementId
            });
    }


    /// <summary>
    /// Метод загрузки главного окна
    /// </summary>
    /// <remarks>В данном методе должна происходить загрузка настроек окна, а так же инициализация полей окна.</remarks>
    private void LoadView() {
        LoadConfig();

        Phases = new ObservableCollection<PhaseViewModel>(GetPhaseViewModels());
        SelectedPhase = Phases
            .FirstOrDefault(phase => phase.ElementId == _configSettings.SelectedPhaseId)
            ?? Phases.LastOrDefault();

        SourceParams = new ObservableCollection<ParamViewModel>(GetSourceParamViewModels());
        SelectedSourceParam = SourceParams
            .FirstOrDefault(param => param.RevitParam.Id == _configSettings.SourceParam?.Id)
            ?? SourceParams.FirstOrDefault();

        TargetParams = new ObservableCollection<ParamViewModel>(GetTargetParamViewModels());
        SelectedTargetParam = TargetParams
            .FirstOrDefault(param => param.RevitParam.Id == _configSettings.TargetParam?.Id)
            ?? TargetParams.FirstOrDefault();

        DigitCount = new ObservableCollection<DigitViewModel>(GetDigitViewModels());
        SelectedDigitCount = DigitCount
            .FirstOrDefault(digit => digit.DigitCount == _configSettings.DigitCount)
            ?? DigitCount.FirstOrDefault();
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
        ErrorText = null;
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
        if(_configSettings.SelectedPhaseId is null || _configSettings.SelectedPhaseId == ElementId.InvalidElementId) {
            var phaseId = _revitRepository.GetPhaseIdByName(_systemPluginConfig.DefaultPhaseName);

            var sourceParam = _paramService.AllRevitParams
                .FirstOrDefault();

            _configSettings.SelectedPhaseId = phaseId;
            _configSettings.SourceParam = sourceParam;
        }
    }

    /// <summary>
    /// Сохранение настроек плагина.
    /// </summary>
    private void SaveConfig() {
        var setting = _pluginConfig.GetSettings(_revitRepository.Document)
                                ?? _pluginConfig.AddSettings(_revitRepository.Document);

        _configSettings.SelectedPhaseId = SelectedPhase.ElementId;
        _configSettings.SourceParam = SelectedSourceParam.RevitParam;

        setting.ConfigSettings = _configSettings;
        _pluginConfig.SaveProjectConfig();
    }
}
