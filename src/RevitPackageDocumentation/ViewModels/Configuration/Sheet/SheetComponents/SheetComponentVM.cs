using System.Linq;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.Revit;
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

    public void SetCustomParams(Element element) {
        if(element is null) {
            return;
        }
        foreach(var param in CustomParamsList.Params) {
            // Если параметр существует на экземпляре и он редактируемый
            if(element.IsExistsParam(param.ParamName) && !element.GetParam(param.ParamName).IsReadOnly) {
                element.SetParamValue(param.ParamName, param.ParamValue);
                continue;
            }
            // Если параметр существует на типе и он редактируемый
            var type = Repository.Document.GetElement(element.GetTypeId());
            if(type != null && type.IsExistsParam(param.ParamName) && !element.GetParam(param.ParamName).IsReadOnly) {
                type.SetParamValue(param.ParamName, param.ParamValue);
            }
        }
    }

    /// <summary>
    /// Получает следующий номер видового экрана на листе
    /// </summary>
    protected int GetLastViewportNumber(int startNumber) {
        var viewports = Sheet.SheetInstance.GetAllViewports()
            .Select(id => Repository.Document.GetElement(id) as Viewport)
            .ToList();

        int lastViewportNumber = startNumber;
        foreach(var viewport in viewports) {
            string viewportNumberAsStr = viewport.GetParamValue<string>(BuiltInParameter.VIEWPORT_DETAIL_NUMBER);
            // Если не число, то не влияет, т.к. плагин будет ставить число
            if(int.TryParse(viewportNumberAsStr, out int viewportNumberAsInt)) {
                if(viewportNumberAsInt > lastViewportNumber) {
                    lastViewportNumber = viewportNumberAsInt;
                }
            }
        }
        return lastViewportNumber;
    }

    public abstract void CreateComponent();
    public abstract bool ValidateModule();
    public abstract void Process();
}
