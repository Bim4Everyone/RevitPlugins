using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Input;

using Autodesk.Revit.DB;

using Bim4Everyone.RevitFiltration;
using Bim4Everyone.RevitFiltration.Controls;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitParamsChecker.Models.Filtration;
using RevitParamsChecker.Services;

namespace RevitParamsChecker.ViewModels.Filtration;

internal class FiltrationPageViewModel : BaseViewModel {
    private readonly ILocalizationService _localization;
    private readonly ILogicalFilterProviderFactory _filterProviderFactory;
    private readonly ILogicalFilterFactory _filterFactory;
    private readonly IFilterContextParser _filterContextParser;
    private readonly DataProvider _dataProvider;
    private readonly DataProvider _materialsDataProvider;
    private readonly FiltersRepository _filtersRepo;
    private readonly FiltersConverter _filtersConverter;
    private readonly NamesService _namesService;
    private FilterViewModel _selectedFilter;
    private string _dirPath;
    private bool _filtersModified;
    private string _errorText;

    public FiltrationPageViewModel(
        ILocalizationService localization,
        ILanguageService languageService,
        IOpenFileDialogService openFileDialogService,
        ISaveFileDialogService saveFileDialogService,
        IMessageBoxService messageBoxService,
        ILogicalFilterProviderFactory filterProviderFactory,
        ILogicalFilterFactory filterFactory,
        IFilterContextParser filterContextParser,
        FilterDataProvider dataProviderFactory,
        FiltersRepository filtersRepo,
        FiltersConverter filtersConverter,
        NamesService namesService) {
        LanguageService = languageService ?? throw new ArgumentNullException(nameof(languageService));
        OpenFileDialogService = openFileDialogService ?? throw new ArgumentNullException(nameof(openFileDialogService));
        SaveFileDialogService = saveFileDialogService ?? throw new ArgumentNullException(nameof(saveFileDialogService));
        MessageBoxService = messageBoxService ?? throw new ArgumentNullException(nameof(messageBoxService));
        _localization = localization ?? throw new ArgumentNullException(nameof(localization));
        _filterProviderFactory =
            filterProviderFactory ?? throw new ArgumentNullException(nameof(filterProviderFactory));
        _filterFactory = filterFactory ?? throw new ArgumentNullException(nameof(filterFactory));
        _filterContextParser = filterContextParser ?? throw new ArgumentNullException(nameof(filterContextParser));
        if(dataProviderFactory is null) {
            throw new ArgumentNullException(nameof(dataProviderFactory));
        }

        _dataProvider = dataProviderFactory.CreateDataProvider();
        _materialsDataProvider = dataProviderFactory.CreateMaterialsDataProvider();
        _filtersRepo = filtersRepo ?? throw new ArgumentNullException(nameof(filtersRepo));
        _filtersConverter = filtersConverter ?? throw new ArgumentNullException(nameof(filtersConverter));
        _namesService = namesService ?? throw new ArgumentNullException(nameof(namesService));
        _dirPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

        Filters = [.._filtersRepo.GetFilters().Select(GetFilterViewModel)];
        SelectedFilter = Filters.FirstOrDefault();
        AddFilterCommand = RelayCommand.Create(AddFilter);
        RenameFilterCommand = RelayCommand.Create<FilterViewModel>(RenameFilter, CanRenameFilter);
        RemoveFiltersCommand = RelayCommand.Create<IList>(RemoveFilters, CanRemoveFilters);
        CopyFilterCommand = RelayCommand.Create<FilterViewModel>(CopyFilter, CanCopyFilter);
        SaveCommand = RelayCommand.Create(Save, CanSave);
        ExportCommand = RelayCommand.Create(Export, CanSave);
        LoadCommand = RelayCommand.Create(Load);
        SubscribeToChanges(Filters);
    }

    public ICommand LoadCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand ExportCommand { get; }
    public ICommand AddFilterCommand { get; }
    public ICommand RenameFilterCommand { get; }
    public ICommand CopyFilterCommand { get; }
    public ICommand RemoveFiltersCommand { get; }
    public ILanguageService LanguageService { get; }
    public IOpenFileDialogService OpenFileDialogService { get; }
    public ISaveFileDialogService SaveFileDialogService { get; }
    public IMessageBoxService MessageBoxService { get; }
    public ILocalizationService LocalizationService => _localization;

