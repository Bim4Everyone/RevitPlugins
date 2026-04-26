using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitMarkAllDocuments.Models;

namespace RevitMarkAllDocuments.ViewModels;

internal class SortPageViewModel : BaseViewModel {
    private ObservableCollection<ParameterViewModel> _selectableParams;
    private ObservableCollection<ParameterViewModel> _selectedParams;
    private ParameterViewModel _selectedParamFromSelectable;
    private ParameterViewModel _selectedParamFromSelected;


    public SortPageViewModel(IList<FilterableParam> parameters) {
        SelectableParams = [..parameters
            .Select(x => new ParameterViewModel(x))
            .OrderBy(x => x.Name)];

        SelectedParams = [];

        AddParamCommand = RelayCommand.Create(AddParam, CanAddParam);
        RemoveParamCommand = RelayCommand.Create(RemoveParam, CanRemoveParam);
        MoveUpParamCommand = RelayCommand.Create(MoveUpParam, CanRemoveParam);
        MoveDownParamCommand = RelayCommand.Create(MoveDownParam, CanRemoveParam);
    }

    public ICommand AddParamCommand { get; }
    public ICommand RemoveParamCommand { get; }
    public ICommand MoveUpParamCommand { get; }
    public ICommand MoveDownParamCommand { get; }

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
        SelectableParams = [.. SelectableParams.OrderBy(x => x.Name)];
    }

    public void MoveUpParam() {
        int index = SelectedParams.IndexOf(SelectedParamFromSelected);

        if(index <= 0) {
            return;
        }

        SelectedParams.Move(index, index - 1);
    }

    public void MoveDownParam() {
        int index = SelectedParams.IndexOf(SelectedParamFromSelected);

        if(index < 0 || index >= SelectedParams.Count - 1) {
            return;
        }

        SelectedParams.Move(index, index + 1);
    }

    private bool CanAddParam() {
        return SelectedParamFromSelectable != null;
    }

    private bool CanRemoveParam() {
        return SelectedParamFromSelected != null;
    }

    private void MoveParams(ObservableCollection<ParameterViewModel> fromParams,
                            ObservableCollection<ParameterViewModel> toParams,
                            ParameterViewModel param) {
        fromParams.Remove(param);
        toParams.Add(param);
    }
}
