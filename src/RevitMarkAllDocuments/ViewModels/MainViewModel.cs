using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

using Autodesk.Revit.DB;

using Bim4Everyone.RevitFiltration;
using Bim4Everyone.RevitFiltration.Controls;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitMarkAllDocuments.Models;
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

    private void AcceptView() {
        var filtrationService = new FiltrationService();
        var sortElementService = new SortElementService();
        var markSetterService = new MarkSetterService();

        // get documents
        var documents = DocumentsPageViewModel.Documents
            .Where(d => d.IsChecked)
            .Select(d => d.Document)
            .ToArray();

        var filterProvider = FilterPageViewModel.FilterProvider;

        // filter elements
        var allElements = filtrationService
            .FilterElements(documents, FilterPageViewModel.FilterProvider);

        // sort elements
        var sortedElements = sortElementService.SortElements();

        // create mark values

        bool linksSelected = DocumentsPageViewModel.Documents
            .Where(d => d.IsChecked)
            .Any(d => d.IsLink);

        if(linksSelected) {
            //export JSON
        }

        bool currentDocSelected = DocumentsPageViewModel.Documents
            .Where(d => d.IsChecked)
            .Any(d => !d.IsLink);

        if(currentDocSelected) {
            //show elements list

            _markListWindowService.ShowWindow(_revitRepository.Document, allElements);
        }

        SaveConfig();
    }

    private bool CanAcceptView() {
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
