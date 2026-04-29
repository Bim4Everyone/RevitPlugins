using System;
using System.Collections.Generic;

using dosymep.SimpleServices;
using dosymep.WPF.ViewModels;

using RevitSplitMepCurve.Models.Enums;

namespace RevitSplitMepCurve.ViewModels;

internal class SelectionModeViewModel : BaseViewModel, IEquatable<SelectionModeViewModel> {
    private bool _isEnabled = true;

    public SelectionModeViewModel(ILocalizationService localization, SelectionMode mode) {
        if(localization == null) {
            throw new ArgumentNullException(nameof(localization));
        }

        Mode = mode;
        Name = localization.GetLocalizedString($"{nameof(SelectionMode)}.{mode}");
    }

    public string Name { get; }

    public SelectionMode Mode { get; }

    public bool IsEnabled {
        get => _isEnabled;
        set => RaiseAndSetIfChanged(ref _isEnabled, value);
    }

    public bool Equals(SelectionModeViewModel other) {
        return other is not null && Mode == other.Mode;
    }

    public override bool Equals(object obj) {
        return Equals(obj as SelectionModeViewModel);
    }

    public override int GetHashCode() {
        return EqualityComparer<SelectionMode>.Default.GetHashCode(Mode);
    }
}
