using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using dosymep.WPF.ViewModels;

namespace RevitSplitMepCurve.ViewModels.Symbols;

internal class SelectableFamilySymbolViewModel : BaseViewModel {
    private FamilySymbolViewModel _selectedItem;

    public SelectableFamilySymbolViewModel(string label, ICollection<FamilySymbolViewModel> availableItems) {
        Label = label ?? throw new ArgumentNullException(nameof(label));
        AvailableItems = [.. availableItems.OrderBy(i => i.Name)];
    }

    public string Label { get; }

    public FamilySymbolViewModel SelectedItem {
        get => _selectedItem;
        set => RaiseAndSetIfChanged(ref _selectedItem, value);
    }

    public ObservableCollection<FamilySymbolViewModel> AvailableItems { get; }
}
