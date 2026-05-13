using System.Collections.ObjectModel;

using dosymep.WPF.ViewModels;

using RevitPackageDocumentation.ViewModels.Configuration.Sheet;

namespace RevitPackageDocumentation.ViewModels.Configuration;
internal class SheetSetVM : BaseViewModel {
    private ObservableCollection<SheetVM> _sheetList = [];
    private SheetSetPropsVM _properties;
    private string _name;

    public string Name {
        get => _name;
        set => RaiseAndSetIfChanged(ref _name, value);
    }

    public SheetSetPropsVM Properties {
        get => _properties;
        set => RaiseAndSetIfChanged(ref _properties, value);
    }

    public ObservableCollection<SheetVM> SheetList {
        get => _sheetList;
        set => RaiseAndSetIfChanged(ref _sheetList, value);
    }

    internal void AddSheet() {
        SheetList.Add(new SheetVM() { Name = "Новый лист" });
    }

    internal void RemoveSheet(SheetVM sheet) {
        if(sheet != null && SheetList.Contains(sheet)) {
            SheetList.Remove(sheet);
        }
    }
}
