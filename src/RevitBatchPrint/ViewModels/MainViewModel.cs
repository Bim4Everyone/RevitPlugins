using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Revit;
using dosymep.Revit.Comparators;
using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitBatchPrint.Extensions;
using RevitBatchPrint.Models;
using RevitBatchPrint.Services;

using IExportContext = RevitBatchPrint.Models.IExportContext;

namespace RevitBatchPrint.ViewModels;

/// <summary>
/// Основная ViewModel главного окна плагина.
/// </summary>
internal class MainViewModel : BaseViewModel, IPrintContext, IExportContext {
    private readonly PluginConfig _pluginConfig;
    private readonly RevitRepository _revitRepository;

    private readonly IRevitPrint _revitPrint;
    private readonly IRevitPrint _revitExport;

    private readonly IPrinterService _printerService;
    private readonly ILocalizationService _localizationService;
    private readonly ISaveFileDialogService _saveFileDialogService;

    private string _errorText;
    private string _searchText;

    private bool _showPrint;
    private bool _showExport;

    private string _albumParamName;
    private PrintOptionsViewModel _printOptions;

    private ObservableCollection<RevitParam> _albumParamNames;
    private ObservableCollection<AlbumViewModel> _mainAlbums;
    private ObservableCollection<AlbumViewModel> _filteredAlbums;

    public MainViewModel(
        PluginConfig pluginConfig,
        RevitRepository revitRepository,
        RevitPrint revitPrint,
        RevitExportToPdf revitExport,
        IPrinterService printerService,
        ILocalizationService localizationService,
        IMessageBoxService messageBoxService,
        ISaveFileDialogService saveFileDialogService) {
        _pluginConfig = pluginConfig;
        _revitRepository = revitRepository;

        _revitPrint = revitPrint;
        _revitExport = revitExport;

        _printerService = printerService;
        _localizationService = localizationService;
        _saveFileDialogService = saveFileDialogService;
        MessageBoxService = messageBoxService;

        LoadViewCommand = RelayCommand.Create(LoadView);
        ChooseSaveFileCommand = RelayCommand.Create(ChooseSaveFile);

        PrintCommand = RelayCommand.Create<Window>(Print, CanPrint);
        ExportCommand = RelayCommand.Create<Window>(Export, CanExport);

        SearchCommand = RelayCommand.Create(ApplySearch);
        ChangeAlbumNameCommand = RelayCommand.Create(ChangeAlbumName);

#if REVIT_2021_OR_LESS
        ShowPrint = true;
        ShowExport = false;
#else
        ShowPrint = true;
        ShowExport = true;
#endif
    }

    public ICommand LoadViewCommand { get; }
    public ICommand ChooseSaveFileCommand { get; }

    public ICommand PrintCommand { get; }
    public ICommand ExportCommand { get; }

    public ICommand SearchCommand { get; set; }
    public ICommand ChangeAlbumNameCommand { get; set; }

    public IMessageBoxService MessageBoxService { get; }

    /// <summary>
    /// Текст ошибки, который отображается при неверном вводе пользователя.
    /// </summary>
    public string ErrorText {
        get => _errorText;
        set => RaiseAndSetIfChanged(ref _errorText, value);
    }

    public string SearchText {
        get => _searchText;
        set => this.RaiseAndSetIfChanged(ref _searchText, value);
    }

    public bool ShowPrint {
        get => _showPrint;
        set => this.RaiseAndSetIfChanged(ref _showPrint, value);
    }

    public bool ShowExport {
        get => _showExport;
        set => this.RaiseAndSetIfChanged(ref _showExport, value);
    }

    public string AlbumParamName {
        get => _albumParamName;
        set => this.RaiseAndSetIfChanged(ref _albumParamName, value);
    }

    public PrintOptionsViewModel PrintOptions {
        get => _printOptions;
        set => this.RaiseAndSetIfChanged(ref _printOptions, value);
    }

    public ObservableCollection<AlbumViewModel> MainAlbums {
        get => _mainAlbums;
        set => this.RaiseAndSetIfChanged(ref _mainAlbums, value);
    }

    public ObservableCollection<AlbumViewModel> FilteredAlbums {
        get => _filteredAlbums;
        set => this.RaiseAndSetIfChanged(ref _filteredAlbums, value);
    }

