using Autodesk.Revit.DB;

namespace RevitPackageDocumentation.ViewModels.Configuration.Sheet.SheetComponents;
internal class TypicalAnnotationVM : SheetComponentVM {
    private Family _annotationFamily;
    private AnnotationSymbolType _annotationType;

    public Family AnnotationFamily {
        get => _annotationFamily;
        set => RaiseAndSetIfChanged(ref _annotationFamily, value);
    }

    public AnnotationSymbolType AnnotationType {
        get => _annotationType;
        set => RaiseAndSetIfChanged(ref _annotationType, value);
    }

    public override void ValidateModule() { }
    public override void Process() { }

    public void Place() { }
}
