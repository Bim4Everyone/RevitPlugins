using System;

using dosymep.SimpleServices;
using dosymep.WPF.ViewModels;

using RevitSleeves.Models.Config;

namespace RevitSleeves.ViewModels.Settings;
internal class OffsetViewModel : BaseViewModel {
    private readonly Offset _offset;
    private readonly ILocalizationService _localizationService;
    private double _value;

    public OffsetViewModel(ILocalizationService localizationService, Offset offset) {
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
        _offset = offset ?? throw new ArgumentNullException(nameof(offset));

        Name = _localizationService.GetLocalizedString($"{nameof(OffsetType)}.{offset.OffsetType}");
        Value = _offset.Value;
        UnitName = _localizationService.GetLocalizedString("Mm");
    }

    public string Name { get; }

    public double Value {
        get => _value;
        set => RaiseAndSetIfChanged(ref _value, value);
    }

    public string UnitName { get; }

    public Offset GetOffset() {
        return new Offset() {
            Value = Value,
            OffsetType = _offset.OffsetType
        };
    }
}
