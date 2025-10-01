using System;

using dosymep.SimpleServices;
using dosymep.WPF.ViewModels;

using RevitClashDetective.Models;

namespace RevitClashDetective.ViewModels.Settings;
internal class SectionBoxModeViewModel : BaseViewModel, IEquatable<SectionBoxModeViewModel> {
    private readonly ILocalizationService _localization;

    public SectionBoxModeViewModel(SectionBoxMode mode, ILocalizationService localization) {
        Mode = mode;
        _localization = localization ?? throw new ArgumentNullException(nameof(localization));

        Name = _localization.GetLocalizedString($"{nameof(SectionBoxMode)}.{mode}");
    }

    public SectionBoxMode Mode { get; }

    public string Name { get; }


    public bool Equals(SectionBoxModeViewModel other) {
        if(other is null) { return false; }
        return ReferenceEquals(this, other) || Mode == other.Mode;
    }

    public override int GetHashCode() {
        return 851357954 + Mode.GetHashCode();
    }

    public override bool Equals(object obj) {
        return Equals(obj as SectionBoxModeViewModel);
    }
}
