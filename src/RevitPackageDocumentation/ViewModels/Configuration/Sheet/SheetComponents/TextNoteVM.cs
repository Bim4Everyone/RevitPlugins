using System.Collections.ObjectModel;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;

using RevitPackageDocumentation.Models;
using RevitPackageDocumentation.ViewModels.Configuration.SheetSetParameters.Parameters;
using RevitPackageDocumentation.ViewModels.FiltrationComboBoxVMs;

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
        CreateComponentCommand = RelayCommand.Create(CreateComponent, Validate);
    }

    public string TextFormula {
        get => _textFormula;
        set => RaiseAndSetIfChanged(ref _textFormula, value);
    }

    public string Text {
        get => _text;
        set => RaiseAndSetIfChanged(ref _text, value);
    }

    public TextNoteType TextNoteType {
        get => _textType;
        set => RaiseAndSetIfChanged(ref _textType, value);
    }

    public FiltrationComboBoxFilterListVM TextNoteTypeFilter {
        get => _textNoteTypeFilter;
        set => RaiseAndSetIfChanged(ref _textNoteTypeFilter, value);
    }

    public string TextWidth {
        get => _textWidth;
        set => RaiseAndSetIfChanged(ref _textWidth, value);
    }

    public override bool Validate() {
        if(string.IsNullOrEmpty(TextFormula)) {
            ModuleErrors = LocalizationService.GetLocalizedString("MainWindow.TextIsEmpty");
            return false;
        }
        if(TextNoteType is null) {
            ModuleErrors = LocalizationService.GetLocalizedString("MainWindow.TextNoteTypeIsNull");
            return false;
        }
        if(!int.TryParse(TextWidth, out int textWidthAsInt) || textWidthAsInt < 1) {
            ModuleErrors = LocalizationService.GetLocalizedString("MainWindow.TextWidthIsNotCorrect");
            return false;
        }
        foreach(var param in CustomParamsList.Params) {
            if(string.IsNullOrEmpty(param.ParamName)) {
                ModuleErrors = LocalizationService.GetLocalizedString("MainWindow.CustomParamsIsNotCorrect");
                return false;
            }
        }

        ModuleErrors = string.Empty;
        return true;
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
            UnitUtilsHelper.ConvertToInternalValue(120),
            0);
        var textNoteInstance = TextNote.Create(Repository.Document, sheetInstance.Id, position, Text, options);

        int textWidthAsInt = int.Parse(TextWidth);
        textNoteInstance.Width = UnitUtilsHelper.ConvertToInternalValue(textWidthAsInt);
        return textNoteInstance;
    }
}