    public ObservableCollection<RevitParam> AlbumParamNames {
        get => _albumParamNames;
        set => this.RaiseAndSetIfChanged(ref _albumParamNames, value);
    }

    private void LoadView() {
        AlbumParamNames = new ObservableCollection<RevitParam>(_revitRepository.GetAlbumParamNames());

        LoadConfig();
        CreateAlbumCollection();
    }

    private void ChooseSaveFile() {
        string fileName = string.IsNullOrEmpty(PrintOptions.FilePath)
            ? $"{_revitRepository.Document.Title}.pdf"
            : Path.GetFileName(PrintOptions.FilePath);

        string folderName = string.IsNullOrEmpty(PrintOptions.FilePath)
            ? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            : Path.GetDirectoryName(PrintOptions.FilePath);

        if(_saveFileDialogService.ShowDialog(folderName, fileName)) {
            PrintOptions.FilePath = _saveFileDialogService.File.FullName;
        }
    }

    private void Print(Window window) {
        SaveConfig();
        Print(GetSheets());

        window.DialogResult = true;
    }

    private bool CanPrint(Window window) {
        return CanAcceptView(true, true, false);
    }

    private void Export(Window window) {
        SaveConfig();
        Export(GetSheets());

        window.DialogResult = true;
    }

    private bool CanExport(Window window) {
        return CanAcceptView(true, false, true);
    }

    public void Print(IEnumerable<SheetViewModel> sheets) {
        SheetElement[] sheetElements = sheets
            .Select(item => item.CreateSheetElement())
            .ToArray();

        _revitPrint.Execute(sheetElements, PrintOptions.CreatePrintOptions());
    }

    public bool CanPrint(IEnumerable<SheetViewModel> sheets) {
        return CanAcceptView(false, true, false);
    }

    public void Export(IEnumerable<SheetViewModel> sheets) {
        SheetElement[] sheetElements = sheets
            .Select(item => item.CreateSheetElement())
            .ToArray();

        _revitExport.Execute(sheetElements, PrintOptions.CreatePrintOptions());
    }

    public bool CanExport(IEnumerable<SheetViewModel> sheets) {
        return CanAcceptView(false, false, true);
    }

    private IEnumerable<SheetViewModel> GetSheets() {
        IEnumerable<SheetViewModel> sheets = MainAlbums
            .SelectMany(item => item.MainSheets)
            .Where(item => item.IsSelected)
            .ToArray();

        if(sheets.Any(item => item.ViewsWithoutCrop.Count > 0)) {
            var messageBoxResult = MessageBoxService.Show(
                _localizationService.GetLocalizedString("MainWindow.SheetsWithoutCropMessage"),
                _localizationService.GetLocalizedString("MainWindow.Title"),
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if(messageBoxResult == MessageBoxResult.No) {
                throw new OperationCanceledException();
            }
        }

        return sheets;
    }

    private bool CanAcceptView(bool checkSelected, bool checkPrint, bool checkExport) {
        if(checkPrint
           && ShowPrint
           && string.IsNullOrEmpty(PrintOptions?.PrinterName)) {
            ErrorText = _localizationService.GetLocalizedString("MainWindow.NotSelectedPrinter");
            return false;
        }

        if(checkExport
           && ShowExport
           && string.IsNullOrEmpty(PrintOptions?.FilePath)) {
            ErrorText = _localizationService.GetLocalizedString("MainWindow.NotSelectedFilePath");
            return false;
        }

        if(checkExport
           && ShowExport
           && !PrintOptions.FilePath.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase)) {
            ErrorText = _localizationService.GetLocalizedString("MainWindow.FilePathNotPdf");
            return false;
        }

        if(checkExport
           && ShowExport
           && !Path.IsPathRooted(PrintOptions.FilePath)) {
            ErrorText = _localizationService.GetLocalizedString("MainWindow.FilePathNotRooted");
            return false;
        }

        if(checkSelected && !MainAlbums.Any(item => item.HasSelectedSheets())) {
            ErrorText = _localizationService.GetLocalizedString("MainWindow.NotSelectedAlbumSheets");
            return false;
        }

        ErrorText = null;
        return true;
    }

