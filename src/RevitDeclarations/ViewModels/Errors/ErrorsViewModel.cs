using System.Collections.Generic;
using System.Collections.ObjectModel;

using dosymep.WPF.ViewModels;

namespace RevitDeclarations.ViewModels;
internal class ErrorsViewModel : BaseViewModel {
    private ErrorsListViewModel _selectedList;

    public ErrorsViewModel(IEnumerable<ErrorsListViewModel> errors, bool isWarning) {
        ErrorLists = [.. errors];
        IsWarning = isWarning;
    }

    public ObservableCollection<ErrorsListViewModel> ErrorLists { get; }

    public ErrorsListViewModel SelectedList {
        get => _selectedList;
        set => RaiseAndSetIfChanged(ref _selectedList, value);
    }

    public bool IsWarning { get; set; }
}
