using Autodesk.Revit.DB;

namespace RevitPackageDocumentation.ViewModels.Configuration.Sheet.SheetComponents;
internal class TextNoteVM : SheetComponentVM {

    public TextNoteType TextType { get; set; }

    public override void ValidateModule() { }
    public override void Process() { }

    public void Place() { }
}
