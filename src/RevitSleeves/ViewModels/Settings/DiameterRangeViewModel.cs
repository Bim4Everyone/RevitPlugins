using System;

using dosymep.SimpleServices;
using dosymep.WPF.ViewModels;

using RevitSleeves.Models.Config;

namespace RevitSleeves.ViewModels.Settings;
internal class DiameterRangeViewModel : BaseViewModel {
    private readonly ILocalizationService _localizationService;
    private readonly DiameterRange _diameterRange;
    private double _sleeveDiameter;
    private double _startMepSize;
    private double _endMepSize;

    public DiameterRangeViewModel(ILocalizationService localizationService, DiameterRange diameterRange) {
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
        _diameterRange = diameterRange ?? throw new ArgumentNullException(nameof(diameterRange));

        StartMepSize = _diameterRange.StartMepSize;
        EndMepSize = _diameterRange.EndMepSize;
        SleeveDiameter = _diameterRange.SleeveDiameter;
        From = _localizationService.GetLocalizedString("From");
        To = _localizationService.GetLocalizedString("To");
        UnitName = _localizationService.GetLocalizedString("Mm");
    }


    public string From { get; }

    public string To { get; }

    public string UnitName { get; }

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


    public DiameterRange GetDiameterRange() {
        return new DiameterRange() {
            StartMepSize = StartMepSize,
            EndMepSize = EndMepSize,
            SleeveDiameter = SleeveDiameter
        };
    }

    public string GetErrorText() {
        if(StartMepSize < 0 || EndMepSize < 0 || SleeveDiameter < 0) {
            return _localizationService.GetLocalizedString(
                "SleevePlacementSettings.Validation.DiameterRangesLessThanZero");
        }
        if(EndMepSize == 0) {
            return _localizationService.GetLocalizedString(
               "SleevePlacementSettings.Validation.EndMepSizeMustBeGreaterThanZero");
        }
        if(SleeveDiameter == 0) {
            return _localizationService.GetLocalizedString(
                "SleevePlacementSettings.Validation.SleeveDiameterMustBeGreaterThanZero");
        }
        if(StartMepSize >= EndMepSize) {
            return _localizationService.GetLocalizedString(
                "SleevePlacementSettings.Validation.StartMepSizeGreaterThanEnd");
        }
        return string.Empty;
    }

    public bool Overlap(DiameterRangeViewModel diameterRange) {
        return (StartMepSize < diameterRange.StartMepSize || StartMepSize < diameterRange.EndMepSize)
            && (diameterRange.EndMepSize < EndMepSize || diameterRange.StartMepSize < EndMepSize);
    }
}
