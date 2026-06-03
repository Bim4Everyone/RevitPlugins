using System.Linq;
using System.Windows.Input;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitPackageDocumentation.Models;
using RevitPackageDocumentation.ViewModels.Configuration.CustomParameters;
using RevitPackageDocumentation.ViewModels.Parameters;

namespace RevitPackageDocumentation.ViewModels.Configuration.Sheet.SheetComponents;
internal abstract class SheetComponentVM : BaseViewModel {
    private bool _isModuleCheck;
    private string _moduleName;
    private string _moduleComment;
    private string _moduleCode;
    private string _moduleTypeName;
    private string _moduleErrors;
    private CustomParametersListVM _customParamsList;
    private readonly SheetVM _sheet;

    protected SheetComponentVM(
        SheetVM sheetVM,
        RevitRepository repository,
        ILocalizationService localizationService,
        StringParamSetService stringParamSetService) {
        _sheet = sheetVM;
        Repository = repository;
        LocalizationService = localizationService;
        StrParamSetService = stringParamSetService;
        ModuleTypeName = LocalizationService.GetLocalizedString($"Type.{this.GetType().Name}");
        CustomParamsList = new CustomParametersListVM(this, StrParamSetService);

        PropUpdateByFormulaCommand = RelayCommand.Create<string>(PropUpdateByFormula);
    }

    public ICommand CreateComponentCommand { get; set; }
    public ICommand PropUpdateByFormulaCommand { get; }


    protected RevitRepository Repository { get; }

    protected ILocalizationService LocalizationService { get; }
    protected StringParamSetService StrParamSetService { get; }

    public SheetVM Sheet => _sheet;

    public bool IsModuleCheck {
        get => _isModuleCheck;
        set => RaiseAndSetIfChanged(ref _isModuleCheck, value);
    }

    public string ModuleName {
        get => _moduleName;
        set => RaiseAndSetIfChanged(ref _moduleName, value);
    }

    public string ModuleComment {
        get => _moduleComment;
        set => RaiseAndSetIfChanged(ref _moduleComment, value);
    }

    public string ModuleCode {
        get => _moduleCode;
        set => RaiseAndSetIfChanged(ref _moduleCode, value);
    }

    public string ModuleTypeName {
        get => _moduleTypeName;
        set => RaiseAndSetIfChanged(ref _moduleTypeName, value);
    }

    public string ModuleErrors {
        get => _moduleErrors;
        set => RaiseAndSetIfChanged(ref _moduleErrors, value);
    }

    public CustomParametersListVM CustomParamsList {
        get => _customParamsList;
        set => RaiseAndSetIfChanged(ref _customParamsList, value);
    }


    private void PropUpdateByFormula(string formulaPropertyName) {
        StrParamSetService.Set(this, formulaPropertyName, Sheet.SheetSet.Params);
    }

    public void UpdateDueParamValueChange(StringParamVM stringParam) {
        StrParamSetService.SetAll(this, Sheet.SheetSet.Params, stringParam);
        CustomParamsList.Params.ToList().ForEach(p => p.UpdateDueParamValueChange(stringParam));
    }

    public abstract void CreateComponent();
    public abstract bool ValidateModule();
    public abstract void Process();
}
