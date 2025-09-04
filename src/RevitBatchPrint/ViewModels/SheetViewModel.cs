using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitBatchPrint.Models;

using IExportContext = RevitBatchPrint.Models.IExportContext;

namespace RevitBatchPrint.ViewModels;

internal sealed class SheetViewModel : BaseViewModel {
    private readonly ViewSheet _viewSheet;
    private readonly IPrintContext _printContext;
    private readonly IExportContext _exportContext;
    private readonly IMessageBoxService _messageBoxService;
    private readonly ILocalizationService _localizationService;

    private string _errorText;
    private bool _isSelected;
    private PrintSheetSettings _printSheetSettings;
    private ObservableCollection<string> _viewsWithoutCrop;

    public SheetViewModel(
        ViewSheet viewSheet,
        AlbumViewModel album,
        IPrintContext printContext,
        IExportContext exportContext,
        IMessageBoxService messageBoxService,
        ILocalizationService localizationService) {
        _viewSheet = viewSheet;
        _printContext = printContext;
        _exportContext = exportContext;

        _messageBoxService = messageBoxService;
        _localizationService = localizationService;

        Name = _viewSheet.Name;
        SheetNumber = _viewSheet.SheetNumber;

        Album = album;
        CheckCommand = Album.CheckUpdateCommand;
        PrintCommand = RelayCommand.Create(Print, CanPrint);
        ExportCommand = RelayCommand.Create(Export, CanExport);
    }

    public string Name { get; }
    public string SheetNumber { get; }
    public AlbumViewModel Album { get; }
    public ICommand CheckCommand { get; }

    public ICommand PrintCommand { get; }
    public ICommand ExportCommand { get; }

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
              + Environment.NewLine
              + " - "
              + string.Join(Environment.NewLine + " - ", ViewsWithoutCrop.Distinct().Take(5))
              + (ViewsWithoutCrop.Count > 5 ? "..." : null);

    public SheetElement CreateSheetElement() {
        return new SheetElement() {
            ViewSheet = _viewSheet,
            PrintSheetSettings = PrintSheetSettings
        };
    }

    private void Print() {
        ShowWarning();
        _printContext.Print([this]);
    }

    private bool CanPrint() {
        return _printContext.CanPrint([this]);
    }
    
    private void Export() {
        ShowWarning();
        _exportContext.Export([this]);
    }

    private bool CanExport() {
        return _exportContext.CanExport([this]);
    }

    private void ShowWarning() {
        if(ViewsWithoutCrop.Count > 0) {
            var messageBoxResult = _messageBoxService.Show(
                _localizationService.GetLocalizedString("MainWindow.ViewsWithoutCropMessage"),
                _localizationService.GetLocalizedString("MainWindow.Title"),
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if(messageBoxResult == MessageBoxResult.No) {
                throw new OperationCanceledException();
            }
        }
    }
}
