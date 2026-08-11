using System;

using dosymep.WPF.ViewModels;

using RevitServerFolders.Models;

namespace RevitServerFolders.ViewModels;
internal sealed class ExcludedObjectPatternViewModel : BaseViewModel {
    private readonly ExcludedObjectPattern _excludedObjectPattern;

    public ExcludedObjectPatternViewModel(ExcludedObjectPattern excludedObjectPattern) {
        _excludedObjectPattern = excludedObjectPattern
            ?? throw new ArgumentNullException(nameof(excludedObjectPattern));
    }

    public Guid Id => _excludedObjectPattern.Id;

    public string Value => _excludedObjectPattern.Value;

    public ExcludedObjectPattern GetSettings() {
        return _excludedObjectPattern;
    }

    public override string ToString() {
        return Value;
    }
}
