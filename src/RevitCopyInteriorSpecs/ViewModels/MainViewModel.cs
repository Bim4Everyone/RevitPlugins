// Ignore Spelling: plugin

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace RevitCopyInteriorSpecs.ViewModels {
    internal class MainViewModel : BaseViewModel {
        private readonly PluginConfig _pluginConfig;
        private readonly RevitRepository _revitRepository;
        private readonly ILocalizationService _localizationService;

        private string _errorText;
        private string _saveProperty;

        private List<ViewSchedule> _selectedSpecs;
        private List<Level> _levels;
        private List<Phase> _phases;

        private ObservableCollection<TaskInfoVM> _tasksForWork = new ObservableCollection<TaskInfoVM>();
        private TaskInfoVM _selectedTask;

        private string _groupTypeParamName = "ФОП_Тип квартиры";
        private string _levelShortNameParamName = "ФОП_Этаж";
        private string _firstDispatcherGroupingLevelParamName = "_Стадия Проекта";
        private string _secondDispatcherGroupingLevelParamName = "_Группа Видов";
        private string _thirdDispatcherGroupingLevelParamName = "Назначение вида";
        private string _generalGroupType;
        private Level _generalLevel;
        private string _generalLevelShortName;
        private Phase _generalPhase;
        private string _generalFirstDispatcherGroupingLevel;
        private string _generalSecondDispatcherGroupingLevel;
        private string _generalThirdDispatcherGroupingLevel;

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

            AddTaskCommand = RelayCommand.Create(AddTask);
            DeleteTaskCommand = RelayCommand.Create(DeleteTask, CanDeleteTask);

            GeneralGroupTypeChangedCommand = RelayCommand.Create(GeneralGroupTypeChanged);
            GeneralLevelChangedCommand = RelayCommand.Create(GeneralLevelChanged);
            GeneralLevelShortNameChangedCommand = RelayCommand.Create(GeneralLevelShortNameChanged);
            GeneralPhaseChangedCommand = RelayCommand.Create(GeneralPhaseChanged);
            GeneralFirstDispatcherGroupingLevelChangedCommand = RelayCommand.Create(GeneralFirstDispatcherGroupingLevelChanged);
            GeneralSecondDispatcherGroupingLevelChangedCommand = RelayCommand.Create(GeneralSecondDispatcherGroupingLevelChanged);
            GeneralThirdDispatcherGroupingLevelChangedCommand = RelayCommand.Create(GeneralThirdDispatcherGroupingLevelChanged);
        }


        public ICommand LoadViewCommand { get; }
        public ICommand AcceptViewCommand { get; }

        public ICommand AddTaskCommand { get; }
        public ICommand DeleteTaskCommand { get; }

        public ICommand GeneralGroupTypeChangedCommand { get; }
        public ICommand GeneralLevelChangedCommand { get; }
        public ICommand GeneralLevelShortNameChangedCommand { get; }
        public ICommand GeneralPhaseChangedCommand { get; }
        public ICommand GeneralFirstDispatcherGroupingLevelChangedCommand { get; }
        public ICommand GeneralSecondDispatcherGroupingLevelChangedCommand { get; }
        public ICommand GeneralThirdDispatcherGroupingLevelChangedCommand { get; }



        public string ErrorText {
            get => _errorText;
            set => this.RaiseAndSetIfChanged(ref _errorText, value);
        }

        public string SaveProperty {
            get => _saveProperty;
            set => this.RaiseAndSetIfChanged(ref _saveProperty, value);
        }

        public List<ViewSchedule> SelectedSpecs {
            get => _selectedSpecs;
            set => this.RaiseAndSetIfChanged(ref _selectedSpecs, value);
        }

        public List<Level> Levels {
            get => _levels;
            set => this.RaiseAndSetIfChanged(ref _levels, value);
        }

        public List<Phase> Phases {
            get => _phases;
            set => this.RaiseAndSetIfChanged(ref _phases, value);
        }

        public ObservableCollection<TaskInfoVM> TasksForWork {
            get => _tasksForWork;
            set => this.RaiseAndSetIfChanged(ref _tasksForWork, value);
        }

        public TaskInfoVM SelectedTask {
            get => _selectedTask;
            set => this.RaiseAndSetIfChanged(ref _selectedTask, value);
        }


        public string GroupTypeParamName {
            get => _groupTypeParamName;
            set => this.RaiseAndSetIfChanged(ref _groupTypeParamName, value);
        }

        public string LevelShortNameParamName {
            get => _levelShortNameParamName;
            set => this.RaiseAndSetIfChanged(ref _levelShortNameParamName, value);
        }

        public string FirstDispatcherGroupingLevelParamName {
            get => _firstDispatcherGroupingLevelParamName;
            set => this.RaiseAndSetIfChanged(ref _firstDispatcherGroupingLevelParamName, value);
        }

        public string SecondDispatcherGroupingLevelParamName {
            get => _secondDispatcherGroupingLevelParamName;
            set => this.RaiseAndSetIfChanged(ref _secondDispatcherGroupingLevelParamName, value);
        }

        public string ThirdDispatcherGroupingLevelParamName {
            get => _thirdDispatcherGroupingLevelParamName;
            set => this.RaiseAndSetIfChanged(ref _thirdDispatcherGroupingLevelParamName, value);
        }


        public string GeneralGroupType {
            get => _generalGroupType;
            set => this.RaiseAndSetIfChanged(ref _generalGroupType, value);
        }

        public Level GeneralLevel {
            get => _generalLevel;
            set => this.RaiseAndSetIfChanged(ref _generalLevel, value);
        }

        public string GeneralLevelShortName {
            get => _generalLevelShortName;
            set => this.RaiseAndSetIfChanged(ref _generalLevelShortName, value);
        }

        public Phase GeneralPhase {
            get => _generalPhase;
            set => this.RaiseAndSetIfChanged(ref _generalPhase, value);
        }

        public string GeneralFirstDispatcherGroupingLevel {
            get => _generalFirstDispatcherGroupingLevel;
            set => this.RaiseAndSetIfChanged(ref _generalFirstDispatcherGroupingLevel, value);
        }

        public string GeneralSecondDispatcherGroupingLevel {
            get => _generalSecondDispatcherGroupingLevel;
            set => this.RaiseAndSetIfChanged(ref _generalSecondDispatcherGroupingLevel, value);
        }

        public string GeneralThirdDispatcherGroupingLevel {
            get => _generalThirdDispatcherGroupingLevel;
            set => this.RaiseAndSetIfChanged(ref _generalThirdDispatcherGroupingLevel, value);
        }



        private void LoadView() {
            LoadConfig();

            SelectedSpecs = _revitRepository.GetSelectedSpecs();
            Levels = _revitRepository.GetElements<Level>();
            Phases = _revitRepository.GetElements<Phase>();

            TasksForWork.Add(new TaskInfoVM());
        }

        private void AcceptView() {
            SaveConfig();

            using(Transaction transaction = _revitRepository.Document.StartTransaction("Копирование спецификаций АИ")) {

                foreach(TaskInfoVM task in TasksForWork) {
                    foreach(ViewSchedule spec in SelectedSpecs) {
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
                            FirstGroupingLevelParamName = FirstDispatcherGroupingLevelParamName,
                            SecondGroupingLevelParamName = SecondDispatcherGroupingLevelParamName,
                            ThirdGroupingLevelParamName = ThirdDispatcherGroupingLevelParamName,

                            FirstGroupingLevelParamValue = task.FirstDispatcherGroupingLevel,
                            SecondGroupingLevelParamValue = task.SecondDispatcherGroupingLevel,
                            ThirdGroupingLevelParamValue = task.ThirdDispatcherGroupingLevel
                        };

                        SetSpecParams(newViewSpec, dispatcherOption);

                        ChangeSpecFilters(newViewSpec, GroupTypeParamName, task.GroupType);
                        ChangeSpecFilters(newViewSpec, "Уровень", task.Level.Id);
                        ChangeSpecFilters(newViewSpec, LevelShortNameParamName, task.LevelShortName);
                    }
                }

                transaction.Commit();
            }

            if(!string.IsNullOrEmpty(_errorReport)) {
                MessageBox.Show(_errorReport, "Отчет по ошибкам");
            }
        }


        private bool CanAcceptView() {

            if(TasksForWork.Count == 0) {
                ErrorText = _localizationService.GetLocalizedString("MainWindow.NoTasksCreated");
                return false;
            }

            foreach(TaskInfoVM task in TasksForWork) {
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



        /// <summary>
        /// Добавляет задачу в список. 
        /// Задача содержит информацию о начальном и конечном уровне, с которыми нужно работать; выбранную область видимости и спеки
        /// </summary>
        private void AddTask() {
            TasksForWork.Add(new TaskInfoVM());
        }

        /// <summary>
        /// Удаляет выбранную в интерфейсе задачу из списка. 
        /// </summary>
        private void DeleteTask() {
            TasksForWork.Remove(SelectedTask);
        }

        private bool CanDeleteTask() {
            if(SelectedTask is null) {
                return false;
            }
            return true;
        }


        private void GeneralGroupTypeChanged() {
            SetTasksPropValues("GroupType", GeneralGroupType);
        }

        private void GeneralLevelChanged() {
            SetTasksPropValues("Level", GeneralLevel);
        }

        private void GeneralLevelShortNameChanged() {
            SetTasksPropValues("LevelShortName", GeneralLevelShortName);
        }

        private void GeneralPhaseChanged() {
            SetTasksPropValues("Phase", GeneralPhase);
        }

        private void GeneralFirstDispatcherGroupingLevelChanged() {
            SetTasksPropValues("FirstDispatcherGroupingLevel", GeneralFirstDispatcherGroupingLevel);
        }

        private void GeneralSecondDispatcherGroupingLevelChanged() {
            SetTasksPropValues("SecondDispatcherGroupingLevel", GeneralSecondDispatcherGroupingLevel);
        }
        private void GeneralThirdDispatcherGroupingLevelChanged() {
            SetTasksPropValues("ThirdDispatcherGroupingLevel", GeneralThirdDispatcherGroupingLevel);
        }

        private void SetTasksPropValues(string propName, object propValue) {
            foreach(TaskInfoVM task in TasksForWork) {
                var prop = task.GetType().GetProperty(propName);
                prop.SetValue(task, propValue);
            }
        }
    }
}
