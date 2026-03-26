using Autodesk.Revit.DB;
using System.Collections.ObjectModel;

using dosymep.WPF.ViewModels;
using RevitMarkAllDocuments.Models;
using System.Linq;

namespace RevitMarkAllDocuments.ViewModels;

internal class SortPageViewModel : BaseViewModel {
    private ObservableCollection<ParameterViewModel> _selectableParams;
    private ObservableCollection<ParameterViewModel> _selectedParams;
    private ParameterViewModel _selectedParamFromSelectable;
    private ParameterViewModel _selectedParamFromSelected;


    public SortPageViewModel(RevitRepository revitRepository, Category category) {
        SelectableParams = [..revitRepository
            .GetSortableParams(category)
            .Select(x => new ParameterViewModel(x))];
        SelectedParams = [];
    }

    public ObservableCollection<ParameterViewModel> SelectableParams {
        get => _selectableParams;
        set => RaiseAndSetIfChanged(ref _selectableParams, value);
    }

    public ObservableCollection<ParameterViewModel> SelectedParams {
        get => _selectedParams;
        set => RaiseAndSetIfChanged(ref _selectedParams, value);
    }

    public ParameterViewModel SelectedParamFromSelectable {
        get => _selectedParamFromSelectable;
        set => RaiseAndSetIfChanged(ref _selectedParamFromSelectable, value);
    }
    public ParameterViewModel SelectedParamFromSelected {
        get => _selectedParamFromSelected;
        set => RaiseAndSetIfChanged(ref _selectedParamFromSelected, value);
    }
}
