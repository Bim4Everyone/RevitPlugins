using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitBatchPrint.Models;

namespace RevitBatchPrint.ViewModels;

internal sealed class SheetViewModel : BaseViewModel {
    private readonly ViewSheet _viewSheet;
    private readonly IPrintContext _printContext;
    private readonly ILocalizationService _localizationService;

    private string _errorText;
    private bool _isSelected;
    private PrintSheetSettings _printSheetSettings;
    private ObservableCollection<string> _viewsWithoutCrop;

    public SheetViewModel(
        ViewSheet viewSheet,
        AlbumViewModel album,
        IPrintContext printContext,
        ILocalizationService localizationService) {
        _viewSheet = viewSheet;
        _printContext = printContext;
        _localizationService = localizationService;

        Name = _viewSheet.Name;
        Album = album;
        CheckCommand = Album.CheckUpdateCommand;
        PrintExportCommand = RelayCommand.Create(ExecutePrintExport, CanExecutePrintExport);
    }

    public string Name { get; }
    public AlbumViewModel Album { get; }
    public ICommand CheckCommand { get; }
    public ICommand PrintExportCommand { get; }

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

    public ObservableCollection<string> ViewsWithoutCrop {
        get => _viewsWithoutCrop;
        set => this.RaiseAndSetIfChanged(ref _viewsWithoutCrop, value);
    }

    public string ViewsWithoutCropText =>
        ViewsWithoutCrop.Count == 0
            ? null
            : _localizationService.GetLocalizedString("TreeView.ViewsWithoutCropToolTip")
              + Environment.NewLine + " - "
              + string.Join(Environment.NewLine + " - ", ViewsWithoutCrop.Take(5))
              + (ViewsWithoutCrop.Count > 5 ? "..." : null);
    
    public SheetElement CreateSheetElement() {
        return new SheetElement() {ViewSheet = _viewSheet, PrintSheetSettings = PrintSheetSettings};
    }
    
    private void ExecutePrintExport() {
        _printContext.ExecutePrintExport([this]);
    }
    
    private bool CanExecutePrintExport() {
        return _printContext.CanExecutePrintExport([this]);
    }
}
