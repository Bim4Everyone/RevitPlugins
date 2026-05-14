namespace RevitPackageDocumentation.ViewModels.Configuration.Sheet.SheetComponents;
internal class LegendViewVM : SheetComponentVM {
    private string _viewName;

    public string ViewName {
        get => _viewName;
        set => RaiseAndSetIfChanged(ref _viewName, value);
    }

    public override void ValidateModule() { }
    public override void Process() { }

    public void Place() { }
}
