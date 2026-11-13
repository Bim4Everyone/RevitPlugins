using dosymep.WPF.ViewModels;

using RevitVolumeModifier.Enums;

namespace RevitVolumeModifier.ViewModels;
internal class CommandStateViewModel : BaseViewModel {

    private CommandType _commandType;
    private CommandStatus _commandStatus;
    private string _commandText;

    public CommandType CommandType {
        get => _commandType;
        set => RaiseAndSetIfChanged(ref _commandType, value);
    }
    public CommandStatus CommandStatus {
        get => _commandStatus;
        set => RaiseAndSetIfChanged(ref _commandStatus, value);
    }
    public string CommandText {
        get => _commandText;
        set => RaiseAndSetIfChanged(ref _commandText, value);
    }
}
