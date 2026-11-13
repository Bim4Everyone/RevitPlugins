using System;
using System.Collections;
using System.Collections.Generic;
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

internal sealed class AlbumViewModel : BaseViewModel {
    private readonly IPrintContext _printContext;
    private readonly IExportContext _exportContext;
    private readonly IMessageBoxService _messageBoxService;
    private readonly ILocalizationService _localizationService;

    private bool? _isSelected;
    private ObservableCollection<SheetViewModel> _mainSheets;
    private ObservableCollection<SheetViewModel> _filteredSheets;
    private ObservableCollection<string> _viewsWithoutCrop;

    public AlbumViewModel(
        string albumName,
        IPrintContext printContext,
        IExportContext exportContext,
        IMessageBoxService messageBoxService,
        ILocalizationService localizationService) {
        _printContext = printContext;
        _exportContext = exportContext;

        _messageBoxService = messageBoxService;
        _localizationService = localizationService;

        IsSelected = false;

        Name = string.IsNullOrEmpty(albumName)
            ? localizationService.GetLocalizedString("MainWindow.EmptyAlbumNameParam")
            : albumName;

        CheckCommand = RelayCommand.Create(Check);
        CheckUpdateCommand = RelayCommand.Create(CheckUpdate);

        PrintCommand = RelayCommand.Create(Print, CanPrint);
        ExportCommand = RelayCommand.Create(Export, CanExport);
    }

    public ICommand CheckCommand { get; }
    public ICommand CheckUpdateCommand { get; }

    public ICommand PrintCommand { get; }
    public ICommand ExportCommand { get; }

    public string Name { get; }

    public bool? IsSelected {
        get => _isSelected;
        set => this.RaiseAndSetIfChanged(ref _isSelected, value);
    }

    public ObservableCollection<SheetViewModel> MainSheets {
        get => _mainSheets;
        set => this.RaiseAndSetIfChanged(ref _mainSheets, value);
    }

    public ObservableCollection<SheetViewModel> FilteredSheets {
        get => _filteredSheets;
        set => this.RaiseAndSetIfChanged(ref _filteredSheets, value);
    }

    public ObservableCollection<string> ViewsWithoutCrop {
        get => _viewsWithoutCrop;
        set => this.RaiseAndSetIfChanged(ref _viewsWithoutCrop, value);
    }

    public string ViewsWithoutCropText =>
        ViewsWithoutCrop.Count == 0
            ? null
            : _localizationService.GetLocalizedString("TreeView.SheetsWithoutCropToolTip")
              + Environment.NewLine
              + " - "
              + string.Join(Environment.NewLine + " - ", ViewsWithoutCrop.Distinct().Take(5))
              + (ViewsWithoutCrop.Count > 5 ? "..." : null);

    public void FilterSheets(string searchText) {
        if(string.IsNullOrWhiteSpace(searchText)) {
            FilteredSheets = new ObservableCollection<SheetViewModel>(MainSheets);
        } else {
            FilteredSheets = new ObservableCollection<SheetViewModel>(
                MainSheets.Where(item =>
                    item.Name.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0
                    || item.SheetNumber.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0));
        }

        // Обновляем выделение после фильтрации,
        // чтобы выделение стало корректным из-за скрытых элементов
        CheckUpdate();
    }

    public bool HasSelectedSheets() {
        return MainSheets.Any(item => item.IsSelected);
    }

    private void Check() {
        if(IsSelected == null) {
            return;
        }

        var sheets = FilteredSheets.Count == 0
            ? MainSheets
            : FilteredSheets;

        foreach(SheetViewModel sheetViewModel in sheets) {
            sheetViewModel.IsSelected = IsSelected.Value;
        }
    }

    private void CheckUpdate() {
        int countSelected = MainSheets.Count(item => item.IsSelected);
        if(countSelected == 0) {
            IsSelected = false;
        } else if(MainSheets.Count == countSelected) {
            IsSelected = true;
        } else if(MainSheets.Count != countSelected) {
            IsSelected = null;
        }
    }

    private void Print() {
        ShowWarning();
        _printContext.Print(MainSheets);
    }

    private bool CanPrint() {
        return _printContext.CanPrint(MainSheets);
    }

    private void Export() {
        ShowWarning();
        _exportContext.Export(MainSheets);
    }

    private bool CanExport() {
        return _exportContext.CanExport(MainSheets);
    }

    private void ShowWarning() {
        if(MainSheets.Any(item => item.ViewsWithoutCrop.Count > 0)) {
            var messageBoxResult = _messageBoxService.Show(
                _localizationService.GetLocalizedString("MainWindow.SheetsWithoutCropMessage"),
                _localizationService.GetLocalizedString("MainWindow.Title"),
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if(messageBoxResult == MessageBoxResult.No) {
                throw new OperationCanceledException();
            }
        }
    }
}
