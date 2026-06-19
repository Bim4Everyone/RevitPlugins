using System.Collections.ObjectModel;
using System.Windows.Input;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

namespace RevitPackageDocumentation.ViewModels.FiltrationComboBoxVMs;
internal class FiltrationComboBoxFilterListVM : BaseViewModel {
    private ObservableCollection<FiltrationComboBoxFilterVM> _valueList = [];

    public FiltrationComboBoxFilterListVM() {
        AddFilterCommand = RelayCommand.Create(AddValue);
        RemoveFilterCommand = RelayCommand.Create<FiltrationComboBoxFilterVM>(RemoveValue);
    }

    public ICommand AddFilterCommand { get; }
    public ICommand RemoveFilterCommand { get; }

    public ObservableCollection<FiltrationComboBoxFilterVM> ValueList {
        get => _valueList;
        set => RaiseAndSetIfChanged(ref _valueList, value);
    }

    private void AddValue() {
        var value = new FiltrationComboBoxFilterVM();
        ValueList.Add(value);
    }

    private void RemoveValue(FiltrationComboBoxFilterVM value) {
        ValueList.Remove(value);
    }
}
