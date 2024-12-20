// Ignore Spelling: plugin

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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

        private ObservableCollection<TaskInfo> _tasksForWork = new ObservableCollection<TaskInfo>();
        private TaskInfo _selectedTask;

        private string _groupTypeParamName = "ФОП_Тип квартиры";
        private string _levelShortNameParamName = "ФОП_Этаж";
        private string _firstDispatcherGroupingLevelParamName = "_Стадия Проекта";
        private string _secondDispatcherGroupingLevelParamName = "_Группа Видов";
        private string _thirdDispatcherGroupingLevelParamName = "Назначение вида";

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
        }

        public ICommand LoadViewCommand { get; }
        public ICommand AcceptViewCommand { get; }

        public ICommand AddTaskCommand { get; }
        public ICommand DeleteTaskCommand { get; }

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

        public ObservableCollection<TaskInfo> TasksForWork {
            get => _tasksForWork;
            set => this.RaiseAndSetIfChanged(ref _tasksForWork, value);
        }

        public TaskInfo SelectedTask {
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



        private void LoadView() {
            LoadConfig();

            SelectedSpecs = _revitRepository.GetSelectedSpecs();
            Levels = _revitRepository.GetElements<Level>();
            Phases = _revitRepository.GetElements<Phase>();

            TasksForWork.Add(new TaskInfo());
        }

        private void AcceptView() {
            SaveConfig();

            using(Transaction transaction = _revitRepository.Document.StartTransaction("Копирование спецификаций АИ")) {

                foreach(TaskInfo task in TasksForWork) {
                    foreach(ViewSchedule spec in SelectedSpecs) {

                        string specOldName = spec.Name;

                        //string newViewSpecName = $"!АИ_О_Спецификация помещений_{task.Phase.Name}_{task.GroupType}_{task.LevelShortName}";
                        string newViewSpecName = $"{specOldName}_{task.Phase.Name}_{task.GroupType}_{task.LevelShortName}";

                        ViewSchedule newViewSpec = DuplicateSpec(spec, newViewSpecName);

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
        }



        private bool CanAcceptView() {

            if(TasksForWork.Count == 0) {
                ErrorText = "Не создано ни одной задачи";
                return false;
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
                throw new ArgumentException($"Спецификация с именем {newViewSpecName} уже существует!", "newViewSpecName");
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
            TasksForWork.Add(new TaskInfo());
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
    }
}
