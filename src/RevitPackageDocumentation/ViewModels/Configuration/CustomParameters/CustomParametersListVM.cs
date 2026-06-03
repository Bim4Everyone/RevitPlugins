using System.Collections.ObjectModel;
using System.Windows.Input;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitPackageDocumentation.Models;
using RevitPackageDocumentation.ViewModels.Configuration.Sheet.SheetComponents;

namespace RevitPackageDocumentation.ViewModels.Configuration.CustomParameters;
internal class CustomParametersListVM : BaseViewModel {
    private ObservableCollection<CustomParameterVM> _params = [];

    public CustomParametersListVM(SheetComponentVM sheetComponentVM, StringParamSetService stringParamSetService) {
        SheetComponent = sheetComponentVM;
        StrParamSetService = stringParamSetService;
        AddCustomParameterCommand = RelayCommand.Create(AddCustomParameter);
        RemoveCustomParameterCommand = RelayCommand.Create<CustomParameterVM>(RemoveCustomParameter);
    }

    public ICommand AddCustomParameterCommand { get; }
    public ICommand RemoveCustomParameterCommand { get; }

    public SheetComponentVM SheetComponent { get; }
    public StringParamSetService StrParamSetService { get; }

    public ObservableCollection<CustomParameterVM> Params {
        get => _params;
        set => RaiseAndSetIfChanged(ref _params, value);
    }

    private void AddCustomParameter() {
        var param = new CustomParameterVM(this, StrParamSetService);
        Params.Add(param);
    }

    private void RemoveCustomParameter(CustomParameterVM param) {
        Params.Remove(param);
    }
}
