using System;

using dosymep.WPF.ViewModels;

using RevitSleeves.Models.Config;

namespace RevitSleeves.ViewModels.Settings;
internal class DiameterRangeViewModel : BaseViewModel {
    private readonly DiameterRange _diameterRange;
    private double _startMepSize;
    private double _endMepSize;
    private double _sleeveDiameter;

    public DiameterRangeViewModel(DiameterRange diameterRange) {
        _diameterRange = diameterRange ?? throw new ArgumentNullException(nameof(diameterRange));
    }

    public double StartMepSize {
        get => _startMepSize;
        set => RaiseAndSetIfChanged(ref _startMepSize, value);
    }

    public double EndMepSize {
        get => _endMepSize;
        set => RaiseAndSetIfChanged(ref _endMepSize, value);
    }

    public double SleeveDiameter {
        get => _sleeveDiameter;
        set => RaiseAndSetIfChanged(ref _sleeveDiameter, value);
    }
}