    public ObservableCollection<FilterViewModel> Filters { get; }

    public bool FiltersModified {
        get => _filtersModified;
        set => RaiseAndSetIfChanged(ref _filtersModified, value);
    }

    public string ErrorText {
        get => _errorText;
        set => RaiseAndSetIfChanged(ref _errorText, value);
    }

    public FilterViewModel SelectedFilter {
        get => _selectedFilter;
        set => RaiseAndSetIfChanged(ref _selectedFilter, value);
    }

    private void AddFilter() {
        try {
            string newName = _namesService.CreateNewName(
                _localization.GetLocalizedString("FiltersPage.NewFilterPrompt"),
                Filters.Select(f => f.Name).ToArray());
            var vm = new FilterViewModel(
                newName,
                GetFilterProvider(),
                GetMaterialsFilterProvider(),
                _filterContextParser);
            vm.PropertyChanged += OnFilterChanged;
            Filters.Add(vm);
            SelectedFilter = vm;
            FiltersModified = true;
        } catch(OperationCanceledException) {
        }
    }

    private void RenameFilter(FilterViewModel filter) {
        try {
            filter.Name = _namesService.CreateNewName(
                _localization.GetLocalizedString("FiltersPage.RenameFilterPrompt"),
                Filters.Select(f => f.Name).ToArray(),
                filter.Name);
        } catch(OperationCanceledException) {
        }
    }

    private bool CanRenameFilter(FilterViewModel filter) {
        return filter is not null;
    }

    private void CopyFilter(FilterViewModel filter) {
        try {
            string copyName = _namesService.CreateNewName(
                _localization.GetLocalizedString("FiltersPage.NewFilterPrompt"),
                Filters.Select(f => f.Name).ToArray(),
                filter.Name);
            var copyContext = GetFilterProvider(_filterContextParser.Serialize(filter.FilterProvider.GetFilter()));
            var copyMaterialsContext = GetMaterialsFilterProvider(
                _filterContextParser.Serialize(filter.MaterialsFilterProvider.GetFilter()));
            var vm = new FilterViewModel(copyName, copyContext, copyMaterialsContext, _filterContextParser);
            vm.PropertyChanged += OnFilterChanged;
            Filters.Add(vm);
            SelectedFilter = vm;
            FiltersModified = true;
        } catch(OperationCanceledException) {
        }
    }

    private bool CanCopyFilter(FilterViewModel filter) {
        return filter is not null
               && filter.FilterProvider.CanGetFilter(out _)
               && filter.MaterialsFilterProvider.CanGetFilter(out _);
    }

    private void RemoveFilters(IList items) {
        var filters = items.OfType<FilterViewModel>().ToArray();
        foreach(var filter in filters) {
            filter.PropertyChanged -= OnFilterChanged;
            Filters.Remove(filter);
        }

        SelectedFilter = Filters.FirstOrDefault();
        FiltersModified = true;
    }

    private bool CanRemoveFilters(IList items) {
        return items != null
               && items.OfType<FilterViewModel>().Count() != 0;
    }

    private void Save() {
        _filtersRepo.SetFilters(GetFilterModels(Filters));
        foreach(var vm in Filters) {
            vm.AcceptChanges();
        }

        FiltersModified = false;
    }

    private void Export() {
        if(SaveFileDialogService.ShowDialog(
               _dirPath,
               _localization.GetLocalizedString("FiltrationPage.SaveFileDefaultName"))) {
            var filters = GetFilterModels(Filters);
            string str = _filtersConverter.ConvertToString(filters);
            File.WriteAllText(SaveFileDialogService.File.FullName, str);
            _dirPath = SaveFileDialogService.File.DirectoryName;
        }
    }

