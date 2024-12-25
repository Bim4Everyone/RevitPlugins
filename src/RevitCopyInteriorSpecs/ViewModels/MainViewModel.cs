// Ignore Spelling: plugin

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Revit;
using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitCopyInteriorSpecs.Models;
using RevitCopyInteriorSpecs.ViewModels.MainViewModelParts;

namespace RevitCopyInteriorSpecs.ViewModels {
    internal class MainViewModel : BaseViewModel {
        private readonly PluginConfig _pluginConfig;
        private readonly RevitRepository _revitRepository;
        private readonly ILocalizationService _localizationService;

        private string _errorText;
        private string _saveProperty;

        private string _errorReport = string.Empty;


        public MainViewModel(
            PluginConfig pluginConfig,
            RevitRepository revitRepository,
            ILocalizationService localizationService) {

            _pluginConfig = pluginConfig;
            _revitRepository = revitRepository;
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
            set => this.RaiseAndSetIfChanged(ref _errorText, value);
        }

        public string SaveProperty {
            get => _saveProperty;
            set => this.RaiseAndSetIfChanged(ref _saveProperty, value);
        }


        private void LoadView() {
            LoadConfig();

            TasksVM.TasksForWork.Add(new TaskInfoVM());
        }


        private void AcceptView() {
            SaveConfig();

            using(Transaction transaction = _revitRepository.Document.StartTransaction("Копирование спецификаций АИ")) {

                foreach(TaskInfoVM task in TasksVM.TasksForWork) {
                    foreach(ViewSchedule spec in SelectedSpecsVM.SelectedSpecs) {
                        string specOldName = spec.Name;

                        string newViewSpecName = $"{specOldName}_{task.Phase.Name}_{task.GroupType}_{task.LevelShortName}";
                        ViewSchedule newViewSpec;
                        try {
                            newViewSpec = DuplicateSpec(spec, newViewSpecName);
                        } catch(ArgumentException e) {
                            _errorReport += $"- {e.Message};" + Environment.NewLine + Environment.NewLine;
                            continue;
                        }

                        DispatcherOption dispatcherOption = new DispatcherOption() {
                            FirstGroupingLevelParamName = ParametersVM.FirstDispatcherGroupingLevelParamName,
                            SecondGroupingLevelParamName = ParametersVM.SecondDispatcherGroupingLevelParamName,
                            ThirdGroupingLevelParamName = ParametersVM.ThirdDispatcherGroupingLevelParamName,

                            FirstGroupingLevelParamValue = task.FirstDispatcherGroupingLevel,
                            SecondGroupingLevelParamValue = task.SecondDispatcherGroupingLevel,
                            ThirdGroupingLevelParamValue = task.ThirdDispatcherGroupingLevel
                        };

                        SetSpecParams(newViewSpec, dispatcherOption);

                        ChangeSpecFilters(newViewSpec, ParametersVM.GroupTypeParamName, task.GroupType);
                        ChangeSpecFilters(newViewSpec, "Уровень", task.Level.Id);
                        ChangeSpecFilters(newViewSpec, ParametersVM.LevelShortNameParamName, task.LevelShortName);
                    }
                }

                transaction.Commit();
            }

            if(!string.IsNullOrEmpty(_errorReport)) {
                MessageBox.Show(_errorReport, "Отчет по ошибкам");
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


        private void LoadConfig() {
            RevitSettings setting = _pluginConfig.GetSettings(_revitRepository.Document);
            SaveProperty = setting?.SaveProperty ?? _localizationService.GetLocalizedString("MainWindow.Hello");
        }

        private void SaveConfig() {
            RevitSettings setting = _pluginConfig.GetSettings(_revitRepository.Document)
                ?? _pluginConfig.AddSettings(_revitRepository.Document);

            setting.SaveProperty = SaveProperty;
            _pluginConfig.SaveProjectConfig();
        }


        /// <summary>
        /// Дублирует спецификацию, и задает указанное имя.
        /// Если спецификация с таким именем уже существует, то выбрасывает исключение.
        /// </summary>
        private ViewSchedule DuplicateSpec(ViewSchedule viewSchedule, string newViewSpecName) {
            ViewSchedule newViewSpec = _revitRepository.GetSpecByName(newViewSpecName);
            // Если спеку с указанным именем не нашли, то будем создавать дублированием
            if(newViewSpec is null) {
                newViewSpec = _revitRepository.Document.GetElement(viewSchedule.Duplicate(ViewDuplicateOption.Duplicate)) as ViewSchedule;
                newViewSpec.Name = newViewSpecName;
            } else {
                throw new ArgumentException($"Спецификация с именем \"{newViewSpecName}\" уже существует!");
            }

            return newViewSpec;
        }

        private void SetSpecParams(ViewSchedule newViewSpec, DispatcherOption dispatcherOption) {
            newViewSpec.SetParamValue(dispatcherOption.FirstGroupingLevelParamName, dispatcherOption.FirstGroupingLevelParamValue);
            newViewSpec.SetParamValue(dispatcherOption.SecondGroupingLevelParamName, dispatcherOption.SecondGroupingLevelParamValue);
            newViewSpec.SetParamValue(dispatcherOption.ThirdGroupingLevelParamName, dispatcherOption.ThirdGroupingLevelParamValue);
        }


        /// <summary>
        /// Метод по изменению фильтра спецификации с указанным именем на указанное значение с учетом формата предыдущего значения
        /// </summary>
        public void ChangeSpecFilters(ViewSchedule spec, string specFilterName, string newFilterValue) {
            ScheduleDefinition specificationDefinition = spec.Definition;
            List<ScheduleFilter> specificationFilters = specificationDefinition.GetFilters().ToList();

            List<ScheduleFilter> newScheduleFilters = new List<ScheduleFilter>();

            // Перебираем фильтры и записываем каждый, изменяя только тот, что ищем
            // потому что механизм изменения значения конкретного фильтра работал нестабильно
            for(int i = 0; i < specificationFilters.Count; i++) {
                ScheduleFilter currentFilter = specificationFilters[i];
                ScheduleField scheduleFieldFromFilter = specificationDefinition.GetField(currentFilter.FieldId);

                if(scheduleFieldFromFilter.GetName() == specFilterName) {
                    currentFilter.SetValue(newFilterValue);
                    newScheduleFilters.Add(currentFilter);
                } else {
                    newScheduleFilters.Add(currentFilter);
                }
            }

            specificationDefinition.SetFilters(newScheduleFilters);
        }


        public void ChangeSpecFilters(ViewSchedule spec, string specFilterName, ElementId newFilterValue) {
            ScheduleDefinition specificationDefinition = spec.Definition;
            List<ScheduleFilter> specificationFilters = specificationDefinition.GetFilters().ToList();


            List<ScheduleFilter> newScheduleFilters = new List<ScheduleFilter>();

            // Перебираем фильтры и записываем каждый, изменяя только тот, что ищем потому что механизм изменения значения конкретного фильтра работал нестабильно
            for(int i = 0; i < specificationFilters.Count; i++) {

                ScheduleFilter currentFilter = specificationFilters[i];
                ScheduleField scheduleFieldFromFilter = specificationDefinition.GetField(currentFilter.FieldId);

                if(scheduleFieldFromFilter.GetName() == specFilterName) {
                    currentFilter.SetValue(newFilterValue);
                    newScheduleFilters.Add(currentFilter);
                } else {
                    newScheduleFilters.Add(currentFilter);
                }
            }

            specificationDefinition.SetFilters(newScheduleFilters);
        }
    }
}
