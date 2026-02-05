using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitCopyInteriorSpecs.Models;

namespace RevitCopyInteriorSpecs.ViewModels.MainViewModelParts;
internal class TasksViewModel : BaseViewModel {
    private readonly RevitRepository _revitRepository;

    private ObservableCollection<TaskInfoViewModel> _tasksForWork = [];
    private TaskInfoViewModel _selectedTask;

    private List<Level> _levels;
    private List<Phase> _phases;

    private string _generalGroupType;
    private Level _generalLevel;
    private string _generalLevelShortName;
    private Phase _generalPhase;
    private string _generalFirstDispatcherGroupingLevel;
    private string _generalSecondDispatcherGroupingLevel;
    private string _generalThirdDispatcherGroupingLevel;


    public TasksViewModel(RevitRepository revitRepository) {
        _revitRepository = revitRepository;

        Levels = _revitRepository.GetElements<Level>();
        Phases = _revitRepository.GetElements<Phase>();

        AddTask();

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


    public ICommand AddTaskCommand { get; }
    public ICommand DeleteTaskCommand { get; }

    public ICommand GeneralGroupTypeChangedCommand { get; }
    public ICommand GeneralLevelChangedCommand { get; }
    public ICommand GeneralLevelShortNameChangedCommand { get; }
    public ICommand GeneralPhaseChangedCommand { get; }
    public ICommand GeneralFirstDispatcherGroupingLevelChangedCommand { get; }
    public ICommand GeneralSecondDispatcherGroupingLevelChangedCommand { get; }
    public ICommand GeneralThirdDispatcherGroupingLevelChangedCommand { get; }


    public ObservableCollection<TaskInfoViewModel> TasksForWork {
        get => _tasksForWork;
        set => RaiseAndSetIfChanged(ref _tasksForWork, value);
    }

    public TaskInfoViewModel SelectedTask {
        get => _selectedTask;
        set => RaiseAndSetIfChanged(ref _selectedTask, value);
    }


    public List<Level> Levels {
        get => _levels;
        set => RaiseAndSetIfChanged(ref _levels, value);
    }

    public List<Phase> Phases {
        get => _phases;
        set => RaiseAndSetIfChanged(ref _phases, value);
    }


    public string GeneralGroupType {
        get => _generalGroupType;
        set => RaiseAndSetIfChanged(ref _generalGroupType, value);
    }

    public Level GeneralLevel {
        get => _generalLevel;
        set => RaiseAndSetIfChanged(ref _generalLevel, value);
    }

    public string GeneralLevelShortName {
        get => _generalLevelShortName;
        set => RaiseAndSetIfChanged(ref _generalLevelShortName, value);
    }

    public Phase GeneralPhase {
        get => _generalPhase;
        set => RaiseAndSetIfChanged(ref _generalPhase, value);
    }

    public string GeneralFirstDispatcherGroupingLevel {
        get => _generalFirstDispatcherGroupingLevel;
        set => RaiseAndSetIfChanged(ref _generalFirstDispatcherGroupingLevel, value);
    }

    public string GeneralSecondDispatcherGroupingLevel {
        get => _generalSecondDispatcherGroupingLevel;
        set => RaiseAndSetIfChanged(ref _generalSecondDispatcherGroupingLevel, value);
    }

    public string GeneralThirdDispatcherGroupingLevel {
        get => _generalThirdDispatcherGroupingLevel;
        set => RaiseAndSetIfChanged(ref _generalThirdDispatcherGroupingLevel, value);
    }


    /// <summary>
    /// Добавляет задачу в список. 
    /// Задача содержит информацию о начальном и конечном уровне, с которыми нужно работать; выбранную область видимости и спеки
    /// </summary>
    private void AddTask() {
        TasksForWork.Add(new TaskInfoViewModel());
    }

    /// <summary>
    /// Удаляет выбранную в интерфейсе задачу из списка. 
    /// </summary>
    private void DeleteTask() {
        TasksForWork.Remove(SelectedTask);
    }

    private bool CanDeleteTask() {
        return SelectedTask is not null;
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
        foreach(var task in TasksForWork) {
            var prop = task.GetType().GetProperty(propName);
            prop.SetValue(task, propValue);
        }
    }
}
