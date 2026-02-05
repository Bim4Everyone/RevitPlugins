using System.Windows.Input;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

namespace RevitDeclarations.ViewModels;
internal class FilterRoomValueVM : BaseViewModel {
    private readonly ParametersViewModel _paramViewModel;

    public FilterRoomValueVM(ParametersViewModel paramViewModel, string value) {
        _paramViewModel = paramViewModel;
        Value = value;

        RemoveFilterCommand = new RelayCommand(RemoveFilter);
    }

    public ICommand RemoveFilterCommand { get; }

    public string Value { get; }

    public void RemoveFilter(object o) {
        _paramViewModel.RemoveFilter(this);
    }
}
