using System.Collections.ObjectModel;
using System.Windows.Input;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;

using RevitPackageDocumentation.Models;
using RevitPackageDocumentation.ViewModels.Configuration.SheetSetParameters.Parameters;
using RevitPackageDocumentation.ViewModels.Validation;
using RevitPackageDocumentation.ViewModels.Validation.Attributes;

namespace RevitPackageDocumentation.ViewModels.Configuration.CustomParameters;
internal class CustomParametersListVM : ValidatableVM {
    private readonly ILocalizationService _localizationService;
    private ObservableCollection<CustomParameterVM> _params = [];

    public CustomParametersListVM(
        ObservableCollection<PluginParamVM> sheetSetParams,
        StringParamSetService stringParamSetService,
        ILocalizationService localizationService)
        : base(localizationService) {

        SheetSetParams = sheetSetParams;
        StrParamSetService = stringParamSetService;
        _localizationService = localizationService;
        ValidateAllProperties();

        AddCustomParameterCommand = RelayCommand.Create(AddCustomParameter);
        RemoveCustomParameterCommand = RelayCommand.Create<CustomParameterVM>(RemoveCustomParameter);
    }

    public ICommand AddCustomParameterCommand { get; }
    public ICommand RemoveCustomParameterCommand { get; }

    public ObservableCollection<PluginParamVM> SheetSetParams { get; }
    public StringParamSetService StrParamSetService { get; }


    [ChildHasErrors]
    public ObservableCollection<CustomParameterVM> Params {
        get => _params;
        set => RaiseAndSetIfChanged(ref _params, value);
    }

    private void AddCustomParameter() {
        var param = new CustomParameterVM(this, StrParamSetService, _localizationService);
        Params.Add(param);
    }

    private void RemoveCustomParameter(CustomParameterVM param) {
        Params.Remove(param);
    }
}
