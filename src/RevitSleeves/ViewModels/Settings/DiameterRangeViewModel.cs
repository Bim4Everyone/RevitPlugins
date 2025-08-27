using System;

using dosymep.SimpleServices;
using dosymep.WPF.ViewModels;

using RevitSleeves.Models.Config;

namespace RevitSleeves.ViewModels.Settings;
internal class DiameterRangeViewModel : BaseViewModel {
    private readonly ILocalizationService _localizationService;
    private readonly DiameterRange _diameterRange;
    private double _sleeveDiameter;
    private double _sleeveThickness;
    private double _startMepSize;
    private double _endMepSize;

    public DiameterRangeViewModel(ILocalizationService localizationService, DiameterRange diameterRange) {
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
        _diameterRange = diameterRange ?? throw new ArgumentNullException(nameof(diameterRange));

        StartMepSize = _diameterRange.StartMepSize;
        EndMepSize = _diameterRange.EndMepSize;
        SleeveDiameter = _diameterRange.SleeveDiameter;
        SleeveThickness = _diameterRange.SleeveThickness;
        From = _localizationService.GetLocalizedString("From");
        To = _localizationService.GetLocalizedString("To");
        UnitName = _localizationService.GetLocalizedString("Mm");
        ToolTipStartPipeDiameter = _localizationService.GetLocalizedString(
            "SleevePlacementSettings.ToolTip.StartPipeDiameter");
        ToolTipEndPipeDiameter = _localizationService.GetLocalizedString(
            "SleevePlacementSettings.ToolTip.EndPipeDiameter");
        ToolTipSleeveDiameter = _localizationService.GetLocalizedString(
            "SleevePlacementSettings.ToolTip.SleeveDiameter");
        ToolTipSleeveThickness = _localizationService.GetLocalizedString(
            "SleevePlacementSettings.ToolTip.SleeveThickness");
    }


    public string From { get; }

    public string To { get; }

    public string UnitName { get; }

    public string ToolTipStartPipeDiameter { get; }

    public string ToolTipEndPipeDiameter { get; }

    public string ToolTipSleeveDiameter { get; }

    public string ToolTipSleeveThickness { get; }

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

    public double SleeveThickness {
        get => _sleeveThickness;
        set => RaiseAndSetIfChanged(ref _sleeveThickness, value);
    }


    public DiameterRange GetDiameterRange() {
        return new DiameterRange() {
            StartMepSize = StartMepSize,
            EndMepSize = EndMepSize,
            SleeveDiameter = SleeveDiameter,
            SleeveThickness = SleeveThickness
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
        if(SleeveThickness <= 0) {
            return _localizationService.GetLocalizedString(
                "SleevePlacementSettings.Validation.ThicknessMustBeGreaterThanZero");
        }
        return string.Empty;
    }

    public bool Overlap(DiameterRangeViewModel diameterRange) {
        return (StartMepSize < diameterRange.StartMepSize || StartMepSize < diameterRange.EndMepSize)
            && (diameterRange.EndMepSize < EndMepSize || diameterRange.StartMepSize < EndMepSize);
    }
}
