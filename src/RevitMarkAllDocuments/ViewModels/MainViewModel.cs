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

using Bim4Everyone.RevitFiltration.Serialization;

using Microsoft.WindowsAPICodePack.Dialogs;

using RevitMarkAllDocuments.Models;
using RevitMarkAllDocuments.Models.Export;
using RevitMarkAllDocuments.Services;

namespace RevitMarkAllDocuments.ViewModels;

internal class MainViewModel : BaseViewModel {
    private readonly PluginConfig _pluginConfig;
    private readonly RevitRepository _revitRepository;
    private readonly DocumentService _documentService;
    private readonly ParamValidationService _paramValidationService;
    private readonly WindowsService _windowsService;
    private readonly ILogicalFilterProviderFactory _filterFactory;
    private readonly IFilterContextParser _filterParser;
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
                         IFilterContextParser filterParser,
                         ILocalizationService localizationService) {        
        _pluginConfig = pluginConfig;
        _revitRepository = revitRepository;
        _documentService = documentService;
        _paramValidationService = paramValidationService;
        _windowsService = markListWindowService;
        _filterFactory = filterFactory;
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
        var paramsForFilterAndSort = paramProvider.GetParamsForFilterAndSort();
        var paramsForMark = paramProvider.GetParamsForMarks();

        _documentsPageViewModel = new DocumentsPageViewModel(_revitRepository);
        var dataProvider = new FilterDataProvider(_revitRepository.Document, _selectedCategory, paramsForFilterAndSort);
        _filterPageViewModel = new FilterPageViewModel(_filterFactory, dataProvider, _localizationService);
        _sortPageViewModel = new SortPageViewModel(paramsForFilterAndSort);
        _markSettingsPageViewModel = new MarkSettingsPageViewModel(paramsForMark);

        LoadConfig(paramsForFilterAndSort);
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
        var filtrationService = new FiltrationService(_isMarkForTypes);
        markData = filtrationService.FilterElements(markData, checkedDocuments, FilterPageViewModel.FilterProvider);

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
            string fullPath = path + "\\" + $"{currentDocName}.json";

            var jsonService = new JsonSerializerService();
            jsonService.ExportMarkData(fullPath, markData);
        }

        // Если выбран текущий документ, то заполняем значения марок в нём
        var markDataForCurrentDoc = markData.GetDataByDocument(currentDocName);
        if(markDataForCurrentDoc != null) {
            _windowsService.ShowMarkListWindow(markData, _revitRepository, _documentService, _localizationService);
        }

        SaveConfig();
    }

    /// <summary>
    /// Проверяет существуют ли выбранные параметры у всех элементов во всех документах.
    /// </summary>
    /// <returns> true если найдены ошибки</returns>
    public bool CheckAreExistParameters(MarkData markData, List<FilterableParam> sortParams, FilterableParam markParam) {
        var filteredElements = markData.GetAllElements();
        var warningsWithNoParams = new WarningsViewModel();

        var warningElementsSortParams = _paramValidationService
            .CheckAreExistParams(_isMarkForTypes, sortParams, filteredElements);

        if(warningElementsSortParams.Any()) {
            var warning = new WarningViewModel() {
                Elements = [.. warningElementsSortParams.Select(x => new WarningElementViewModel(x))],
                FullName = "В проектах отсутствует параметр для сортировки",
                Description = "В указанных проектах отсутствует параметр для сортировки для некоторых элементов выбранной категории"
            };

            warningsWithNoParams.Warnings.Add(warning);
        }

        var warningElementsMarkParams = _paramValidationService
            .CheckIsExistParam(_isMarkForTypes, markParam, filteredElements);

        if(warningElementsMarkParams.Any()) {
            var warning = new WarningViewModel() {
                Elements = [.. warningElementsMarkParams.Select(x => new WarningElementViewModel(x)).Take(100)],
                FullName = "В проектах отсутствует параметр для заполнения марки",
                Description = "В проектах отсутствует параметр для заполнения марки доступен"
            };

            warningsWithNoParams.Warnings.Add(warning);
        }

        return _windowsService.ShowWarningsWindow(warningsWithNoParams);
    }

    /// <summary>
    /// Проверяет доступен ли параметр для записи у всех элементов во всех документах.
    /// </summary>
    /// <returns> true если найдены ошибки</returns>
    public bool CheckReadOnlyParameters(MarkData markData, FilterableParam markParam) {
        var warningsWithReadonlyParams = new WarningsViewModel();
        var filteredElements = markData.GetAllElements();

        var warningElementsReadonlyParams = _paramValidationService
            .CheckIsReadonlyParam(markParam, filteredElements);

        if(warningElementsReadonlyParams.Any()) {
            var elementsToShow = warningElementsReadonlyParams.Select(x => new WarningElementViewModel(x));

            int elementsNumber = elementsToShow.Count();
            int maxShownElements = 100;
            string additionalMessage = string.Empty;
            if(elementsNumber > maxShownElements) {
                elementsToShow = elementsToShow.Take(maxShownElements);
                additionalMessage = $"Отображены {maxShownElements} из {elementsNumber} элементов";
            }

            var warning = new WarningViewModel() {
                Elements = [.. elementsToShow],
                FullName = "В проектах параметр для заполнения марки доступен только для чтения.",
                Description = "В проектах параметр для заполнения марки доступен только для чтения." + additionalMessage
            };

            warningsWithReadonlyParams.Warnings.Add(warning);
        }

        return _windowsService.ShowWarningsWindow(warningsWithReadonlyParams);
    }

    private bool CanAcceptView() {
        if((bool) !DocumentsPageViewModel?.Documents.Any(d => d.IsChecked)) {
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
