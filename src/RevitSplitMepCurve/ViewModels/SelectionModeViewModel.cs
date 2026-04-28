using System;
using System.Collections.Generic;

using dosymep.SimpleServices;
using dosymep.WPF.ViewModels;

using RevitSplitMepCurve.Models.Enums;

namespace RevitSplitMepCurve.ViewModels;

internal class SelectionModeViewModel : BaseViewModel, IEquatable<SelectionModeViewModel> {
    public SelectionModeViewModel(ILocalizationService localization, SelectionMode mode) {
        Mode = mode;
        Name = localization.GetLocalizedString($"SelectionMode.{mode}");
    }

    public string Name { get; }

    public SelectionMode Mode { get; }

    public bool Equals(SelectionModeViewModel other) => other is not null && Mode == other.Mode;

    public override bool Equals(object obj) => Equals(obj as SelectionModeViewModel);

    public override int GetHashCode() =>
        EqualityComparer<SelectionMode>.Default.GetHashCode(Mode);
}
