using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

using RevitPylonDocumentation.Models;
using RevitPylonDocumentation.Models.PylonSheetNView;
using RevitPylonDocumentation.Models.Services;

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

    // Марка пилона 
    public string PylonKeyName { get; set; }
    public string ProjectSection { get; set; }

    public bool SheetInProject { get; set; } = false;
    public List<Element> HostElems { get; set; } = [];
    public ViewSheet PylonViewSheet { get; set; }

    public PylonSheetInfo GetPylonSheetInfo(CreationSettings settings, RevitRepository repository,
                                            ParamValueService paramValService, RebarFinderService rebarFinder) {
        return new PylonSheetInfo() {
            Settings = settings,
            Repository = repository,
            ParamValService = paramValService,
            RebarFinder = rebarFinder,

            PylonKeyName = PylonKeyName,
            ProjectSection = ProjectSection,
            SheetInProject = SheetInProject,
            HostElems = HostElems,
            PylonViewSheet = PylonViewSheet
        };
    }
}
