using System;
using System.Windows;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitCopyInteriorSpecs.Models;
using RevitCopyInteriorSpecs.Services;
using RevitCopyInteriorSpecs.ViewModels.MainViewModelParts;

namespace RevitCopyInteriorSpecs.ViewModels;
internal class MainViewModel : BaseViewModel {
    private readonly PluginConfig _pluginConfig;
    private readonly RevitRepository _revitRepository;
    private readonly SpecificationService _specificationService;
    private readonly DefaultParamNameService _defaultParamNameService;
    private readonly ILocalizationService _localizationService;

    private string _errorText;
    private string _errorReport = string.Empty;

    public MainViewModel(
        PluginConfig pluginConfig,
        RevitRepository revitRepository,
        SpecificationService specificationService,
        DefaultParamNameService defaultParamNameService,
        ILocalizationService localizationService) {

        _pluginConfig = pluginConfig;
        _revitRepository = revitRepository;
        _specificationService = specificationService;
        _defaultParamNameService = defaultParamNameService;
        _localizationService = localizationService;


        LoadViewCommand = RelayCommand.Create(LoadView);
        AcceptViewCommand = RelayCommand.Create(AcceptView, CanAcceptView);

        SelectedSpecsVM = new SelectedSpecsViewModel(_revitRepository);
        TasksVM = new TasksViewModel(_revitRepository);
        ParametersVM = new ParametersViewModel();
    }


    public SelectedSpecsViewModel SelectedSpecsVM { get; }
    public TasksViewModel TasksVM { get; }
    public ParametersViewModel ParametersVM { get; }

    public ICommand LoadViewCommand { get; }
    public ICommand AcceptViewCommand { get; }


    public string ErrorText {
        get => _errorText;
        set => RaiseAndSetIfChanged(ref _errorText, value);
    }


    private void LoadView() {
        LoadConfig();
        SelectedSpecsVM.GetSelectedSpecs();
    }


    private void AcceptView() {
        SaveConfig();
        string transactionName = _localizationService.GetLocalizedString("MainWindow.TransactionName");
        using(var transaction = _revitRepository.Document.StartTransaction(transactionName)) {
            foreach(var task in TasksVM.TasksForWork) {
                foreach(ViewSchedule spec in SelectedSpecsVM.SelectedSpecs) {
                    string specOldName = spec.Name;
                    string newViewSpecName = $"{specOldName}_{task.Phase.Name}_{task.GroupType}_{task.LevelShortName}";

                    try {
                        ViewSchedule newViewSpec = _specificationService.DuplicateSpec(spec, newViewSpecName);
                        SetSpecParams(newViewSpec, task);
                        ChangeSpecFilters(newViewSpec, task);
                    } catch(ArgumentException e) {
                        _errorReport += $"- {e.Message} - \"{newViewSpecName}\";" + Environment.NewLine + Environment.NewLine;
                        continue;
                    }
                }
            }
            transaction.Commit();
        }

        if(!string.IsNullOrEmpty(_errorReport)) {
            MessageBox.Show(_errorReport, _localizationService.GetLocalizedString("MainWindow.ErrorReport"));
        }
    }


    private bool CanAcceptView() {
        if(SelectedSpecsVM.SelectedSpecs.Count == 0) {
            ErrorText = _localizationService.GetLocalizedString("MainWindow.NoSpecsSelected");
            return false;
        }

        if(TasksVM.TasksForWork.Count == 0) {
            ErrorText = _localizationService.GetLocalizedString("MainWindow.NoTasksCreated");
            return false;
        }

        if(ParametersVM.GroupTypeParamName == string.Empty) {
            ErrorText = _localizationService.GetLocalizedString("MainWindow.GroupTypeParamNameEmpty");
            return false;
        }
        if(ParametersVM.LevelParamName == string.Empty) {
            ErrorText = _localizationService.GetLocalizedString("MainWindow.LevelParamNameEmpty");
            return false;
        }
        if(ParametersVM.LevelShortNameParamName == string.Empty) {
            ErrorText = _localizationService.GetLocalizedString("MainWindow.LevelShortNameParamNameEmpty");
            return false;
        }
        if(ParametersVM.PhaseParamName == string.Empty) {
            ErrorText = _localizationService.GetLocalizedString("MainWindow.PhaseParamNameEmpty");
            return false;
        }
        if(ParametersVM.FirstDispatcherGroupingLevelParamName == string.Empty) {
            ErrorText = _localizationService.GetLocalizedString("MainWindow.FirstDispatcherGroupingLevelParamNameEmpty");
            return false;
        }
        if(ParametersVM.SecondDispatcherGroupingLevelParamName == string.Empty) {
            ErrorText = _localizationService.GetLocalizedString("MainWindow.SecondDispatcherGroupingLevelParamNameEmpty");
            return false;
        }
        if(ParametersVM.ThirdDispatcherGroupingLevelParamName == string.Empty) {
            ErrorText = _localizationService.GetLocalizedString("MainWindow.ThirdDispatcherGroupingLevelParamNameEmpty");
            return false;
        }

        foreach(var task in TasksVM.TasksForWork) {
            if(task.Level is null) {
                ErrorText = _localizationService.GetLocalizedString("MainWindow.NotAllLevelsSelected");
                return false;
            }

            if(task.Phase is null) {
                ErrorText = _localizationService.GetLocalizedString("MainWindow.NotAllPhasesSelected");
                return false;
            }
        }
        ErrorText = string.Empty;
        return true;
    }


