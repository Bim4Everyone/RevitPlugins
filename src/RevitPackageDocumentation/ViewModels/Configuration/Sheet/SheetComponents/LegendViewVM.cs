using Autodesk.Revit.DB;

namespace RevitPackageDocumentation.ViewModels.Configuration.Sheet.SheetComponents;
internal class LegendViewVM : SheetComponentVM {
    private View _legendView;

    public View LegendView {
        get => _legendView;
        set => RaiseAndSetIfChanged(ref _legendView, value);
    }

    public override void ValidateModule() { }
    public override void Process() { }

    public void Place() { }
}
