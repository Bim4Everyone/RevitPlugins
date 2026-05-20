using Autodesk.Revit.DB;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;

namespace RevitPackageDocumentation.ViewModels.Configuration.Sheet.SheetComponents;
internal class TextNoteVM : SheetComponentVM {
    private readonly ILocalizationService _localizationService;

    private string _text;
    private TextNoteType _textType;

    public TextNoteVM(SheetVM sheetVM, ILocalizationService localizationService) : base(sheetVM) {
        _localizationService = localizationService;
        CreateComponentCommand = RelayCommand.Create(CreateComponent, ValidateModule);
    }

    public string Text {
        get => _text;
        set => RaiseAndSetIfChanged(ref _text, value);
    }

    public TextNoteType TextNoteType {
        get => _textType;
        set => RaiseAndSetIfChanged(ref _textType, value);
    }

    public override void CreateComponent() { }

    public override bool ValidateModule() {
        if(string.IsNullOrEmpty(Text)) {
            ModuleErrors = _localizationService.GetLocalizedString("MainWindow.TextIsEmpty");
            return false;
        }
        if(TextNoteType is null) {
            ModuleErrors = _localizationService.GetLocalizedString("MainWindow.TextNoteTypeIsNull");
            return false;
        }

        ModuleErrors = string.Empty;
        return true;
    }

    public override void Process() { }

    public void Place() { }
}
