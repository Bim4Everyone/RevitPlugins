using System.Windows.Input;

using dosymep.WPF.ViewModels;

namespace RevitPackageDocumentation.ViewModels.Configuration.Sheet.SheetComponents;
internal abstract class SheetComponentVM : BaseViewModel {
    private bool _isModuleCheck;
    private string _moduleName;
    private string _moduleComment;
    private string _moduleCode;
    private string _moduleErrors;
    private SheetVM _sheet;

    protected SheetComponentVM(SheetVM sheetVM) {
        Sheet = sheetVM;
    }

    public ICommand CreateComponentCommand { get; set; }

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

    public string ModuleErrors {
        get => _moduleErrors;
        set => RaiseAndSetIfChanged(ref _moduleErrors, value);
    }

    public SheetVM Sheet {
        get => _sheet;
        set => RaiseAndSetIfChanged(ref _sheet, value);
    }

    public T GetSettings<T>() where T : new() {
        var settings = new T();
        var vmType = GetType();
        var modelType = typeof(T);

        foreach(var prop in modelType.GetProperties()) {
            var vmProp = vmType.GetProperty(prop.Name);
            if(vmProp != null
               && vmProp.CanRead
               && prop.CanWrite
               && vmProp.PropertyType == prop.PropertyType) {
                var value = vmProp.GetValue(this);
                prop.SetValue(settings, value);
            }
        }
        return settings;
    }

    public abstract void CreateComponent();
    public abstract bool ValidateModule();
    public abstract void Process();
}