    private void ApplySearch() {
        foreach(AlbumViewModel albumViewModel in MainAlbums) {
            albumViewModel.FilterSheets(SearchText);
        }

        if(string.IsNullOrWhiteSpace(SearchText)) {
            FilteredAlbums = new ObservableCollection<AlbumViewModel>(MainAlbums);
        } else {
            FilteredAlbums = new ObservableCollection<AlbumViewModel>(
                MainAlbums
                    .Where(item =>
                        item.FilteredSheets.Count > 0
                        || item.Name.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0));
        }
    }

    private void ChangeAlbumName() {
        CreateAlbumCollection();
    }

    private RevitParam GetSelectedRevitParam() {
        return AlbumParamNames.FirstOrDefault(item => item.Name == AlbumParamName);
    }

    private void CreateAlbumCollection() {
        AlbumViewModel[] albums = _revitRepository.GetSheetsInfo()
            .GroupBy(item => AlbumParamName == null
                ? null
                : item.ViewSheet.GetParamDisplayValue(GetSelectedRevitParam()))
            .Select(item => CreateAlbum(item.Key, item))
            .OrderBy(item => item.Name)
            .ToArray();

        SearchText = null;
        MainAlbums = new ObservableCollection<AlbumViewModel>(albums);
        FilteredAlbums = new ObservableCollection<AlbumViewModel>(albums);
    }

    private AlbumViewModel CreateAlbum(
        string albumName,
        IEnumerable<(ViewSheet ViewSheet, FamilyInstance TitleBlock, Viewport[] Viewports)> sheetsInfo) {
        var viewModel = new AlbumViewModel(
            albumName,
            this,
            this,
            MessageBoxService,
            _localizationService);

        SheetViewModel[] sheets = CreateSheetCollection(viewModel, sheetsInfo);
        viewModel.MainSheets = new ObservableCollection<SheetViewModel>(sheets);
        viewModel.FilteredSheets = new ObservableCollection<SheetViewModel>(sheets);

        viewModel.ViewsWithoutCrop = new ObservableCollection<string>(
            viewModel.MainSheets
                .Where(item => item.ViewsWithoutCrop.Count > 0)
                .Select(item => item.Name)
                .ToList());

        return viewModel;
    }

    private SheetViewModel[] CreateSheetCollection(
        AlbumViewModel album,
        IEnumerable<(ViewSheet ViewSheet, FamilyInstance TitleBlock, Viewport[] Viewports)> sheetsInfo) {
        LogicalStringComparer comparer = new();
        return sheetsInfo
            .Select(item => CreateSheet(album, item))
            .OrderBy(item => item.SheetNumber, comparer)
            .ToArray();
    }

    private SheetViewModel CreateSheet(
        AlbumViewModel album,
        (ViewSheet ViewSheet, FamilyInstance TitleBlock, Viewport[] Viewports) sheetsInfo) {
        return new SheetViewModel(
            sheetsInfo.ViewSheet,
            album,
            this,
            this,
            MessageBoxService,
            _localizationService) {
            PrintSheetSettings = _revitRepository.GetPrintSettings(sheetsInfo.TitleBlock),
            ViewsWithoutCrop = new ObservableCollection<string>(sheetsInfo.Viewports?.Select(item => item.Name) ?? [])
        };
    }

    /// <summary>
    /// Загрузка настроек плагина.
    /// </summary>
    private void LoadConfig() {
        RevitSettings setting = _pluginConfig.GetSettings(_revitRepository.Document);

        AlbumParamName = AlbumParamNames
                             .Select(item => item.Name)
                             .FirstOrDefault(item =>
                                 item.Equals(setting?.AlbumParamName))
                         ?? AlbumParamNames
                             .Select(item => item.Name)
                             .FirstOrDefault(item =>
                                 PluginSystemConfig.PrintParamNames.Contains(item))
                         ?? AlbumParamNames
                             .Select(item => item.Name)
                             .FirstOrDefault();

        PrintOptions = new PrintOptionsViewModel(
            _printerService.EnumPrinterNames(),
            setting?.PrintOptions ?? new PrintOptions());
    }

    /// <summary>
    /// Сохранение настроек плагина.
    /// </summary>
    private void SaveConfig() {
        RevitSettings setting = _pluginConfig.GetSettings(_revitRepository.Document)
                                ?? _pluginConfig.AddSettings(_revitRepository.Document);

        setting.AlbumParamName = AlbumParamName;
        setting.PrintOptions = PrintOptions.CreatePrintOptions();

        _pluginConfig.SaveProjectConfig();
    }
}
