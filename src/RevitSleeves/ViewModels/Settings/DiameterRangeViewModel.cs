using System;

using dosymep.SimpleServices;
using dosymep.WPF.ViewModels;

using RevitSleeves.Models.Config;

namespace RevitSleeves.ViewModels.Settings;
internal class DiameterRangeViewModel : BaseViewModel {
    private readonly ILocalizationService _localizationService;
    private readonly DiameterRange _diameterRange;
    private double _sleeveDiameter;

    public DiameterRangeViewModel(ILocalizationService localizationService, DiameterRange diameterRange) {
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
        _diameterRange = diameterRange ?? throw new ArgumentNullException(nameof(diameterRange));

        StartMepSize = _diameterRange.StartMepSize;
        EndMepSize = _diameterRange.EndMepSize;
        SleeveDiameter = _diameterRange.SleeveDiameter;
        Name = $"{_localizationService.GetLocalizedString("From")} " +
            $"{StartMepSize} " +
            $"{_localizationService.GetLocalizedString("To")} " +
            $"{EndMepSize} {_localizationService.GetLocalizedString("Mm")}:";
        UnitName = _localizationService.GetLocalizedString("Mm");
    }

    public double StartMepSize { get; }

    public double EndMepSize { get; }

    public string Name { get; }

    public string UnitName { get; }

    public double SleeveDiameter {
        get => _sleeveDiameter;
        set => RaiseAndSetIfChanged(ref _sleeveDiameter, value);
    }
}
