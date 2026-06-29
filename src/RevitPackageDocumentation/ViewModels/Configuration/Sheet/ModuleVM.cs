using System.Collections.ObjectModel;

using RevitPackageDocumentation.Models;
using RevitPackageDocumentation.ViewModels.Configuration.SheetSetParameters.Parameters;

namespace RevitPackageDocumentation.ViewModels.Configuration.Sheet;
internal abstract class ModuleVM : BaseParamContainerVM {
    private bool _isModuleCheck = false;
    private string _moduleName = string.Empty;
    private string _moduleComment = string.Empty;
    private string _moduleCode = string.Empty;
    private string _moduleTypeName = string.Empty;
    private string _moduleErrors = string.Empty;
    private bool _hasErrors = false;

    protected ModuleVM(
        RevitRepository repository,
        StringParamSetService stringParamSetService,
        ObservableCollection<PluginParamVM> sheetSetParams) : base(repository, stringParamSetService, sheetSetParams) {
    }

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

    public bool HasErrors {
        get => _hasErrors;
        set => RaiseAndSetIfChanged(ref _hasErrors, value);
    }

    public abstract void CreateComponent();
    public abstract bool Validate();
    public abstract void Process(bool processDependent = false);
}
