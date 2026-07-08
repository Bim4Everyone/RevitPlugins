using System.Collections.ObjectModel;
using System.Windows.Input;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitPackageDocumentation.Models;
using RevitPackageDocumentation.ViewModels.Configuration.SheetSetParameters.Parameters;

namespace RevitPackageDocumentation.ViewModels.FiltrationComboBoxVMs;
internal class FiltrationComboBoxFilterListVM : BaseViewModel {
    private ObservableCollection<FiltrationComboBoxFilterVM> _valueList = [];

    public FiltrationComboBoxFilterListVM(
        ObservableCollection<PluginParamVM> sheetSetParams,
        StringParamSetService stringParamSetService) {
        SheetSetParams = sheetSetParams;
        StrParamSetService = stringParamSetService;
        AddFilterCommand = RelayCommand.Create(AddValue);
        RemoveFilterCommand = RelayCommand.Create<FiltrationComboBoxFilterVM>(RemoveValue);
    }

    public ICommand AddFilterCommand { get; }
    public ICommand RemoveFilterCommand { get; }

    public ObservableCollection<PluginParamVM> SheetSetParams { get; }
    public StringParamSetService StrParamSetService { get; }

    public ObservableCollection<FiltrationComboBoxFilterVM> ValueList {
        get => _valueList;
        set => RaiseAndSetIfChanged(ref _valueList, value);
    }

    private void AddValue() {
        var value = new FiltrationComboBoxFilterVM(this, StrParamSetService);
        ValueList.Add(value);
    }

    private void RemoveValue(FiltrationComboBoxFilterVM value) {
        ValueList.Remove(value);
    }
}
