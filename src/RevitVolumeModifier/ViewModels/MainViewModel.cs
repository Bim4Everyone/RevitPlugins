using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone.SimpleServices;
using dosymep.Revit;
using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitVolumeModifier.Interfaces;
using RevitVolumeModifier.Models;

namespace RevitVolumeModifier.ViewModels;

internal class MainViewModel : BaseViewModel {
    private readonly ILocalizationService _localizationService;
    private readonly IParamAvailabilityService _paramAvailabilityService;
    private readonly IRevitParamFactory _revitParamFactory;
    private readonly RevitRepository _revitRepository;
    private readonly SystemPluginConfig _systemPluginConfig;
    private readonly PluginConfig _pluginConfig;

    private List<ParamModel> _paramModels;
    private ICollection<ElementId> _elementIds;
    private ObservableCollection<ParamViewModel> _paramViewModels;
    private bool _hasParamWarning;

    private string _errorText;

    public MainViewModel(
        ILocalizationService localizationService,
        IParamAvailabilityService paramAvailabilityService,
        IRevitParamFactory revitParamFactory,
        RevitRepository revitRepository,
        SystemPluginConfig systemPluginConfig,
        PluginConfig pluginConfig) {

        _localizationService = localizationService;
        _paramAvailabilityService = paramAvailabilityService;
        _revitParamFactory = revitParamFactory;
        _revitRepository = revitRepository;
        _systemPluginConfig = systemPluginConfig;
        _pluginConfig = pluginConfig;

        LoadViewCommand = RelayCommand.Create(LoadView);
        SaveConfigCommand = RelayCommand.Create(SaveConfig);
        JoinCommand = RelayCommand.Create(Join);
        DivideBySelectHorizontalPointCommand = RelayCommand.Create(DivideBySelectHorizontalPoint);
        DivideBySelectVerticalPointCommand = RelayCommand.Create(DivideBySelectVerticalPoint);
        DivideBySelectThreePointCommand = RelayCommand.Create(DivideBySelectThreePointPoint);
        DivideBySelectFacesCommand = RelayCommand.Create(DivideBySelectFacesPoint);
        CutCommand = RelayCommand.Create(Cut);
    }

    public ICommand LoadViewCommand { get; }
    public ICommand SaveConfigCommand { get; }
    public ICommand JoinCommand { get; }
    public ICommand DivideBySelectHorizontalPointCommand { get; }
    public ICommand DivideBySelectVerticalPointCommand { get; }
    public ICommand DivideBySelectThreePointCommand { get; }
    public ICommand DivideBySelectFacesCommand { get; }
    public ICommand CutCommand { get; }

    public ICollection<ElementId> ElementIds {
        get => _elementIds;
        set => RaiseAndSetIfChanged(ref _elementIds, value);
    }
    public ObservableCollection<ParamViewModel> ParamViewModels {
        get => _paramViewModels;
        set => RaiseAndSetIfChanged(ref _paramViewModels, value);
    }
    public bool HasParamWarning {
        get => _hasParamWarning;
        set => RaiseAndSetIfChanged(ref _hasParamWarning, value);
    }
    public string ErrorText {
        get => _errorText;
        set => RaiseAndSetIfChanged(ref _errorText, value);
    }

    // Метод обновления предупреждений в параметрах
    private void UpdateParamWarnings() {
        foreach(var paramViewModel in ParamViewModels) {
            paramViewModel.UpdateWarning(_revitRepository.Document, _localizationService, _paramAvailabilityService);
        }
        HasParamWarning = ParamViewModels.Any(param => param.HasWarning);
    }

    // Метод подписанный на событие изменения ParamViewModel
    private void OnParamChanged(object sender, PropertyChangedEventArgs e) {
        if(sender is not ParamViewModel vm) {
            return;
        }
        switch(e.PropertyName) {
            case nameof(ParamViewModel.RevitParamName):
                UpdateParamWarnings();
                if(!vm.HasWarning) {
                    var def = _paramAvailabilityService.GetDefinitionByName(_revitRepository.Document, vm.RevitParamName);
                    var newParam = _revitParamFactory.Create(_revitRepository.Document, def.GetElementId());
                    vm.ParamModel.RevitParam = newParam;
                }
                break;
        }
    }

    // Метод получения коллекции ParamViewModel для Params
    private IEnumerable<ParamViewModel> GetParamViewModels() {
        return _paramModels
            .Select(param => new ParamViewModel {
                ParamModel = param,
                RevitParamName = param.RevitParam.Name,
                Description = _localizationService.GetLocalizedString($"MainViewModel.{param.ParamType}Description"),
                DetailDescription = _localizationService.GetLocalizedString($"MainViewModel.{param.ParamType}DetailDescription")
            });
    }

    private void Join() { }

    private void DivideBySelectHorizontalPoint() { }

    private void DivideBySelectVerticalPoint() { }

    private void DivideBySelectThreePointPoint() { }

    private void DivideBySelectFacesPoint() { }

    private void Cut() { }

    // Метод загрузки вида
    private void LoadView() {
        LoadConfig();
        ParamViewModels = new ObservableCollection<ParamViewModel>(GetParamViewModels());
        // Подписка на события в ParamViewModel
        foreach(var param in ParamViewModels) {
            param.PropertyChanged += OnParamChanged;
        }
        UpdateParamWarnings();
    }

    // Метод загрузки конфигурации
    private void LoadConfig() {
        var setting = _pluginConfig.GetSettings(_revitRepository.Document);
        _paramModels = setting?.ParamModels ?? _systemPluginConfig.GetDefaultParams();
    }

    // Метод сохранения конфигурации
    public void SaveConfig() {
        var setting = _pluginConfig.GetSettings(_revitRepository.Document)
                                ?? _pluginConfig.AddSettings(_revitRepository.Document);
        setting.ParamModels = ParamViewModels.Select(vm => vm.ParamModel).ToList();
        _pluginConfig.SaveProjectConfig();
    }
}
