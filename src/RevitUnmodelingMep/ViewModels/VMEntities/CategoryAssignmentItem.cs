using System.Collections.ObjectModel;

using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

namespace RevitUnmodelingMep.ViewModels;

internal class CategoryAssignmentItem : BaseViewModel {
    private string _name;
    private ObservableCollection<SystemTypeItem> _systemTypes;

    public string Name {
        get => _name;
        set => RaiseAndSetIfChanged(ref _name, value);
    }

    public BuiltInCategory Category { get; set; }

    public ObservableCollection<SystemTypeItem> SystemTypes {
        get => _systemTypes;
        set => RaiseAndSetIfChanged(ref _systemTypes, value);
    }
}
