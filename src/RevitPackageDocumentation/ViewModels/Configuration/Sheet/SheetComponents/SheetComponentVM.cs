using dosymep.WPF.ViewModels;

namespace RevitPackageDocumentation.ViewModels.Configuration.Sheet.SheetComponents;
internal abstract class SheetComponentVM : BaseViewModel {

    public bool IsModuleCheck { get; set; }
    public string ModuleName { get; set; }
    public string ModuleComment { get; set; }
    public string ModuleCode { get; set; }

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

    public virtual void ValidateModule() { }
    public virtual void Process() { }
}
