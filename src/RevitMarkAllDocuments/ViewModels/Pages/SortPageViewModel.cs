using Autodesk.Revit.DB;
using System.Collections.ObjectModel;

using dosymep.WPF.ViewModels;
using RevitMarkAllDocuments.Models;
using System.Linq;
using System.Windows.Input;
using dosymep.WPF.Commands;
using dosymep.Bim4Everyone;

namespace RevitMarkAllDocuments.ViewModels;

internal class SortPageViewModel : BaseViewModel {
    private ObservableCollection<ParameterViewModel> _selectableParams;
    private ObservableCollection<ParameterViewModel> _selectedParams;
    private ParameterViewModel _selectedParamFromSelectable;
    private ParameterViewModel _selectedParamFromSelected;


    public SortPageViewModel(RevitRepository revitRepository, Category category) {
        SelectableParams = [..revitRepository
            .GetSortableParams(category)
            .Select(x => new ParameterViewModel(x))
            .OrderBy(x => x.Name)];
        SelectedParams = [];

        AddParamCommand = RelayCommand.Create(AddParam, CanAddParam);
        RemoveParamCommand = RelayCommand.Create(RemoveParam, CanRemoveParam);
    }

    public ICommand AddParamCommand { get; }
    public ICommand RemoveParamCommand { get; }

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

    public void AddParam() {
        MoveParams(SelectableParams, SelectedParams, SelectedParamFromSelectable);
    }

    public void RemoveParam() {
        MoveParams(SelectedParams, SelectableParams, SelectedParamFromSelected);
    }

    private bool CanAddParam() {
        return SelectedParamFromSelectable != null;
    }

    private bool CanRemoveParam() {
        return SelectedParamFromSelected != null;
    }

    public void MoveParams(ObservableCollection<ParameterViewModel> fromParams,
                           ObservableCollection<ParameterViewModel> toParams,
                           ParameterViewModel param) {
        fromParams.Remove(param);
        toParams.Add(param);

        toParams.Clear();
        var sorted = toParams.OrderBy(x => x.Name).ToList();

        foreach(var item in sorted) {
            toParams.Add(item);
        }
    }
}
