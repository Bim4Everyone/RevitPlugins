using System;

using dosymep.WPF.ViewModels;

namespace RevitParamsChecker.ViewModels.Utils;

internal class SelectableNameViewModel : BaseViewModel {
    private bool _isSelected;

    public SelectableNameViewModel(string name) {
        if(string.IsNullOrEmpty(name)) {
            throw new ArgumentNullException(nameof(name));
        }

        Name = name;
    }

    public string Name { get; }

    public bool IsSelected {
        get => _isSelected;
        set => RaiseAndSetIfChanged(ref _isSelected, value);
    }
}
