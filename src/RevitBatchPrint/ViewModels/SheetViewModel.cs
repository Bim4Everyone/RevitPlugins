using Autodesk.Revit.DB;

using dosymep.SimpleServices;
using dosymep.WPF.ViewModels;

using RevitBatchPrint.Models;

namespace RevitBatchPrint.ViewModels;

internal sealed class SheetViewModel : BaseViewModel {
    private readonly ViewSheet _viewSheet;
    private readonly ILocalizationService _localizationService;

    private string _errorText;
    private bool _isSelected;
    private PrintSheetSettings _printSheetSettings;

    public SheetViewModel(ViewSheet viewSheet, AlbumViewModel album, ILocalizationService localizationService) {
        _viewSheet = viewSheet;
        _localizationService = localizationService;

        Name = _viewSheet.Name;
        Album = album;
    }

    public string Name { get; }
    public AlbumViewModel Album { get; }

    public string ErrorText {
        get => _errorText;
        set => this.RaiseAndSetIfChanged(ref _errorText, value);
    }

    public bool IsSelected {
        get => _isSelected;
        set => this.RaiseAndSetIfChanged(ref _isSelected, value);
    }

    public PrintSheetSettings PrintSheetSettings {
        get => _printSheetSettings;
        set => this.RaiseAndSetIfChanged(ref _printSheetSettings, value);
    }

    public SheetElement CreateSheetElement() {
        return new SheetElement() {ViewSheet = _viewSheet, PrintSheetSettings = PrintSheetSettings};
    }
}
