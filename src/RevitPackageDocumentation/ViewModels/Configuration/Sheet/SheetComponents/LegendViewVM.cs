using Autodesk.Revit.DB;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;

namespace RevitPackageDocumentation.ViewModels.Configuration.Sheet.SheetComponents;
internal class LegendViewVM : SheetComponentVM {
    private readonly ILocalizationService _localizationService;

    private View _legendView;

    public LegendViewVM(SheetVM sheetVM, ILocalizationService localizationService) : base(sheetVM) {
        _localizationService = localizationService;
        CreateComponentCommand = RelayCommand.Create(CreateComponent, ValidateModule);
    }

    public View LegendView {
        get => _legendView;
        set => RaiseAndSetIfChanged(ref _legendView, value);
    }

    public override void CreateComponent() { }

    public override bool ValidateModule() {
        if(LegendView is null) {
            ModuleErrors = _localizationService.GetLocalizedString("MainWindow.LegendViewIsNull");
            return false;
        }

        ModuleErrors = string.Empty;
        return true;
    }

    public override void Process() { }

    public void Place() { }
}
