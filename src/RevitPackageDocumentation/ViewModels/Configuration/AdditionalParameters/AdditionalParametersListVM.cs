using System.Collections.ObjectModel;
using System.Windows.Input;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

namespace RevitPackageDocumentation.ViewModels.Configuration.AdditionalParameters;
internal class AdditionalParametersListVM : BaseViewModel {
    private ObservableCollection<AdditionalParameterVM> _params = [];

    public AdditionalParametersListVM() {
        AddAdditionalParameterCommand = RelayCommand.Create(AddAdditionalParameter);
        RemoveAdditionalParameterCommand = RelayCommand.Create<AdditionalParameterVM>(RemoveAdditionalParameter);
    }

    public ICommand AddAdditionalParameterCommand { get; }
    public ICommand RemoveAdditionalParameterCommand { get; }

    public ObservableCollection<AdditionalParameterVM> Params {
        get => _params;
        set => RaiseAndSetIfChanged(ref _params, value);
    }

    private void AddAdditionalParameter() {
        var param = new AdditionalParameterVM(this);
        Params.Add(param);
    }

    private void RemoveAdditionalParameter(AdditionalParameterVM param) {
        Params.Remove(param);
    }
}
