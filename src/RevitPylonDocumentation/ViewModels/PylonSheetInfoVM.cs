using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

namespace RevitPylonDocumentation.ViewModels;
internal class PylonSheetInfoVM : BaseViewModel {
    private bool _isCheck = false;

    internal PylonSheetInfoVM(string projectSection, string pylonKeyName) {
        PylonKeyName = pylonKeyName;
        ProjectSection = projectSection;
    }

    public bool IsCheck {
        get => _isCheck;
        set => RaiseAndSetIfChanged(ref _isCheck, value);
    }

    public bool SheetInProject { get; set; } = false;

    // Марка пилона 
    public string PylonKeyName { get; set; }
    public string ProjectSection { get; set; }

    public List<Element> HostElems { get; set; } = [];
    public ViewSheet PylonViewSheet { get; set; }
}
