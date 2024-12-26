// Ignore Spelling: plugin

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

namespace RevitCopyInteriorSpecs.ViewModels {
    internal class MainViewModel : BaseViewModel {
        private readonly PluginConfig _pluginConfig;
        private readonly RevitRepository _revitRepository;
        private readonly ILocalizationService _localizationService;
        private readonly SpecificationService _specificationService;

        private string _errorText;
        private string _errorReport = string.Empty;

        public MainViewModel(
            PluginConfig pluginConfig,
            RevitRepository revitRepository,
            ILocalizationService localizationService) {

            _pluginConfig = pluginConfig;
            _revitRepository = revitRepository;
            _localizationService = localizationService;

            _specificationService = new SpecificationService(revitRepository, localizationService);

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
            set => this.RaiseAndSetIfChanged(ref _errorText, value);
        }


        private void LoadView() {
            LoadConfig();
            TasksVM.TasksForWork.Add(new TaskInfoVM());
        }


        private void AcceptView() {
            SaveConfig();
            var transactionName = _localizationService.GetLocalizedString("MainWindow.TransactionName");
            using(Transaction transaction = _revitRepository.Document.StartTransaction(transactionName)) {
                foreach(TaskInfoVM task in TasksVM.TasksForWork) {
                    foreach(ViewSchedule spec in SelectedSpecsVM.SelectedSpecs) {
                        string specOldName = spec.Name;
                        string newViewSpecName = $"{specOldName}_{task.Phase.Name}_{task.GroupType}_{task.LevelShortName}";

                        try {
                            ViewSchedule newViewSpec = _specificationService.DuplicateSpec(spec, newViewSpecName);
                            SetSpecParams(newViewSpec, task);
                            ChangeSpecFilters(newViewSpec, task);
                        } catch(ArgumentException e) {
                            _errorReport += $"- {e.Message};" + Environment.NewLine + Environment.NewLine;
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
            if(TasksVM.TasksForWork.Count == 0) {
                ErrorText = _localizationService.GetLocalizedString("MainWindow.NoTasksCreated");
                return false;
            }

            foreach(TaskInfoVM task in TasksVM.TasksForWork) {
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


        private void SetSpecParams(ViewSchedule newViewSpec, TaskInfoVM task) {
            var dispatcherOption = new DispatcherOption {
                FirstGroupingLevelParamName = ParametersVM.FirstDispatcherGroupingLevelParamName,
                SecondGroupingLevelParamName = ParametersVM.SecondDispatcherGroupingLevelParamName,
                ThirdGroupingLevelParamName = ParametersVM.ThirdDispatcherGroupingLevelParamName,

                FirstGroupingLevelParamValue = task.FirstDispatcherGroupingLevel,
                SecondGroupingLevelParamValue = task.SecondDispatcherGroupingLevel,
                ThirdGroupingLevelParamValue = task.ThirdDispatcherGroupingLevel
            };

            _specificationService.SetSpecParams(newViewSpec, dispatcherOption);
        }


        private void ChangeSpecFilters(ViewSchedule spec, TaskInfoVM task) {
            _specificationService.ChangeSpecFilters(spec, ParametersVM.GroupTypeParamName, task.GroupType);
            _specificationService.ChangeSpecFilters(spec, ParametersVM.LevelParamName, task.Level.Id);
            _specificationService.ChangeSpecFilters(spec, ParametersVM.LevelShortNameParamName, task.LevelShortName);
        }

        private void LoadConfig() {
            RevitSettings setting = _pluginConfig.GetSettings(_revitRepository.Document);

            ParametersVM.GroupTypeParamName =
                setting?.GroupTypeParamName ??
                    _localizationService.GetLocalizedString("MainWindow.GroupTypeParamName");
            ParametersVM.LevelParamName =
                setting?.LevelParamName ??
                    _localizationService.GetLocalizedString("MainWindow.LevelParamName");
            ParametersVM.LevelShortNameParamName =
                setting?.LevelShortNameParamName ??
                    _localizationService.GetLocalizedString("MainWindow.LevelShortNameParamName");
            ParametersVM.FirstDispatcherGroupingLevelParamName =
                setting?.FirstDispatcherGroupingLevelParamName ??
                    _localizationService.GetLocalizedString("MainWindow.FirstDispatcherGroupingLevelParamName");
            ParametersVM.SecondDispatcherGroupingLevelParamName =
                setting?.SecondDispatcherGroupingLevelParamName ??
                    _localizationService.GetLocalizedString("MainWindow.SecondDispatcherGroupingLevelParamName");
            ParametersVM.ThirdDispatcherGroupingLevelParamName =
                setting?.ThirdDispatcherGroupingLevelParamName ??
                    _localizationService.GetLocalizedString("MainWindow.ThirdDispatcherGroupingLevelParamName");
        }

        private void SaveConfig() {
            RevitSettings setting = _pluginConfig.GetSettings(_revitRepository.Document)
                ?? _pluginConfig.AddSettings(_revitRepository.Document);

            setting.GroupTypeParamName = ParametersVM.GroupTypeParamName;
            setting.LevelParamName = ParametersVM.LevelParamName;
            setting.LevelShortNameParamName = ParametersVM.LevelShortNameParamName;
            setting.FirstDispatcherGroupingLevelParamName = ParametersVM.FirstDispatcherGroupingLevelParamName;
            setting.SecondDispatcherGroupingLevelParamName = ParametersVM.SecondDispatcherGroupingLevelParamName;
            setting.ThirdDispatcherGroupingLevelParamName = ParametersVM.ThirdDispatcherGroupingLevelParamName;
            _pluginConfig.SaveProjectConfig();
        }
    }
}
