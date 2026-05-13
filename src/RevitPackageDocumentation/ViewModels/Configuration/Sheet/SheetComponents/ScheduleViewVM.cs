namespace RevitPackageDocumentation.ViewModels.Configuration.Sheet.SheetComponents;
internal class ScheduleViewVM : SheetComponentVM {

    public string ReferenceViewName { get; set; }
    public string ViewName { get; set; }
    public int ViewRow { get; set; }
    public int ViewCount { get; set; }
    //public List<Filter> Filters { get; set; }
    //public IViewBase ViewBase { get; set; }
    //public ModulTools Tools { get; set; }

    public override void ValidateModule() { }
    public override void Process() { }

    public void Create() { }
    public void Place() { }
}
