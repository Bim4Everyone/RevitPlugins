using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Input;

using Autodesk.Revit.DB;

using Bim4Everyone.RevitFiltration.Controls;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using Microsoft.WindowsAPICodePack.Dialogs;

using RevitMarkAllDocuments.Models;
using RevitMarkAllDocuments.Services;
using RevitMarkAllDocuments.Services.Export;

namespace RevitMarkAllDocuments.ViewModels;

internal class MainViewModel : BaseViewModel {
    private readonly PluginConfig _pluginConfig;
    private readonly RevitRepository _revitRepository;
    private readonly DocumentService _documentService;
    private readonly ParamValidationService _paramValidationService;
    private readonly WindowsService _windowsService;
    private readonly ILogicalFilterProviderFactory _filterFactory;
    private readonly IFilterContextParser _filterParser;
    private readonly JsonSerializerService _jsonService;
    private readonly ILocalizationService _localizationService;
    private readonly bool _isMarkForTypes;
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
                         ParamValidationService paramValidationService,
                         WindowsService markListWindowService,
                         ILogicalFilterProviderFactory filterFactory,
                         JsonSerializerService jsonService,
                         IFilterContextParser filterParser,
                         ILocalizationService localizationService) {        
        _pluginConfig = pluginConfig;
        _revitRepository = revitRepository;
        _documentService = documentService;
        _paramValidationService = paramValidationService;
        _windowsService = markListWindowService;
        _filterFactory = filterFactory;
        _jsonService = jsonService;
        _filterParser = filterParser;
        _localizationService = localizationService;

        _isMarkForTypes = categoryContext.IsMarkForTypes;
        _selectedCategory = categoryContext.SelectedCategory;
        _selectedCategoryName = categoryContext.SelectedCategory.Name;

        LoadViewCommand = RelayCommand.Create(LoadView);
        AcceptViewCommand = RelayCommand.Create(AcceptView, CanAcceptView);
    }

    public ICommand LoadViewCommand { get; }
    
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
        var paramProvider = new ParamProvider(_revitRepository, _selectedCategory, _isMarkForTypes);
        var paramsForFilterAndSort = paramProvider.GetParamsForFilterAndSort().ToList();
        var paramsForMark = paramProvider.GetParamsForMarks().ToList();

        _documentsPageViewModel = new DocumentsPageViewModel(_revitRepository, _localizationService);
        var dataProvider = new FilterDataProvider(_revitRepository.Document, _selectedCategory, paramsForFilterAndSort);
        _filterPageViewModel = new FilterPageViewModel(_filterFactory, dataProvider, _localizationService);
        _sortPageViewModel = new SortPageViewModel(paramsForFilterAndSort);
        _markSettingsPageViewModel = new MarkSettingsPageViewModel(paramsForMark);

        LoadConfig(paramsForFilterAndSort);
    }

    private string SelectFolder() {
        var dialog = new CommonOpenFileDialog() {
            IsFolderPicker = true
        };

        if(dialog.ShowDialog() == CommonFileDialogResult.Ok) {
            return dialog.FileName;
        }

        return string.Empty;
    }

    private void AcceptView() {
        // Получение документов
        var checkedDocuments = DocumentsPageViewModel.Documents
            .Where(d => d.IsChecked)
            .Select(d => d.Document)
            .ToArray();

        // Создание MarkData и фильтрация элементов
        var selectedMarkParam = MarkSettingsPageViewModel.SelectedParam.FilterableParam;
        var markData = new MarkData() {
            MarkRevitParam = selectedMarkParam.RevitParam,
        };
        var logicalFilterContext = FilterPageViewModel.FilterProvider.GetFilter();
        var filtrationService = new FiltrationService(_isMarkForTypes, 
                                                      _selectedCategory, 
                                                      _documentService,
                                                      logicalFilterContext);
        markData = filtrationService.FilterElements(markData, checkedDocuments);

        var sortParams = SortPageViewModel.SelectedParams
            .Select(x => x.FilterableParam)
            .ToList();

        // Проверка на наличие параметров и возможности записать значения в параметры
        if(CheckAreExistParameters(markData, sortParams, selectedMarkParam)) {
            return;
        } 
        if(CheckReadOnlyParameters(markData, selectedMarkParam)) {
            return;
        }

        // Создание значений для марок на основе отсортированного списка элементов.
        var startValue = MarkSettingsPageViewModel.GetStartValue();
        markData.CreateMarkValues(_isMarkForTypes, sortParams, startValue);

        string currentDocName = _documentService.GetDocumentFullName(_revitRepository.Document);

        // Если есть связанные документы, то экспортируем информацию про марки в json
        if(markData.HasLinksForExport(currentDocName)) {
            string path = SelectFolder();
            if(string.IsNullOrEmpty(path)) {
                return;
            }
            string fullPath = Path.Combine(path, $"{currentDocName}_{_selectedCategoryName}.json");

            _jsonService.ExportMarkData(fullPath, markData);
        }

        // Если выбран текущий документ, то заполняем значения марок в нём
        var markDataForCurrentDoc = markData.GetDataByDocument(currentDocName);
        if(markDataForCurrentDoc != null) {
            _windowsService.ShowMarkListWindow(markDataForCurrentDoc, markData.MarkRevitParam);
        }

        SaveConfig();
    }

    /// <summary>
    /// Проверяет существуют ли выбранные параметры у всех элементов во всех документах.
    /// </summary>
    /// <returns> true если найдены ошибки</returns>
    private bool CheckAreExistParameters(MarkData markData, List<FilterableParam> sortParams, FilterableParam markParam) {
        var filteredElements = markData.GetAllElements();
        var warningsWithNoParams = new WarningsViewModel();

        var warningElementsSortParams = _paramValidationService
            .CheckAreExistParams(_isMarkForTypes, sortParams, filteredElements);

        if(warningElementsSortParams.Any()) {
            var warning = new WarningViewModel() {
                Elements = [.. warningElementsSortParams.Select(x => new WarningElementViewModel(x))],
                FullName = _localizationService.GetLocalizedString("WarningsWindow.NoSortParam"),
                Description = _localizationService.GetLocalizedString("WarningsWindow.NoSortParamInfo")
            };

            warningsWithNoParams.Warnings.Add(warning);
        }

        var warningElementsMarkParams = _paramValidationService
            .CheckIsExistParam(_isMarkForTypes, markParam, filteredElements);

        if(warningElementsMarkParams.Any()) {
            var warning = new WarningViewModel() {
                Elements = [.. warningElementsMarkParams.Select(x => new WarningElementViewModel(x)).Take(100)],
                FullName = _localizationService.GetLocalizedString("WarningsWindow.NoMarkParam"),
                Description = _localizationService.GetLocalizedString("WarningsWindow.NoMarkParamInfo")
            };

            warningsWithNoParams.Warnings.Add(warning);
        }

        return _windowsService.ShowWarningsWindow(warningsWithNoParams);
    }

    /// <summary>
    /// Проверяет доступен ли параметр для записи у всех элементов во всех документах.
    /// </summary>
    /// <returns> true если найдены ошибки</returns>
    private bool CheckReadOnlyParameters(MarkData markData, FilterableParam markParam) {
        var warningsWithReadonlyParams = new WarningsViewModel();
        var filteredElements = markData.GetAllElements();

        var warningElementsReadonlyParams = _paramValidationService
            .CheckIsReadonlyParam(markParam, filteredElements);

        if(warningElementsReadonlyParams.Any()) {
            var elementsToShow = warningElementsReadonlyParams
                .Select(x => new WarningElementViewModel(x))
                .ToList();

            int elementsNumber = elementsToShow.Count;
            int maxShownElements = 100;
            string additionalMessage = string.Empty;
            if(elementsNumber > maxShownElements) {
                elementsToShow = elementsToShow.Take(maxShownElements).ToList();
                additionalMessage = _localizationService.GetLocalizedString("WarningsWindow.ReadOnlyParamShownElements", maxShownElements, elementsNumber);
            }

            var warning = new WarningViewModel() {
                Elements = [.. elementsToShow],
                FullName = _localizationService.GetLocalizedString("WarningsWindow.ReadOnlyParam"),
                Description = _localizationService.GetLocalizedString("WarningsWindow.ReadOnlyParamInfo") + additionalMessage
            };

            warningsWithReadonlyParams.Warnings.Add(warning);
        }

        return _windowsService.ShowWarningsWindow(warningsWithReadonlyParams);
    }

    private bool CanAcceptView() {
        if(DocumentsPageViewModel?.Documents?.Any(d => d.IsChecked) != true) {
            ErrorText = _localizationService.GetLocalizedString("MainWindow.ErrorNoDocs");
            return false;
        }
        if(!SortPageViewModel.SelectedParams.Any()) {
            ErrorText = _localizationService.GetLocalizedString("MainWindow.ErrorNoSortParams");
            return false;
        }
        if(MarkSettingsPageViewModel.SelectedParam == null) {
            ErrorText = _localizationService.GetLocalizedString("MainWindow.ErrorNoMarkParam");
            return false;
        }
        if(string.IsNullOrEmpty(MarkSettingsPageViewModel.StartNumber)) {
            ErrorText = _localizationService.GetLocalizedString("MainWindow.ErrorNoStartValue");
            return false;
        }
        if(!int.TryParse(MarkSettingsPageViewModel.StartNumber, out int number)) {
            ErrorText = _localizationService.GetLocalizedString("MainWindow.ErrorStartValueIsNotInt");
            return false;
        }

        ErrorText = null;
        return true;
    }

    private void LoadConfig(List<FilterableParam> paramsForFilter) {
        RevitSettings settings = _pluginConfig.GetSettings(_revitRepository.Document);

        if(settings is null) {
            return;
        }

        if(settings.Category == _selectedCategoryName) {
            var documents = DocumentsPageViewModel.Documents
                .Where(x => settings.SelectedDocuments.Contains(x.Name));

            foreach(var document in documents) {
                document.IsChecked = true;
            }

            var sortParameters = SortPageViewModel.SelectableParams
                .Where(x => settings.SelectedSortParams.Contains(x.Name))
                .ToList();

            foreach(var parameter in sortParameters) {
                SortPageViewModel.SelectedParamFromSelectable = parameter;
                SortPageViewModel.AddParam();
            }

            if(_filterParser.TryParse(settings.FilterSettings, out var filter)) {
                var dataProvider = new FilterDataProvider(_revitRepository.Document, _selectedCategory, paramsForFilter);
                _filterPageViewModel = new FilterPageViewModel(_filterFactory, dataProvider, filter, _localizationService);
            }

            var selectedMarkParam = MarkSettingsPageViewModel.ParamsForMark
                .FirstOrDefault(x => x.Name == settings.MarkParam);

            MarkSettingsPageViewModel.SelectedParam = selectedMarkParam;
            MarkSettingsPageViewModel.StartNumber = settings.StartValue;
            MarkSettingsPageViewModel.Prefix = settings.Prefix;
            MarkSettingsPageViewModel.Suffix = settings.Suffix;
        }
    }

    private void SaveConfig() {
        RevitSettings settings = _pluginConfig.GetSettings(_revitRepository.Document)
                                ?? _pluginConfig.AddSettings(_revitRepository.Document);

        settings.Category = _selectedCategoryName;
        settings.SelectedDocuments = [.. DocumentsPageViewModel.Documents
            .Where(x => x.IsChecked)
            .Select(x => x.Name)];

        settings.SelectedSortParams = [.. SortPageViewModel.SelectedParams
            .Select(x => x.Name)];

        settings.FilterSettings = _filterParser.Serialize(FilterPageViewModel.FilterProvider.GetFilter());

        settings.MarkParam = MarkSettingsPageViewModel.SelectedParam.Name;
        settings.StartValue = MarkSettingsPageViewModel.StartNumber;
        settings.Prefix = MarkSettingsPageViewModel.Prefix;
        settings.Suffix = MarkSettingsPageViewModel.Suffix;

        _pluginConfig.SaveProjectConfig();
    }
}
