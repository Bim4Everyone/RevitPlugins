using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

using Autodesk.Revit.DB;

using Bim4Everyone.RevitFiltration;
using Bim4Everyone.RevitFiltration.Controls;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using Microsoft.WindowsAPICodePack.Dialogs;

using RevitMarkAllDocuments.Models;
using RevitMarkAllDocuments.Models.Export;
using RevitMarkAllDocuments.Services;

namespace RevitMarkAllDocuments.ViewModels;

internal class MainViewModel : BaseViewModel {
    private readonly PluginConfig _pluginConfig;
    private readonly RevitRepository _revitRepository;
    private readonly MarkListWindowService _markListWindowService;
    private readonly ILogicalFilterProviderFactory _filterFactory;
    private readonly ILocalizationService _localizationService;
    private readonly Category _selectedCategory;
    private readonly string _selectedCategoryName;

    private DocumentsPageViewModel _documentsPageViewModel;
    private FilterPageViewModel _filterPageViewModel;
    private SortPageViewModel _sortPageViewModel;
    private MarkSettingsPageViewModel _markSettingsPageViewModel;

    private string _errorText;

    public MainViewModel(PluginConfig pluginConfig,
                         RevitRepository revitRepository,
                         CategoryContext categoryContext,
                         MarkListWindowService markListWindowService,
                         ILogicalFilterProviderFactory filterFactory,
                         ILocalizationService localizationService) {        
        _pluginConfig = pluginConfig;
        _revitRepository = revitRepository;
        _markListWindowService = markListWindowService;
        _filterFactory = filterFactory;
        _localizationService = localizationService;

        _selectedCategory = categoryContext.SelectedCategory;
        _selectedCategoryName = categoryContext.SelectedCategory.Name;

        LoadViewCommand = RelayCommand.Create(LoadView);
        AcceptViewCommand = RelayCommand.Create(AcceptView, CanAcceptView);
    }

    public ICommand LoadViewCommand { get; }
    
    /// <summary>
    /// Команда применения настроек главного окна. (запуск плагина)
    /// </summary>
    /// <remarks>В случаях, когда используется немодальное окно, требуется данную команду удалять.</remarks>
    public ICommand AcceptViewCommand { get; }

    public DocumentsPageViewModel DocumentsPageViewModel => _documentsPageViewModel;
    public FilterPageViewModel FilterPageViewModel => _filterPageViewModel;
    public SortPageViewModel SortPageViewModel => _sortPageViewModel;
    public MarkSettingsPageViewModel MarkSettingsPageViewModel => _markSettingsPageViewModel;

    public string SelectedCategoryName => _selectedCategoryName;

    public string ErrorText {
        get => _errorText;
        set => RaiseAndSetIfChanged(ref _errorText, value);
    }

    private void LoadView() {
        _documentsPageViewModel = new DocumentsPageViewModel(_revitRepository);
        _filterPageViewModel = new FilterPageViewModel(_filterFactory, _localizationService, 
                new FilterDataProvider(_revitRepository, _selectedCategory));
        _sortPageViewModel = new SortPageViewModel(_revitRepository, _selectedCategory);
        _markSettingsPageViewModel = new MarkSettingsPageViewModel(_revitRepository, _selectedCategory);

        LoadConfig();
    }

    public string SelectFolder() {
        var dialog = new CommonOpenFileDialog() {
            IsFolderPicker = true
        };

        if(dialog.ShowDialog() == CommonFileDialogResult.Ok) {
            return dialog.FileName;
        }

        return string.Empty;
    }

    private void AcceptView() {
        var filtrationService = new FiltrationService();
        var markSetterService = new MarkSetterService();

        // get documents
        var checkedDocuments = DocumentsPageViewModel.Documents
            .Where(d => d.IsChecked)
            .Select(d => d.Document)
            .ToArray();

        // filter elements
        var markData = filtrationService.FilterElements(checkedDocuments, FilterPageViewModel.FilterProvider);
        markData.ParamName = MarkSettingsPageViewModel.SelectedParam.Name;

        // sort and mark elements
        var sortParams = SortPageViewModel.SelectedParams
            .Select(x => x.RevitParam)
            .ToList();
        var startValue = MarkSettingsPageViewModel.GetStartValue();
        markData.SerMarkValues(sortParams, startValue);

        var docService = new DocumentService();
        string currentDocName = docService.GetDocumentFullName(_revitRepository.Document);

        if(markData.HasLinksForExport(currentDocName)) {
            string path = SelectFolder();
            string fullPath = path + "\\" + $"{currentDocName}.json";

            var exporter = new JsonExporter();
            exporter.Export(fullPath, markData);
        }

        var markDataForCurrentDoc = markData.GetDataByDocument(currentDocName);        
        if(markDataForCurrentDoc != null) {
            _markListWindowService.ShowWindow(_revitRepository.Document, markDataForCurrentDoc);
        }

        SaveConfig();
    }

    private bool CanAcceptView() {
        if(!DocumentsPageViewModel.Documents.Any(d => d.IsChecked)) {
            ErrorText = "Не выбраны документы";
            return false;
        }
        if(!SortPageViewModel.SelectedParams.Any()) {
            ErrorText = "Не выбраны параметры для сортировки";
            return false;
        }
        if(MarkSettingsPageViewModel.SelectedParam == null) {
            ErrorText = "Не выбран параметр для заполнения марки";
            return false;
        }
        if(string.IsNullOrEmpty(MarkSettingsPageViewModel.StartNumber)) {
            ErrorText = "Не заполнен начальный номер";
            return false;
        }

        ErrorText = null;
        return true;
    }

    private void LoadConfig() {
        RevitSettings setting = _pluginConfig.GetSettings(_revitRepository.Document);
    }

    private void SaveConfig() {
        RevitSettings setting = _pluginConfig.GetSettings(_revitRepository.Document)
                                ?? _pluginConfig.AddSettings(_revitRepository.Document);

        _pluginConfig.SaveProjectConfig();
    }
}
