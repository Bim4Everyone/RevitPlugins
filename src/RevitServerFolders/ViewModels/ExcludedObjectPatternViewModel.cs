using System;

using dosymep.WPF.ViewModels;

using RevitServerFolders.Models;

namespace RevitServerFolders.ViewModels;

internal sealed class ExcludedObjectPatternViewModel : BaseViewModel, IEquatable<ExcludedObjectPatternViewModel> {

    private readonly ExcludedObjectPattern _excludedObjectPattern;

    public ExcludedObjectPatternViewModel(ExcludedObjectPattern excludedObjectPattern) {
        _excludedObjectPattern =
            excludedObjectPattern ?? throw new ArgumentNullException(nameof(excludedObjectPattern));

        Value = _excludedObjectPattern.Value;
    }

    public string Value { get; }

    public ExcludedObjectPattern GetSettings() {
        return _excludedObjectPattern;
    }

    public override string ToString() {
        return Value;
    }

    public bool Equals(ExcludedObjectPatternViewModel other) {
        if(other is null) {
            return false;
        }

        if(ReferenceEquals(this, other)) {
            return true;
        }

        return Equals(_excludedObjectPattern, other._excludedObjectPattern);
    }

    public override bool Equals(object obj) {
        return Equals(obj as ExcludedObjectPatternViewModel);
    }

    public override int GetHashCode() {
        return _excludedObjectPattern.GetHashCode();
    }
}
