using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

using Autodesk.Revit.DB;

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
        private List<string> _paramNames = new List<string>();

        public SettingsPageVM(PluginConfig pluginConfig, RevitRepository revitRepository) {
            _pluginConfig = pluginConfig;
            _revitRepository = revitRepository;

            _eventUtils = new EventsUtils(this, _revitRepository);
            _eventUtils.SubscribeToEvents();

            GetParams();

            Tasks.Add(new TaskItemVM(ParamNames));

            LoadViewCommand = RelayCommand.Create(LoadView);
            SaveSettingsCommand = RelayCommand.Create(SaveSettings, CanSaveSettings);

            AddTaskCommand = RelayCommand.Create(AddTask);
            DeleteTaskCommand = RelayCommand.Create(DeleteTask);
            SelectAllCommand = RelayCommand.Create(SelectAll);
            UnselectAllCommand = RelayCommand.Create(UnselectAll);
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

        public List<string> ParamNames {
            get => _paramNames;
            set => this.RaiseAndSetIfChanged(ref _paramNames, value);
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

            Tasks.Add(new TaskItemVM(ParamNames));
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


        private void GetParams() {

            Document document = _revitRepository.Document;

            List<Category> neededCategories = new List<Category>() {

                Category.GetCategory(document, BuiltInCategory.OST_Walls),
                Category.GetCategory(document, BuiltInCategory.OST_Floors),
                Category.GetCategory(document, BuiltInCategory.OST_Columns),
                Category.GetCategory(document, BuiltInCategory.OST_StructuralColumns),
                Category.GetCategory(document, BuiltInCategory.OST_StructuralFoundation),
                Category.GetCategory(document, BuiltInCategory.OST_Rebar)
            };

            BindingMap bindingMap = _revitRepository.Document.ParameterBindings;
            DefinitionBindingMapIterator iterator = bindingMap.ForwardIterator();
            //iterator.Reset();
            while(iterator.MoveNext()) {

                var current = iterator.Current;

                if(current is InstanceBinding binding) {

                    foreach(Category category in neededCategories) {

                        if(binding.Categories.Contains(category)) {

                            ParamNames.Add(iterator.Key.Name);
                            break;
                        }
                    }
                }
            }

            ParamNames.Sort();
        }
    }
}
