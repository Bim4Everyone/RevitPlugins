using Autodesk.Revit.DB;

namespace RevitPackageDocumentation.ViewModels.Configuration.Sheet.SheetComponents;
internal class TextNoteVM : SheetComponentVM {
    private string _text;
    private TextNoteType _textType;

    public string Text {
        get => _text;
        set => RaiseAndSetIfChanged(ref _text, value);
    }

    public TextNoteType TextNoteType {
        get => _textType;
        set => RaiseAndSetIfChanged(ref _textType, value);
    }

    public override void ValidateModule() { }
    public override void Process() { }

    public void Place() { }
}
