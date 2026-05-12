using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;

using dosymep.WPF.ViewModels;

namespace RevitSplitMepCurve.ViewModels.Symbols;

internal class SelectableFamilySymbolViewModel : BaseViewModel {
    private FamilySymbolViewModel _selectedItem;
    private string _searchText;

    public SelectableFamilySymbolViewModel(string label, ICollection<FamilySymbolViewModel> availableItems) {
        Label = label ?? throw new ArgumentNullException(nameof(label));
        AvailableItems = [.. availableItems.OrderBy(i => i.Name)];
        FilteredItems = new CollectionViewSource() { Source = AvailableItems };
        FilteredItems.Filter += SymbolsFilterHandler;
        PropertyChanged += OnPropertyChanged;
    }

    public string Label { get; }

    public FamilySymbolViewModel SelectedItem {
        get => _selectedItem;
        set => RaiseAndSetIfChanged(ref _selectedItem, value);
    }

    public string SearchText {
        get => _searchText;
        set => RaiseAndSetIfChanged(ref _searchText, value);
    }

    public ObservableCollection<FamilySymbolViewModel> AvailableItems { get; }

    public CollectionViewSource FilteredItems { get; }

    private void SymbolsFilterHandler(object sender, FilterEventArgs e) {
        if(e.Item is FamilySymbolViewModel symbol) {
            if(!string.IsNullOrWhiteSpace(SearchText)) {
                e.Accepted = symbol.Name.IndexOf(SearchText, StringComparison.CurrentCultureIgnoreCase) >= 0;
                return;
            }

            e.Accepted = true;
        }
    }

    private void OnPropertyChanged(object sender, PropertyChangedEventArgs e) {
        if(e.PropertyName == nameof(SearchText)) {
            FilteredItems?.View.Refresh();
        }
    }
}
