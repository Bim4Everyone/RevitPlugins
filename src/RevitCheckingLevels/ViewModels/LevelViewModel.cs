using dosymep.WPF.ViewModels;

using RevitCheckingLevels.Models;
using RevitCheckingLevels.Models.LevelParser;

namespace RevitCheckingLevels.ViewModels;
internal class LevelViewModel : BaseViewModel {
    private ErrorType _errorType;
    private string _toolTipInfo;

    public LevelViewModel(LevelInfo levelInfoInfo) {
        LevelInfo = levelInfoInfo;
    }

    public string Name => LevelInfo.Level.Name;
    public string MeterElevation => LevelInfo.Level.GetFormattedMeterElevation();
    public string MillimeterElevation => LevelInfo.Level.GetFormattedMillimeterElevation();

    public LevelInfo LevelInfo { get; }

    public ErrorType ErrorType {
        get => _errorType;
        set => RaiseAndSetIfChanged(ref _errorType, value);
    }

    public string ToolTipInfo {
        get => _toolTipInfo;
        set => RaiseAndSetIfChanged(ref _toolTipInfo, value);
    }
}