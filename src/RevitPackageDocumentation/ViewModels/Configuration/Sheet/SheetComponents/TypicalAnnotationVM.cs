using Autodesk.Revit.DB;

namespace RevitPackageDocumentation.ViewModels.Configuration.Sheet.SheetComponents;
internal class TypicalAnnotationVM : SheetComponentVM {

    public Family AnnotationFamily { get; set; }
    public AnnotationSymbolType AnnotationType { get; set; }
    //public List<ParameterValue> ParamList { get; set; }
    public string ModuleErrors { get; set; }


    public override void ValidateModule() { }
    public override void Process() { }

    public void Place() { }
}
