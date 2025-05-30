using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
internal class MainViewModel : BaseViewModel {
    private readonly PluginConfig _pluginConfig;
    private readonly RevitRepository _revitRepository;

    private readonly IRevitPrint _revitPrint;
    private readonly IRevitPrint _revitExport;

    private readonly IPrinterService _printerService;
    private readonly ILocalizationService _localizationService;

    private string _errorText;
    private string _searchText;
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
        ILocalizationService localizationService) {
        _pluginConfig = pluginConfig;
        _revitRepository = revitRepository;

        _revitPrint = revitPrint;
        _revitExport = revitExport;

        _printerService = printerService;
        _localizationService = localizationService;

        LoadViewCommand = RelayCommand.Create(LoadView);

        PrintCommand = RelayCommand.Create(Print, CanPrint);
        ExportCommand = RelayCommand.Create(Export, CanExport);

        SearchCommand = RelayCommand.Create(ApplySearch);
    }


    public ICommand LoadViewCommand { get; }

    public ICommand PrintCommand { get; }
    public ICommand ExportCommand { get; }

    public ICommand SearchCommand { get; set; }

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

    private void Print() {
        AcceptView(_revitPrint);
    }

    private bool CanPrint() {
        if(string.IsNullOrEmpty(AlbumParamName)) {
            ErrorText = _localizationService.GetLocalizedString("MainWindow.NotSelectedAlbumParamName");
            return false;
        }

        if(string.IsNullOrEmpty(PrintOptions.PrinterName)) {
            ErrorText = _localizationService.GetLocalizedString("MainWindow.NotSelectedPrinter");
            return false;
        }

        if(!MainAlbums.Any(item => item.HasSelectedSheets())) {
            ErrorText = _localizationService.GetLocalizedString("MainWindow.NotSelectedAlbumSheets");
            return false;
        }

        ErrorText = null;
        return true;
    }

    private void Export() {
        AcceptView(_revitExport);
    }

    private bool CanExport() {
        if(string.IsNullOrEmpty(AlbumParamName)) {
            ErrorText = _localizationService.GetLocalizedString("MainWindow.NotSelectedAlbumParamName");
            return false;
        }

        if(string.IsNullOrEmpty(PrintOptions.FilePath)) {
            ErrorText = _localizationService.GetLocalizedString("MainWindow.NotSelectedFilePath");
            return false;
        }

        if(!MainAlbums.Any(item => item.HasSelectedSheets())) {
            ErrorText = _localizationService.GetLocalizedString("MainWindow.NotSelectedAlbumSheets");
            return false;
        }

        ErrorText = null;
        return true;
    }

    private void AcceptView(IRevitPrint revitPrint) {
        SaveConfig();

        SheetElement[] sheetElements = MainAlbums
            .SelectMany(item => item.MainSheets)
            .Where(item => item.IsSelected)
            .Select(item => item.CreateSheetElement())
            .ToArray();

        _revitExport.Execute(sheetElements, PrintOptions.CreatePrintOptions());
    }

    private void ApplySearch() {
        if(string.IsNullOrEmpty(SearchText)) {
            FilteredAlbums = new ObservableCollection<AlbumViewModel>(MainAlbums);
        } else {
            FilteredAlbums = new ObservableCollection<AlbumViewModel>(
                MainAlbums
                    .Where(item => item.Name.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) > 1));
        }

        foreach(AlbumViewModel albumViewModel in FilteredAlbums) {
            albumViewModel.FilterSheets(SearchText);
        }
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
        var viewModel = new AlbumViewModel(albumName, _localizationService);

        SheetViewModel[] sheets = CreateSheetCollection(viewModel, viewSheets);
        viewModel.MainSheets = new ObservableCollection<SheetViewModel>(sheets);
        viewModel.FilteredSheets = new ObservableCollection<SheetViewModel>(sheets);

        return viewModel;
    }

    private SheetViewModel[] CreateSheetCollection(AlbumViewModel album, IEnumerable<ViewSheet> viewSheets) {
        return viewSheets
            .Select(item => CreateSheet(item, album))
            .OrderBy(item => item.Name)
            .ToArray();
    }

    private SheetViewModel CreateSheet(ViewSheet viewSheet, AlbumViewModel album) {
        return new SheetViewModel(viewSheet, album, _localizationService) {
            PrintSheetSettings = _revitRepository.GetPrintSettings(viewSheet)
        };
    }

    /// <summary>
    /// Загрузка настроек плагина.
    /// </summary>
    private void LoadConfig() {
        RevitSettings setting = _pluginConfig.GetSettings(_revitRepository.Document);

        AlbumParamName = setting?.AlbumParamName;
        AlbumParamName ??= AlbumParamNames
            .FirstOrDefault(item =>
                PluginSystemConfig.PrintParamNames.Contains(item));

        PrintOptions = new PrintOptionsViewModel(setting?.PrintOptions ?? new PrintOptions()) {
            PrinterNames = new ObservableCollection<string>(_printerService.EnumPrinterNames())
        };
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
