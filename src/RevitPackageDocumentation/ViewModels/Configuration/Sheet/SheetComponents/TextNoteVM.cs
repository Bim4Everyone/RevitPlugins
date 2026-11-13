using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;

using RevitPackageDocumentation.Models;
using RevitPackageDocumentation.ViewModels.Configuration.SheetSetParameters.Parameters;
using RevitPackageDocumentation.ViewModels.FiltrationComboBoxVMs;
using RevitPackageDocumentation.ViewModels.Validation.Attributes;

namespace RevitPackageDocumentation.ViewModels.Configuration.Sheet.SheetComponents;

/// <summary>
/// Модуль текста: размещает на листе текстовое примечание с текстом по формуле
/// </summary>
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

    /// <summary>
    /// Ширина текстового примечания на листе, мм
    /// </summary>
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

    /// <summary>
    /// Текст, вычисленный по формуле
    /// </summary>
    public string Text {
        get => _text;
        set => RaiseAndSetIfChanged(ref _text, value);
    }

    public override void Process(bool processDependent = false) {
        var textNote = Place();
        SetCustomParams(textNote);
    }

    /// <summary>
    /// Размещает текстовое примечание слева от рамки листа. Если на листе уже есть примечание с таким же текстом
    /// (без учета вида переносов строк и пробелов по краям), новое не создается
    /// </summary>
    /// <returns>Созданное или найденное примечание; null, если листа нет</returns>
    public TextNote Place() {
        var sheetInstance = Sheet.SheetInstance;
        if(sheetInstance is null) {
            return null;
        }

        // Если текстовое примечание с таким текстом уже существует на листе, то новую не ставим
        string text = NormalizeText(Text);
        if(Repository.GetTextNotes(sheetInstance)
            .FirstOrDefault(t => NormalizeText(t.Text) == text) is TextNote textNote) {
            return textNote;
        }

        var options = new TextNoteOptions(TextNoteType.Id);
        // Точка вставки в мм в координатах листа
        var position = new XYZ(
            UnitUtilsHelper.ConvertToInternalValue(-190),
            UnitUtilsHelper.ConvertToInternalValue(170),
            0);
        var textNoteInstance = TextNote.Create(Repository.Document, sheetInstance.Id, position, Text, options);

        int textWidthAsInt = int.Parse(TextWidth);
        textNoteInstance.Width = UnitUtilsHelper.ConvertToInternalValue(textWidthAsInt);
        return textNoteInstance;
    }

    /// <summary>
    /// Приводит переносы строк к "\n" и убирает пробелы по краям. Revit хранит переносы в тексте примечания
    /// иначе, чем они приходят из поля ввода
    /// </summary>
    private static string NormalizeText(string text) {
        return (text ?? string.Empty).Replace("\r\n", "\n").Replace("\r", "\n").Trim();
    }
}
