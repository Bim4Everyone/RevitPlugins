using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

namespace RevitBatchPrint.ViewModels;

internal sealed class AlbumViewModel : BaseViewModel {
    private readonly ILocalizationService _localizationService;

    private bool? _isSelected;
    private ObservableCollection<SheetViewModel> _mainSheets;
    private ObservableCollection<SheetViewModel> _filteredSheets;

    public AlbumViewModel(
        string albumName,
        ILocalizationService localizationService) {
        _localizationService = localizationService;

        IsSelected = false;

        Name = string.IsNullOrEmpty(albumName)
            ? localizationService.GetLocalizedString("MainWindow.EmptyAlbumNameParam")
            : albumName;

        CheckCommand = RelayCommand.Create(Check);
        CheckUpdateCommand = RelayCommand.Create(CheckUpdate);
    }

    public ICommand CheckCommand { get; }
    public ICommand CheckUpdateCommand { get; }

    public string Name { get; }

    public bool? IsSelected {
        get => _isSelected;
        set => this.RaiseAndSetIfChanged(ref _isSelected, value);
    }

    public ObservableCollection<SheetViewModel> MainSheets {
        get => _mainSheets;
        set => this.RaiseAndSetIfChanged(ref _mainSheets, value);
    }

    public ObservableCollection<SheetViewModel> FilteredSheets {
        get => _filteredSheets;
        set => this.RaiseAndSetIfChanged(ref _filteredSheets, value);
    }

    public void FilterSheets(string searchText) {
        if(string.IsNullOrWhiteSpace(searchText)) {
            FilteredSheets = new ObservableCollection<SheetViewModel>(MainSheets);
        } else {
            FilteredSheets = new ObservableCollection<SheetViewModel>(
                MainSheets.Where(item => item.Name.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0));
        }
    }

    public bool HasSelectedSheets() {
        return MainSheets.Any(item => item.IsSelected);
    }

    private void Check() {
        if(IsSelected == null) {
            return;
        }

        foreach(SheetViewModel sheetViewModel in MainSheets) {
            sheetViewModel.IsSelected = IsSelected.Value;
        }
    }

    private void CheckUpdate() {
        int countSelected = MainSheets.Count(item => item.IsSelected);
        if(MainSheets.Count != countSelected) {
            IsSelected = null;
        } else if(MainSheets.Count == countSelected) {
            IsSelected = true;
        }
    }
}
