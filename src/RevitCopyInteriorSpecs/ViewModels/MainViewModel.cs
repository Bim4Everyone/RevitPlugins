// Ignore Spelling: plugin

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

using Autodesk.Revit.DB;

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

        private void LoadView() {
            LoadConfig();

            SelectedSpecs = _revitRepository.GetSelectedSpecs();
            Levels = _revitRepository.GetElements<Level>();
            Phases = _revitRepository.GetElements<Phase>();


        }

        private void AcceptView() {
            SaveConfig();

        }

        private bool CanAcceptView() {
            if(string.IsNullOrEmpty(SaveProperty)) {
                ErrorText = _localizationService.GetLocalizedString("MainWindow.HelloCheck");
                return false;
            }

            ErrorText = null;
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
