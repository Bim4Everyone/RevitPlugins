using dosymep.SimpleServices;
using dosymep.WPF.Commands;

namespace RevitPackageDocumentation.ViewModels.Configuration.Sheet.SheetComponents;
internal class ScheduleViewVM : SheetComponentVM {
    private readonly ILocalizationService _localizationService;

    private string _referenceViewName;
    private string _viewName;
    private string _viewColumn;
    private string _viewCount;

    public ScheduleViewVM(ILocalizationService localizationService) {
        _localizationService = localizationService;
        CreateComponentCommand = RelayCommand.Create(CreateComponent, ValidateModule);
    }

    public string ReferenceViewName {
        get => _referenceViewName;
        set => RaiseAndSetIfChanged(ref _referenceViewName, value);
    }

    public string ViewName {
        get => _viewName;
        set => RaiseAndSetIfChanged(ref _viewName, value);
    }

    public string ViewColumn {
        get => _viewColumn;
        set => RaiseAndSetIfChanged(ref _viewColumn, value);
    }

    public string ViewCount {
        get => _viewCount;
        set => RaiseAndSetIfChanged(ref _viewCount, value);
    }

    public override void CreateComponent() { }

    public override bool ValidateModule() {
        if(string.IsNullOrEmpty(ReferenceViewName)) {
            ModuleErrors = _localizationService.GetLocalizedString("MainWindow.ReferenceViewNameIsEmpty");
            return false;
        }
        if(string.IsNullOrEmpty(ViewName)) {
            ModuleErrors = _localizationService.GetLocalizedString("MainWindow.ViewNameIsEmpty");
            return false;
        }
        if(double.TryParse(ViewColumn, out double viewColumnAsDouble) || viewColumnAsDouble < 1) {
            ModuleErrors = _localizationService.GetLocalizedString("MainWindow.ViewColumnIsNotCorrect");
            return false;
        }
        if(double.TryParse(ViewCount, out double viewCountAsDouble) || viewCountAsDouble < 1) {
            ModuleErrors = _localizationService.GetLocalizedString("MainWindow.ViewCountIsNotCorrect");
            return false;
        }

        ModuleErrors = string.Empty;
        return true;
    }

    public override void Process() { }

    public void Create() { }
    public void Place() { }
}
