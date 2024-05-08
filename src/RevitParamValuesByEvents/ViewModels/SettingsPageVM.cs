using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitParamValuesByEvents.Models;

namespace RevitParamValuesByEvents.ViewModels {
    internal class SettingsPageVM : BaseViewModel {

        private readonly PluginConfig _pluginConfig;
        private readonly RevitRepository _revitRepository;
        private readonly EventsUtils _eventUtils;
        private ObservableCollection<TaskItemVM> _tasks = new ObservableCollection<TaskItemVM>();

        private string _saveProperty;

        public SettingsPageVM(PluginConfig pluginConfig, RevitRepository revitRepository) {
            _pluginConfig = pluginConfig;
            _revitRepository = revitRepository;

            _eventUtils = new EventsUtils(this, _revitRepository);
            _eventUtils.SubscribeToEvents();


            LoadViewCommand = RelayCommand.Create(LoadView);
            SaveSettingsCommand = RelayCommand.Create(SaveSettings, CanSaveSettings);

            AddTaskCommand = RelayCommand.Create(AddTask);
            DeleteTaskCommand = RelayCommand.Create(DeleteTask);
            SelectAllCommand = RelayCommand.Create(SelectAll);
            UnselectAllCommand = RelayCommand.Create(UnselectAll);

            Tasks.Add(new TaskItemVM(true, "Комментарии", "111"));
            Tasks.Add(new TaskItemVM(false, "Марка", "222"));
        }

        public ICommand LoadViewCommand { get; }
        public ICommand SaveSettingsCommand { get; }
        public ICommand AddTaskCommand { get; }
        public ICommand DeleteTaskCommand { get; }
        public ICommand SelectAllCommand { get; }
        public ICommand UnselectAllCommand { get; }


        public string SaveProperty {
            get => _saveProperty;
            set => this.RaiseAndSetIfChanged(ref _saveProperty, value);
        }


        public ObservableCollection<TaskItemVM> Tasks {
            get => _tasks;
            set => this.RaiseAndSetIfChanged(ref _tasks, value);
        }


        private void SaveSettings() {

            SaveConfig();
        }


        private bool CanSaveSettings() {

            return true;
        }


        private void LoadView() {
            LoadConfig();
        }


        private void AddTask() {

            Tasks.Add(new TaskItemVM(true, "", ""));
        }


        private void DeleteTask() {

            List<TaskItemVM> tasksForDel = new List<TaskItemVM>();
            foreach(TaskItemVM task in Tasks) {

                if(task.IsCheck == true) {

                    tasksForDel.Add(task);
                }
            }

            foreach(TaskItemVM task in tasksForDel) {

                Tasks.Remove(task);
            }
        }


        private void SelectAll() {

            foreach(TaskItemVM task in Tasks) {

                task.IsCheck = true;
            }
        }

        private void UnselectAll() {

            foreach(TaskItemVM task in Tasks) {

                task.IsCheck = false;
            }
        }


        private void LoadConfig() {
            RevitSettings setting = _pluginConfig.GetSettings(_revitRepository.Document);

            SaveProperty = setting?.SaveProperty ?? "Привет Revit!";
        }

        private void SaveConfig() {
            RevitSettings setting = _pluginConfig.GetSettings(_revitRepository.Document)
                                    ?? _pluginConfig.AddSettings(_revitRepository.Document);

            setting.SaveProperty = SaveProperty;
            _pluginConfig.SaveProjectConfig();
        }
    }
}
