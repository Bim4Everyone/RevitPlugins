using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitBatchPrint.Models;
using RevitBatchPrint.Services;

namespace RevitBatchPrint.ViewModels;

/// <summary>
/// Основная ViewModel главного окна плагина.
/// </summary>
internal class MainViewModel : BaseViewModel, IPrintContext {
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
    private IRevitPrint _currentPrintExport;

    private string _albumParamName;
    private PrintOptionsViewModel _printOptions;

    private ObservableCollection<string> _albumParamNames;
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
        AcceptVewCommand = RelayCommand.Create<Window>(AcceptView, CanAcceptView);

        ChangeModeCommand = RelayCommand.Create(ChangeMode);
        ChooseSaveFileCommand = RelayCommand.Create(ChooseSaveFile);

        SearchCommand = RelayCommand.Create(ApplySearch);
        ChangeAlbumNameCommand = RelayCommand.Create(ChangeAlbumName);

        ShowPrint = true;
        ShowExport = false;
        _currentPrintExport = _revitPrint;
    }

    public ICommand LoadViewCommand { get; }
    public ICommand AcceptVewCommand { get; }

    public ICommand ChangeModeCommand { get; }
    public ICommand ChooseSaveFileCommand { get; }

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

    public ObservableCollection<string> AlbumParamNames {
        get => _albumParamNames;
        set => this.RaiseAndSetIfChanged(ref _albumParamNames, value);
    }

    private void LoadView() {
        AlbumParamNames = new ObservableCollection<string>(_revitRepository.GetAlbumParamNames());

        LoadConfig();
        CreateAlbumCollection();
    }

    private void ChangeMode() {
        ShowPrint = !ShowPrint;
        ShowExport = !ShowExport;

        if(ShowPrint) {
            _currentPrintExport = _revitPrint;
        } else if(ShowExport) {
            _currentPrintExport = _revitExport;
        }
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

    private void AcceptView(Window window) {
        SaveConfig();

        IEnumerable<SheetViewModel> sheets = MainAlbums
            .SelectMany(item => item.MainSheets)
            .Where(item => item.IsSelected)
            .ToArray();

        if(sheets.Any(item => item.ViewsWithoutCrop.Count > 0)) {
            var messageBoxResult = MessageBoxService.Show(
                _localizationService.GetLocalizedString("MainWindow.SheetsWithoutCropMessage"), 
                _localizationService.GetLocalizedString("MainWindow.Title"),
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if(messageBoxResult == MessageBoxResult.No) {
                throw new OperationCanceledException();
            }
        }

        ExecutePrintExport(sheets);
        window.DialogResult = true;
    }

    public void ExecutePrintExport(IEnumerable<SheetViewModel> sheets) {
        SheetElement[] sheetElements = sheets
            .Select(item => item.CreateSheetElement())
            .ToArray();

        _currentPrintExport.Execute(sheetElements, PrintOptions.CreatePrintOptions());
    }

    public bool CanExecutePrintExport(IEnumerable<SheetViewModel> sheets) {
        return CanAcceptView(false);
    }

    private bool CanAcceptView(Window window) {
        return CanAcceptView(true);
    }

    private bool CanAcceptView(bool checkSelected) {
        if(ShowPrint && string.IsNullOrEmpty(PrintOptions?.PrinterName)) {
            ErrorText = _localizationService.GetLocalizedString("MainWindow.NotSelectedPrinter");
            return false;
        }

        if(ShowExport && string.IsNullOrEmpty(PrintOptions?.FilePath)) {
            ErrorText = _localizationService.GetLocalizedString("MainWindow.NotSelectedFilePath");
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

    private void CreateAlbumCollection() {
        AlbumViewModel[] albums = _revitRepository.GetViewSheets()
            .GroupBy(item => string.IsNullOrEmpty(AlbumParamName)
                ? null
                : item.GetParamValueOrDefault<string>(AlbumParamName))
            .Select(item => CreateAlbum(item.Key, item))
            .OrderBy(item => item.Name)
            .ToArray();

        SearchText = null;
        MainAlbums = new ObservableCollection<AlbumViewModel>(albums);
        FilteredAlbums = new ObservableCollection<AlbumViewModel>(albums);
    }

    private AlbumViewModel CreateAlbum(string albumName, IEnumerable<ViewSheet> viewSheets) {
        var viewModel = new AlbumViewModel(albumName, this, MessageBoxService, _localizationService);

        SheetViewModel[] sheets = CreateSheetCollection(viewModel, viewSheets);
        viewModel.MainSheets = new ObservableCollection<SheetViewModel>(sheets);
        viewModel.FilteredSheets = new ObservableCollection<SheetViewModel>(sheets);
        
        viewModel.ViewsWithoutCrop = new ObservableCollection<string>(
            viewModel.MainSheets
                .Where(item => item.ViewsWithoutCrop.Count > 0)
                .Select(item => item.Name)
                .ToList());
        
        return viewModel;
    }

    private SheetViewModel[] CreateSheetCollection(AlbumViewModel album, IEnumerable<ViewSheet> viewSheets) {
        return viewSheets
            .Select(item => CreateSheet(item, album))
            .OrderBy(item => item.Name)
            .ToArray();
    }

    private SheetViewModel CreateSheet(ViewSheet viewSheet, AlbumViewModel album) {
        return new SheetViewModel(viewSheet, album, this, MessageBoxService, _localizationService) {
            PrintSheetSettings = _revitRepository.GetPrintSettings(viewSheet),
            ViewsWithoutCrop = new ObservableCollection<string>(
                _revitRepository.GetViewsWithoutCrop(viewSheet)
                    .Select(item => item.Name))
        };
    }

    /// <summary>
    /// Загрузка настроек плагина.
    /// </summary>
    private void LoadConfig() {
        RevitSettings setting = _pluginConfig.GetSettings(_revitRepository.Document);

        AlbumParamName = AlbumParamNames
                               .FirstOrDefault(item =>
                                   item.Equals(setting?.AlbumParamName))
                           ?? AlbumParamNames
                               .FirstOrDefault(item =>
                                   PluginSystemConfig.PrintParamNames.Contains(item))
                           ?? AlbumParamNames.FirstOrDefault();

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
