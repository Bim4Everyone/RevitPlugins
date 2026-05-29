using Autodesk.Revit.DB;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;

using RevitPackageDocumentation.Models;

namespace RevitPackageDocumentation.ViewModels.Configuration.Sheet.SheetComponents;
internal class TextNoteVM : SheetComponentVM {
    private string _text;
    private TextNoteType _textType;

    public TextNoteVM(SheetVM sheetVM, RevitRepository repository, ILocalizationService localizationService) 
        : base(sheetVM, repository, localizationService) {
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
            ModuleErrors = LocalizationService.GetLocalizedString("MainWindow.TextIsEmpty");
            return false;
        }
        if(TextNoteType is null) {
            ModuleErrors = LocalizationService.GetLocalizedString("MainWindow.TextNoteTypeIsNull");
            return false;
        }

        ModuleErrors = string.Empty;
        return true;
    }

    public override void Process() {
        Place();
    }

    public void Place() {
        var sheetInstance = Sheet.SheetInstance;
        var options = new TextNoteOptions(TextNoteType.Id);
        var position = new XYZ(
            UnitUtilsHelper.ConvertToInternalValue(-190),
            UnitUtilsHelper.ConvertToInternalValue(120),
            0);
        TextNote.Create(Repository.Document, sheetInstance.Id, position, Text, options);
    }
}
