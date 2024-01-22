using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

using Autodesk.Revit.DB;
using Autodesk.Revit.Exceptions;
using Autodesk.Revit.UI.Selection;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitArchitecturalDocumentation.Models;
using RevitArchitecturalDocumentation.Models.Exceptions;

namespace RevitArchitecturalDocumentation.ViewModels.Components {
    internal class TaskInfoVM : BaseViewModel {
        private readonly PluginConfig _pluginConfig;
        private readonly RevitRepository _revitRepository;
        private readonly CreatingARDocsVM _creatingARDocsVM;

        private TaskInfo _selectedTask;
        private List<Element> _visibilityScopes;
        private ObservableCollection<TaskInfo> _tasksForWork = new ObservableCollection<TaskInfo>();


        public TaskInfoVM(PluginConfig pluginConfig, RevitRepository revitRepository, CreatingARDocsVM creatingARDocsVM) {
            _pluginConfig = pluginConfig;
            _revitRepository = revitRepository;
            _creatingARDocsVM = creatingARDocsVM;

            AddTaskCommand = RelayCommand.Create(AddTask);
            DeleteTaskCommand = RelayCommand.Create(DeleteTask);
            SelectSpecsCommand = new RelayCommand(SelectSpecs);

            VisibilityScopes = _revitRepository.VisibilityScopes;
            TasksForWork.Add(new TaskInfo(_revitRepository.RegexForBuildingPart, _revitRepository.RegexForBuildingSection, 1));
        }


        public ICommand AddTaskCommand { get; }
        public ICommand DeleteTaskCommand { get; }
        public ICommand SelectSpecsCommand { get; }


        public ObservableCollection<TaskInfo> TasksForWork {
            get => _tasksForWork;
            set => this.RaiseAndSetIfChanged(ref _tasksForWork, value);
        }

        public TaskInfo SelectedTask {
            get => _selectedTask;
            set => this.RaiseAndSetIfChanged(ref _selectedTask, value);
        }

        public List<Element> VisibilityScopes {
            get => _visibilityScopes;
            set => this.RaiseAndSetIfChanged(ref _visibilityScopes, value);
        }


        /// <summary>
        /// Добавляет задачу в список. 
        /// Задача содержит информацию о начальном и конечном уровне, с которыми нужно работать; выбранную область видимости и спеки
        /// </summary>
        private void AddTask() {

            TasksForWork.Add(new TaskInfo(_revitRepository.RegexForBuildingPart, _revitRepository.RegexForBuildingSection, TasksForWork.Count + 1));
        }

        /// <summary>
        /// Удаляет выбранную в интерфейсе задачу из списка. 
        /// </summary>
        private void DeleteTask() {

            if(TasksForWork.Count > 0) {
                TasksForWork.RemoveAt(TasksForWork.Count - 1);
            }
            _creatingARDocsVM.SpecOptsVM.GetFilterNames(TasksForWork);
        }


        /// <summary>
        /// После скрытия окна позволяет выбрать видовые экраны спек в Revit
        /// </summary>
        private void SelectSpecs(object obj) {

            _creatingARDocsVM.PCOnASPDocsView.Hide();

            TaskInfo task = obj as TaskInfo;
            if(task != null) {

                ISelectionFilter selectFilter = new ScheduleSelectionFilter();
                IList<Reference> references = new List<Reference>();
                try {
                    references = _revitRepository.ActiveUIDocument.Selection
                                .PickObjects(ObjectType.Element, selectFilter, "Выберите спецификации на листе");
                } catch(OperationCanceledException) {

                    _creatingARDocsVM.PCOnASPDocsView.ShowDialog();
                    return;
                }

                task.ListSpecHelpers.Clear();

                foreach(Reference reference in references) {

                    ScheduleSheetInstance scheduleSheetInstance = _revitRepository.Document.GetElement(reference) as ScheduleSheetInstance;
                    if(scheduleSheetInstance is null) {
                        continue;
                    }

                    SpecHelper specHelper = new SpecHelper(_revitRepository, scheduleSheetInstance);
                    task.ListSpecHelpers.Add(specHelper);
                    try {
                        specHelper.NameHelper.AnalyzeNGetNameInfo();

                    } catch(ViewNameException ex) {
                        _creatingARDocsVM.ErrorText = ex.Message;
                    }

                }
                _creatingARDocsVM.SpecOptsVM.GetFilterNames(TasksForWork);
            }
            _creatingARDocsVM.PCOnASPDocsView.ShowDialog();
        }
    }
}
