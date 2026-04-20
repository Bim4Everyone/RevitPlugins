using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    private readonly DocumentService _documentService;
    private readonly WindowsService _windowsService;
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
                         DocumentService documentService,
                         WindowsService markListWindowService,
                         ILogicalFilterProviderFactory filterFactory,
                         ILocalizationService localizationService) {        
        _pluginConfig = pluginConfig;
        _revitRepository = revitRepository;
        _documentService = documentService;
        _windowsService = markListWindowService;
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
        var dataProvider = new FilterDataProvider(_revitRepository, _selectedCategory);
        _filterPageViewModel = new FilterPageViewModel(_filterFactory, _localizationService, dataProvider);
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

        // creating mark data and filter elements
        var selectedParam = MarkSettingsPageViewModel.SelectedParam;
        var markData = new MarkData() {
            RevitParam = selectedParam.RevitParam,
        };
        markData = filtrationService.FilterElements(markData, checkedDocuments, FilterPageViewModel.FilterProvider);

        // checking params
        var sortParams = SortPageViewModel.SelectedParams
            .Select(x => x.RevitParam)
            .ToList();

        var validation = new ParamValidationService();
        var filteredElements = markData.GetAllElements();
        var warningsWithNoParams = new WarningsViewModel();

        var warningElementsSortParams = validation.CheckAreExistParams(sortParams, filteredElements);

        if(warningElementsSortParams.Any()) {
            var warning = new WarningViewModel() {
                Elements = [.. warningElementsSortParams.Select(x => new WarningElementViewModel() {
                    Element = x.RevitElement,
                })],
                FullName = "В проектах отсутствует параметр для сортировки",
                Description = "В указанных проектах отсутствует параметр для сортировки для некоторых элементов выбранной категории"
            };

            warningsWithNoParams.Warnings.Add(warning);
        }

        var warningElementsMarkParams = validation.CheckIsExistParam(selectedParam.RevitParam, filteredElements);
        if(warningElementsMarkParams.Any()) {
            var warning = new WarningViewModel() {
                Elements = [.. warningElementsMarkParams.Select(x => new WarningElementViewModel() {
                    Element = x.RevitElement,
                }).Take(100)],
                FullName = "В проектах отсутствует параметр для заполнения марки",
                Description = "В проектах отсутствует параметр для заполнения марки доступен"
            };

            warningsWithNoParams.Warnings.Add(warning);
        }

        if(_windowsService.ShowWarningsWindow(warningsWithNoParams)) {
            return;
        }

        var warningsWithReadonlyParams = new WarningsViewModel();

        var warningElementsReadonlyParams = validation.CheckIsReadonlyParam(selectedParam.RevitParam, filteredElements);
        if(warningElementsReadonlyParams.Any()) {
            var elementsToShow = warningElementsReadonlyParams.Select(x => new WarningElementViewModel() {
                    Element = x.RevitElement,
                });

            int elementsNumber = elementsToShow.Count();
            int maxShownElements = 100;
            string additionalMessage = string.Empty;
            if(elementsNumber > maxShownElements) {
                elementsToShow = elementsToShow.Take(maxShownElements);
                additionalMessage = $"Отображены {maxShownElements} из {elementsNumber} элементов";
            }

            var warning = new WarningViewModel() {
                Elements = [..elementsToShow],
                FullName = "В проектах параметр для заполнения марки доступен только для чтения.",
                Description = "В проектах параметр для заполнения марки доступен только для чтения." + additionalMessage
            };

            warningsWithReadonlyParams.Warnings.Add(warning);
        }

        if(_windowsService.ShowWarningsWindow(warningsWithReadonlyParams)) {
            return;
        }


        // sort and mark elements
        var startValue = MarkSettingsPageViewModel.GetStartValue();
        markData.CreateMarkValues(sortParams, startValue);

        string currentDocName = _documentService.GetDocumentFullName(_revitRepository.Document);

        if(markData.HasLinksForExport(currentDocName)) {
            string path = SelectFolder();
            string fullPath = path + "\\" + $"{currentDocName}.json";

            var jsonService = new JsonSerializerService();
            jsonService.ExportMarkData(fullPath, markData);
        }

        var markDataForCurrentDoc = markData.GetDataByDocument(currentDocName);
        if(markDataForCurrentDoc != null) {
            _windowsService.ShowMarkListWindow(markData, _revitRepository, _documentService, _localizationService);
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
