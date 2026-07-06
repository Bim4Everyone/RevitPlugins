using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;

using RevitPackageDocumentation.Models;
using RevitPackageDocumentation.ViewModels.Configuration.SheetSetParameters.Parameters;
using RevitPackageDocumentation.ViewModels.FiltrationComboBoxVMs;
using RevitPackageDocumentation.ViewModels.Validation.Attributes;

namespace RevitPackageDocumentation.ViewModels.Configuration.Sheet.SheetComponents;
internal class TextNoteVM : SheetComponentVM {
    private string _textFormula = string.Empty;
    private string _text;
    private TextNoteType _textType;

    private FiltrationComboBoxFilterListVM _textNoteTypeFilter;
    private string _textWidth;

    public TextNoteVM(
        RevitRepository repository,
        StringParamSetService stringParamSetService,
        ObservableCollection<PluginParamVM> sheetSetParams,
        SheetVM sheetVM,
        ILocalizationService localizationService)
        : base(repository, stringParamSetService, sheetSetParams, sheetVM, localizationService) {
        ValidateAllProperties();
        CreateComponentCommand = RelayCommand.Create(CreateComponent, CanCreateComponent);
    }

    [Required(ErrorMessage = "Validation.TextNoteTypeIsNull")]
    public TextNoteType TextNoteType {
        get => _textType;
        set => RaiseAndSetIfChanged(ref _textType, value);
    }

    public FiltrationComboBoxFilterListVM TextNoteTypeFilter {
        get => _textNoteTypeFilter;
        set => RaiseAndSetIfChanged(ref _textNoteTypeFilter, value);
    }

    [PositiveInteger(ErrorMessage = "Validation.TextWidthIsNotCorrect")]
    public string TextWidth {
        get => _textWidth;
        set => RaiseAndSetIfChanged(ref _textWidth, value);
    }

    [Required(ErrorMessage = "Validation.TextIsEmpty")]
    public string TextFormula {
        get => _textFormula;
        set => RaiseAndSetIfChanged(ref _textFormula, value);
    }

    public string Text {
        get => _text;
        set => RaiseAndSetIfChanged(ref _text, value);
    }


    public override void Process(bool processDependent = false) {
        var textNote = Place();
        SetCustomParams(textNote);
    }

    public TextNote Place() {
        var sheetInstance = Sheet.SheetInstance;

        // Если текстовое примечание с таким текстом уже существует на листе, то новую не ставим
        if(Repository.GetTextNotes(sheetInstance)
            .FirstOrDefault(t => t.Text.Replace("\r\n", "\n").Replace("\r", "\n").Trim()
                == Text.Replace("\r\n", "\n").Replace("\r", "\n").Trim()) is TextNote textNote) {
            return textNote;
        }

        var options = new TextNoteOptions(TextNoteType.Id);
        var position = new XYZ(
            UnitUtilsHelper.ConvertToInternalValue(-190),
            UnitUtilsHelper.ConvertToInternalValue(170),
            0);
        var textNoteInstance = TextNote.Create(Repository.Document, sheetInstance.Id, position, Text, options);

        int textWidthAsInt = int.Parse(TextWidth);
        textNoteInstance.Width = UnitUtilsHelper.ConvertToInternalValue(textWidthAsInt);
        return textNoteInstance;
    }
}
