using System.Collections.ObjectModel;
using System.Windows.Input;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitPackageDocumentation.Models;
using RevitPackageDocumentation.ViewModels.Configuration.Sheet;

namespace RevitPackageDocumentation.ViewModels.Configuration.CustomParameters;
internal class CustomParametersListVM : BaseViewModel {
    private ObservableCollection<CustomParameterVM> _params = [];

    public CustomParametersListVM(BaseParamContainerVM baseParamContainerVM, StringParamSetService stringParamSetService) {
        BaseParamContainer = baseParamContainerVM;
        StrParamSetService = stringParamSetService;
        AddCustomParameterCommand = RelayCommand.Create(AddCustomParameter);
        RemoveCustomParameterCommand = RelayCommand.Create<CustomParameterVM>(RemoveCustomParameter);
    }

    public ICommand AddCustomParameterCommand { get; }
    public ICommand RemoveCustomParameterCommand { get; }

    public BaseParamContainerVM BaseParamContainer { get; }
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
