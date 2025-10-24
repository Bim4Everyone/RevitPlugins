using System;

using dosymep.WPF.ViewModels;

using RevitClashDetective.Models;

namespace RevitClashDetective.ViewModels.ClashDetective;
internal class SelectableNameViewModel : BaseViewModel, IName {
    private bool _isSelected;

    public SelectableNameViewModel(IName namedEntity) {
        Entity = namedEntity ?? throw new ArgumentNullException(nameof(namedEntity));
    }

    public IName Entity { get; }

    public string Name => Entity.Name;

    public bool IsSelected {
        get => _isSelected;
        set => RaiseAndSetIfChanged(ref _isSelected, value);
    }
}