    private void SetSpecParams(ViewSchedule newViewSpec, TaskInfoViewModel task) {
        var dispatcherOption = new ParametersOption {
            FirstParamName = ParametersVM.FirstDispatcherGroupingLevelParamName,
            SecondParamName = ParametersVM.SecondDispatcherGroupingLevelParamName,
            ThirdParamName = ParametersVM.ThirdDispatcherGroupingLevelParamName,
            FourthParamName = ParametersVM.PhaseParamName,

            FirstParamValue = task.FirstDispatcherGroupingLevel,
            SecondParamValue = task.SecondDispatcherGroupingLevel,
            ThirdParamValue = task.ThirdDispatcherGroupingLevel,
            FourthParamValue = task.Phase.Id
        };

        _specificationService.SetSpecParams(newViewSpec, dispatcherOption);
    }


    private void ChangeSpecFilters(ViewSchedule spec, TaskInfoViewModel task) {
        _specificationService.ChangeSpecFilter(spec, ParametersVM.GroupTypeParamName, task.GroupType);
        _specificationService.ChangeSpecFilter(spec, ParametersVM.LevelParamName, task.Level.Id);
        _specificationService.ChangeSpecFilter(spec, ParametersVM.LevelShortNameParamName, task.LevelShortName);
    }


    private void LoadConfig() {
        RevitSettings setting = _pluginConfig.GetSettings(_revitRepository.Document);

        ParametersVM.GroupTypeParamName =
            setting?.GroupTypeParamName ??
                _defaultParamNameService.GetGroupType();
        ParametersVM.LevelParamName =
            setting?.LevelParamName ??
                _defaultParamNameService.GetLevel();
        ParametersVM.LevelShortNameParamName =
            setting?.LevelShortNameParamName ??
                _defaultParamNameService.GetLevelShortName();
        ParametersVM.PhaseParamName =
            setting?.PhaseParamName ??
                _defaultParamNameService.GetPhase();
        ParametersVM.FirstDispatcherGroupingLevelParamName =
            setting?.FirstDispatcherGroupingLevelParamName ??
                _defaultParamNameService.GetFirstDispatcherGroupingLevel();
        ParametersVM.SecondDispatcherGroupingLevelParamName =
            setting?.SecondDispatcherGroupingLevelParamName ??
                _defaultParamNameService.GetSecondDispatcherGroupingLevel();
        ParametersVM.ThirdDispatcherGroupingLevelParamName =
            setting?.ThirdDispatcherGroupingLevelParamName ??
                _defaultParamNameService.GetThirdDispatcherGroupingLevel();
    }

    private void SaveConfig() {
        RevitSettings setting = _pluginConfig.GetSettings(_revitRepository.Document)
            ?? _pluginConfig.AddSettings(_revitRepository.Document);

        setting.GroupTypeParamName = ParametersVM.GroupTypeParamName;
        setting.LevelParamName = ParametersVM.LevelParamName;
        setting.LevelShortNameParamName = ParametersVM.LevelShortNameParamName;
        setting.PhaseParamName = ParametersVM.PhaseParamName;
        setting.FirstDispatcherGroupingLevelParamName = ParametersVM.FirstDispatcherGroupingLevelParamName;
        setting.SecondDispatcherGroupingLevelParamName = ParametersVM.SecondDispatcherGroupingLevelParamName;
        setting.ThirdDispatcherGroupingLevelParamName = ParametersVM.ThirdDispatcherGroupingLevelParamName;
        _pluginConfig.SaveProjectConfig();
    }
}