    private bool CanSave() {
        foreach(var vm in Filters) {
            if(!vm.FilterProvider.CanGetFilter(out var errors)) {
                ErrorText = $"{vm.Name}: "
                            + _localization.GetLocalizedString(
                                "FiltrationPage.Error.ElementsFilter",
                                errors.FirstOrDefault()?.Message ?? string.Empty);
                return false;
            }

            if(!vm.MaterialsFilterProvider.CanGetFilter(out var materialsErrors)) {
                ErrorText = $"{vm.Name}: "
                            + _localization.GetLocalizedString(
                                "FiltrationPage.Error.MaterialsFilter",
                                materialsErrors.FirstOrDefault()?.Message ?? string.Empty);
                return false;
            }
        }

        ErrorText = string.Empty;
        return true;
    }

    private Filter[] GetFilterModels(IEnumerable<FilterViewModel> filterViewModels) {
        return filterViewModels.Select(f => new Filter() {
                Name = f.Name,
                FilterContext = _filterContextParser.Serialize(f.FilterProvider.GetFilter()),
                MaterialsFilterContext = _filterContextParser.Serialize(f.MaterialsFilterProvider.GetFilter())
            })
            .ToArray();
    }

    private void Load() {
        if(OpenFileDialogService.ShowDialog(_dirPath)) {
            string str = File.ReadAllText(OpenFileDialogService.File.FullName);
            Filter[] filters;
            try {
                filters = _filtersConverter.ConvertFromString(str);
            } catch(InvalidOperationException) {
                MessageBoxService.Show(_localization.GetLocalizedString("FiltrationPage.Error.CannotLoadFilters"));
                return;
            }

            var vms = _namesService.GetResolvedCollection(
                    Filters.ToArray(),
                    filters.Select(f => {
                            var vm = GetFilterViewModel(f);
                            vm.PropertyChanged += OnFilterChanged;
                            return vm;
                        })
                        .ToArray())
                .OfType<FilterViewModel>();
            string selectedName = SelectedFilter?.Name;
            Filters.Clear();
            foreach(var vm in vms) {
                Filters.Add(vm);
            }

            FiltersModified = true;
            SelectedFilter = Filters.FirstOrDefault(f => f.Name.Equals(selectedName)) ?? Filters.FirstOrDefault();
            _dirPath = OpenFileDialogService.File.DirectoryName;
        }
    }

    private void SubscribeToChanges(ObservableCollection<FilterViewModel> filters) {
        foreach(var filter in filters) {
            filter.PropertyChanged += OnFilterChanged;
        }
    }

    private void OnFilterChanged(object sender, PropertyChangedEventArgs e) {
        if((sender is FilterViewModel filter)
           && filter.Modified) {
            FiltersModified = true;
        }
    }

    private FilterViewModel GetFilterViewModel(Filter filter) {
        return new FilterViewModel(
            filter.Name,
            GetFilterProvider(filter.FilterContext),
            GetMaterialsFilterProvider(filter.MaterialsFilterContext),
            _filterContextParser) { Modified = false };
    }

    private ILogicalFilterProvider GetFilterProvider(string serializedContext = "") {
        if(string.IsNullOrWhiteSpace(serializedContext)) {
            return _filterProviderFactory.Create(_dataProvider);
        }

        bool success = _filterContextParser.TryParse(serializedContext, out var context);
        if(success) {
            return _filterProviderFactory.Create(_dataProvider, context!);
        } else {
            return _filterProviderFactory.Create(_dataProvider);
        }
    }

    /// <summary>
    /// Создает провайдер фильтра по параметрам материалов элементов.
    /// </summary>
    /// <param name="serializedContext">Сериализованный контекст фильтра.</param>
    /// <remarks>
    /// Если контекст не задан, либо его не удалось прочитать, создается провайдер с пустым набором правил.
    /// </remarks>
    private ILogicalFilterProvider GetMaterialsFilterProvider(string serializedContext = "") {
        if(!string.IsNullOrWhiteSpace(serializedContext)
           && _filterContextParser.TryParse(serializedContext, out var context)) {
            return _filterProviderFactory.Create(_materialsDataProvider, context!);
        }

        return _filterProviderFactory.Create(
            _materialsDataProvider,
            _filterFactory.CreateAndFilter(),
            [BuiltInCategory.OST_Materials]);
    }
}
