using dosymep.WPF.ViewModels;

namespace RevitCopyInteriorSpecs.ViewModels.MainViewModelParts;
internal class ParametersViewModel : BaseViewModel {
    private string _groupTypeParamName = string.Empty;
    private string _levelParamName = string.Empty;
    private string _levelShortNameParamName = string.Empty;
    private string _phaseParamName = string.Empty;
    private string _firstDispatcherGroupingLevelParamName = string.Empty;
    private string _secondDispatcherGroupingLevelParamName = string.Empty;
    private string _thirdDispatcherGroupingLevelParamName = string.Empty;

    public string GroupTypeParamName {
        get => _groupTypeParamName;
        set => RaiseAndSetIfChanged(ref _groupTypeParamName, value);
    }

    public string LevelParamName {
        get => _levelParamName;
        set => RaiseAndSetIfChanged(ref _levelParamName, value);
    }

    public string LevelShortNameParamName {
        get => _levelShortNameParamName;
        set => RaiseAndSetIfChanged(ref _levelShortNameParamName, value);
    }

    public string PhaseParamName {
        get => _phaseParamName;
        set => RaiseAndSetIfChanged(ref _phaseParamName, value);
    }

    public string FirstDispatcherGroupingLevelParamName {
        get => _firstDispatcherGroupingLevelParamName;
        set => RaiseAndSetIfChanged(ref _firstDispatcherGroupingLevelParamName, value);
    }

    public string SecondDispatcherGroupingLevelParamName {
        get => _secondDispatcherGroupingLevelParamName;
        set => RaiseAndSetIfChanged(ref _secondDispatcherGroupingLevelParamName, value);
    }

    public string ThirdDispatcherGroupingLevelParamName {
        get => _thirdDispatcherGroupingLevelParamName;
        set => RaiseAndSetIfChanged(ref _thirdDispatcherGroupingLevelParamName, value);
    }
}
