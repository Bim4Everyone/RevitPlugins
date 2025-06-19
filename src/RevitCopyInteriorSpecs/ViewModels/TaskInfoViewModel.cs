using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

namespace RevitCopyInteriorSpecs.ViewModels;
internal class TaskInfoViewModel : BaseViewModel {
    private string _groupType = string.Empty;
    private Level _level;
    private string _levelShortName = string.Empty;
    private Phase _phase;
    private string _firstDispatcherGroupingLevel = string.Empty;
    private string _secondDispatcherGroupingLevel = string.Empty;
    private string _thirdDispatcherGroupingLevel = string.Empty;

    public TaskInfoViewModel() { }

    public string GroupType {
        get => _groupType;
        set => RaiseAndSetIfChanged(ref _groupType, value);
    }

    public Level Level {
        get => _level;
        set => RaiseAndSetIfChanged(ref _level, value);
    }

    public string LevelShortName {
        get => _levelShortName;
        set => RaiseAndSetIfChanged(ref _levelShortName, value);
    }

    public Phase Phase {
        get => _phase;
        set => RaiseAndSetIfChanged(ref _phase, value);
    }

    public string FirstDispatcherGroupingLevel {
        get => _firstDispatcherGroupingLevel;
        set => RaiseAndSetIfChanged(ref _firstDispatcherGroupingLevel, value);
    }

    public string SecondDispatcherGroupingLevel {
        get => _secondDispatcherGroupingLevel;
        set => RaiseAndSetIfChanged(ref _secondDispatcherGroupingLevel, value);
    }

    public string ThirdDispatcherGroupingLevel {
        get => _thirdDispatcherGroupingLevel;
        set => RaiseAndSetIfChanged(ref _thirdDispatcherGroupingLevel, value);
    }
}
