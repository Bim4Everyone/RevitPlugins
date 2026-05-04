using Autodesk.Revit.DB;

namespace RevitPackageDocumentation.ViewModels.Configuration.Sheet.SheetComponents;
internal class CalloutViewVM : SheetComponentVM {

    public string ViewName { get; set; }
    public ViewType ViewFamilyType { get; set; }
    public ElementType ViewportType { get; set; }
    public ViewPlan ViewTemplate { get; set; }
    public int ViewCount { get; set; }
    public string ViewportNumber { get; set; }

    //public IViewBase ViewBase { get; set; }
    //public ModulTools Tools { get; set; }
    public string ModuleErrors { get; set; }


    public override void ValidateModule() { }
    public override void Process() { }

    public void Create() { }
    public void Place() { }
}
