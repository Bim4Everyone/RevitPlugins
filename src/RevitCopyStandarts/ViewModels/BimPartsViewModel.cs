using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitCopyStandarts.Models;

namespace RevitCopyStandarts.ViewModels;

internal sealed class BimPartsViewModel : BaseViewModel {
    private string _searchText;
    private ObservableCollection<BimFileViewModel> _mainBimFiles;
    private ObservableCollection<BimFileViewModel> _filteredBimFiles;

    public BimPartsViewModel(string partName) {
        Name = partName;
        SearchCommand = RelayCommand.Create(ApplySearch);
    }

    public ICommand SearchCommand { get; }

    public string Name { get; }

    public string SearchText {
        get => _searchText;
        set => this.RaiseAndSetIfChanged(ref _searchText, value);
    }

    public ObservableCollection<BimFileViewModel> MainBimFiles {
        get => _mainBimFiles;
        set => this.RaiseAndSetIfChanged(ref _mainBimFiles, value);
    }

    public ObservableCollection<BimFileViewModel> FilteredBimFiles {
        get => _filteredBimFiles;
        set => this.RaiseAndSetIfChanged(ref _filteredBimFiles, value);
    }

    private void ApplySearch() {
        if(string.IsNullOrEmpty(SearchText)) {
            FilteredBimFiles = new ObservableCollection<BimFileViewModel>(MainBimFiles);
        } else {
            FilteredBimFiles = new ObservableCollection<BimFileViewModel>(
                MainBimFiles
                    .Where(item => item.Name.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0));
        }
    }
}
