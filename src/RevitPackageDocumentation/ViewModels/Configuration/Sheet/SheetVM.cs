using System.Collections.ObjectModel;

using dosymep.WPF.ViewModels;

using RevitPackageDocumentation.ViewModels.Configuration.Sheet.SheetComponents;

namespace RevitPackageDocumentation.ViewModels.Configuration.Sheet;
internal class SheetVM : BaseViewModel {
    private string _name;
    private SheetPropsVM _properties;
    private ObservableCollection<SheetComponentVM> _sheetComponents = [];

    public string Name {
        get => _name;
        set => RaiseAndSetIfChanged(ref _name, value);
    }

    public SheetPropsVM Properties {
        get => _properties;
        set => RaiseAndSetIfChanged(ref _properties, value);
    }

    public ObservableCollection<SheetComponentVM> SheetComponents {
        get => _sheetComponents;
        set => RaiseAndSetIfChanged(ref _sheetComponents, value);
    }

    internal void AddComponent() {
        SheetComponents.Add(new PlanViewVM() { ModuleName = "Новый модуль" });
    }

    internal void RemoveComponent(SheetComponentVM sheetComponent) {
        if(sheetComponent != null && SheetComponents.Contains(sheetComponent)) {
            SheetComponents.Remove(sheetComponent);
        }
    }
}
