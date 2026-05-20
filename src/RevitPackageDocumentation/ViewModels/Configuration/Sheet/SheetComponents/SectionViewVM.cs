using Autodesk.Revit.DB;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;

namespace RevitPackageDocumentation.ViewModels.Configuration.Sheet.SheetComponents;
internal class SectionViewVM : SheetComponentVM {
    private readonly ILocalizationService _localizationService;

    private string _viewName;
    private ViewFamilyType _viewFamilyType;
    private ElementType _viewportType;
    private ViewSection _viewTemplate;
    private string _viewCount;

    public SectionViewVM(SheetVM sheetVM, ILocalizationService localizationService) : base(sheetVM) {
        _localizationService = localizationService;
        CreateComponentCommand = RelayCommand.Create(CreateComponent, ValidateModule);
    }

    public string ViewName {
        get => _viewName;
        set => RaiseAndSetIfChanged(ref _viewName, value);
    }

    public ViewFamilyType ViewFamilyType {
        get => _viewFamilyType;
        set => RaiseAndSetIfChanged(ref _viewFamilyType, value);
    }

    public ElementType ViewportType {
        get => _viewportType;
        set => RaiseAndSetIfChanged(ref _viewportType, value);
    }

    public ViewSection ViewTemplate {
        get => _viewTemplate;
        set => RaiseAndSetIfChanged(ref _viewTemplate, value);
    }

    public string ViewCount {
        get => _viewCount;
        set => RaiseAndSetIfChanged(ref _viewCount, value);
    }

    public override void CreateComponent() { }

    public override bool ValidateModule() {
        if(string.IsNullOrEmpty(ViewName)) {
            ModuleErrors = _localizationService.GetLocalizedString("MainWindow.ViewNameIsEmpty");
            return false;
        }
        if(ViewFamilyType is null) {
            ModuleErrors = _localizationService.GetLocalizedString("MainWindow.ViewFamilyTypeIsNull");
            return false;
        }
        if(ViewportType is null) {
            ModuleErrors = _localizationService.GetLocalizedString("MainWindow.ViewportTypeIsNull");
            return false;
        }
        if(ViewTemplate is null) {
            ModuleErrors = _localizationService.GetLocalizedString("MainWindow.ViewTemplateIsNull");
            return false;
        }
        if(!double.TryParse(ViewCount, out double viewCountAsDouble) || viewCountAsDouble < 1) {
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
