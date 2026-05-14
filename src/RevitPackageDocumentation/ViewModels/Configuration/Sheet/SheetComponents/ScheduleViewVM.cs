namespace RevitPackageDocumentation.ViewModels.Configuration.Sheet.SheetComponents;
internal class ScheduleViewVM : SheetComponentVM {
    private string _referenceViewName;
    private string _viewName;
    private int _viewColumn;
    private int _viewCount;

    public string ReferenceViewName {
        get => _referenceViewName;
        set => RaiseAndSetIfChanged(ref _referenceViewName, value);
    }

    public string ViewName {
        get => _viewName;
        set => RaiseAndSetIfChanged(ref _viewName, value);
    }

    public int ViewColumn {
        get => _viewColumn;
        set => RaiseAndSetIfChanged(ref _viewColumn, value);
    }

    public int ViewCount {
        get => _viewCount;
        set => RaiseAndSetIfChanged(ref _viewCount, value);
    }

    public override void ValidateModule() { }
    public override void Process() { }

    public void Create() { }
    public void Place() { }
}
