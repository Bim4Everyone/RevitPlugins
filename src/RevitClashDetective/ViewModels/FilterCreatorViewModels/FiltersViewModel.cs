using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

using Bim4Everyone.RevitFiltration.Controls;

using dosymep.Bim4Everyone;
using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using Ninject;
using Ninject.Syntax;

using RevitClashDetective.Models;
using RevitClashDetective.Models.FilterModel;
using RevitClashDetective.Models.Filtration;
using RevitClashDetective.Resources;
using RevitClashDetective.ViewModels.Common;
using RevitClashDetective.ViewModels.SearchSet;
using RevitClashDetective.ViewModels.Services;
using RevitClashDetective.Views;
using RevitClashDetective.Views.Common;


namespace RevitClashDetective.ViewModels.FilterCreatorViewModels;

internal class FiltersViewModel : BaseViewModel, IWindowClosingHandler {
    private readonly RevitRepository _revitRepository;
    private readonly ILocalizationService _localization;
    private readonly IResolutionRoot _resolutionRoot;
    private readonly FiltersConfig _config;
    private readonly ILogicalFilterProviderFactory _filterProviderFactory;
    private readonly IFilterContextParser _filterContextParser;
    private readonly DataProvider _dataProvider;
    private readonly LegacyFilterConverter _legacyFilterConverter;
    private ObservableCollection<FilterViewModel> _filters;
    private string _errorText;
    private string _messageText;
    private DispatcherTimer _timer;
    private FilterViewModel _selectedFilter;

    public FiltersViewModel(
        RevitRepository revitRepository,
        ILocalizationService localization,
        ILanguageService languageService,
        IOpenFileDialogService openFileDialogService,
        ISaveFileDialogService saveFileDialogService,
        IMessageBoxService messageBoxService,
        IResolutionRoot resolutionRoot,
        FiltersConfig config,
        ILogicalFilterProviderFactory filterProviderFactory,
        IFilterContextParser filterContextParser,
        DataProvider dataProvider,
        LegacyFilterConverter legacyFilterConverter) {

        _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
        _localization = localization ?? throw new ArgumentNullException(nameof(localization));
        LanguageService = languageService ?? throw new ArgumentNullException(nameof(languageService));
        OpenFileDialogService = openFileDialogService ?? throw new ArgumentNullException(nameof(openFileDialogService));
        SaveFileDialogService = saveFileDialogService ?? throw new ArgumentNullException(nameof(saveFileDialogService));
        MessageBoxService = messageBoxService ?? throw new ArgumentNullException(nameof(messageBoxService));
        _resolutionRoot = resolutionRoot ?? throw new ArgumentNullException(nameof(resolutionRoot));
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _filterProviderFactory = filterProviderFactory
                                 ?? throw new ArgumentNullException(nameof(filterProviderFactory));
        _filterContextParser = filterContextParser ?? throw new ArgumentNullException(nameof(filterContextParser));
        _dataProvider = dataProvider ?? throw new ArgumentNullException(nameof(dataProvider));
        _legacyFilterConverter = legacyFilterConverter
                                 ?? throw new ArgumentNullException(nameof(legacyFilterConverter));

        InitializeFilters();
        InitializeTimer();

        CreateCommand = RelayCommand.Create(Create);
        DeleteCommand = RelayCommand.Create<IList>(Delete, CanDelete);
        RenameCommand = RelayCommand.Create(Rename, CanRename);
        CopyCommand = RelayCommand.Create(Copy, CanCopy);
        SaveCommand = RelayCommand.Create(Save, CanSave);
        SaveAsCommand = RelayCommand.Create(SaveAs, CanSave);
        LoadCommand = RelayCommand.Create(Load);
        CheckSearchSetCommand = RelayCommand.Create(CheckSearchSet, CanCheckSearchSet);

        SelectedFilter = Filters.FirstOrDefault();
    }

    public string ErrorText {
        get => _errorText;
        set => RaiseAndSetIfChanged(ref _errorText, value);
    }

    public string MessageText {
        get => _messageText;
        set => RaiseAndSetIfChanged(ref _messageText, value);
    }

    public ICommand CreateCommand { get; }
    public ICommand CopyCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand RenameCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand SaveAsCommand { get; }
    public ICommand LoadCommand { get; }
    public ICommand CheckSearchSetCommand { get; }
    public IOpenFileDialogService OpenFileDialogService { get; }
    public ISaveFileDialogService SaveFileDialogService { get; }
    public IMessageBoxService MessageBoxService { get; }

    /// <summary>
    /// Сервис для установки локализации в контроле фильтрации
    /// </summary>
    public ILanguageService LanguageService { get; }

    public FilterViewModel SelectedFilter {
        get => _selectedFilter;
        set => RaiseAndSetIfChanged(ref _selectedFilter, value);
    }

    public ObservableCollection<FilterViewModel> Filters {
        get => _filters;
        set => RaiseAndSetIfChanged(ref _filters, value);
    }

    public IEnumerable<FilterSettings> GetFilters() {
        return Filters.Select(item => new FilterSettings() {
            Name = item.Name,
            FilterContext = _filterContextParser.Serialize(item.FilterProvider.GetFilter())
        });
    }

    private void InitializeFilters() {
        Filters = new ObservableCollection<FilterViewModel>(InitializeFilters(_config));
    }

    private IEnumerable<FilterViewModel> InitializeFilters(FiltersConfig config) {
        var filters = (config.FilterSettings.Count > 0
                ? CreateFilters(config.FilterSettings)
                : CreateFilters(config.Filters))
            .OrderBy(item => item.Name)
            .ToArray();

        string[] notLoadedFilters = [
            .. filters
                .Where(item => !item.FilterProvider.CanGetFilter(out _))
                .Select(item => item.Name)
        ];

        if(notLoadedFilters.Length > 0) {
            MessageBoxService.Show(
                _localization.GetLocalizedString(
                    "FilterCreation.Validation.NotLoadedFilters",
                    string.Join("\n", notLoadedFilters)),
                _localization.GetLocalizedString("BIM"),
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }

        return filters;
    }

    /// <summary>
    /// Создает поисковые наборы из настроек Bim4Everyone.RevitFiltration
    /// </summary>
    private IEnumerable<FilterViewModel> CreateFilters(ICollection<FilterSettings> filterSettings) {
        foreach(var settings in filterSettings) {
            var provider = _filterContextParser.TryParse(settings.FilterContext, out var context)
                ? _filterProviderFactory.Create(_dataProvider, context!)
                : _filterProviderFactory.Create(_dataProvider);
            yield return new FilterViewModel(settings.Name, provider);
        }
    }

    /// <summary>
    /// Создает поисковые наборы из устаревших настроек предыдущих версий плагина
    /// </summary>
    private IEnumerable<FilterViewModel> CreateFilters(ICollection<Filter> legacyFilters) {
        foreach(var legacyFilter in legacyFilters) {
            yield return new FilterViewModel(
                legacyFilter.Name,
                _legacyFilterConverter.Convert(legacyFilter));
        }
    }

    private void Create() {
        string name = GetFilterName();
        if(name is null) {
            return;
        }

        var newFilter = new FilterViewModel(name, _filterProviderFactory.Create(_dataProvider));
        Filters.Add(newFilter);

        Filters = new ObservableCollection<FilterViewModel>(Filters.OrderBy(item => item.Name));
        SelectedFilter = newFilter;
    }

    /// <summary>
    /// Показывает окно ввода имени поискового набора
    /// </summary>
    /// <param name="currentName">Текущее имя поискового набора, если оно есть</param>
    /// <returns>Введенное имя или null, если пользователь отменил ввод</returns>
    private string GetFilterName(string currentName = null) {
        var nameViewModel = new EntityNameViewModel(_localization, Filters.Select(f => f.Name), currentName);
        var view = _resolutionRoot.Get<EntityNameView>();
        view.DataContext = nameViewModel;
        return view.ShowDialog() == true ? nameViewModel.Name : null;
    }

    private void Delete(IList selectedItems) {
        var filters = selectedItems.OfType<FilterViewModel>().ToArray();
        if(MessageBoxService.Show(
               _localization.GetLocalizedString(
                   "FilterCreation.DeleteFilterPrompt",
                   string.Join(", ", filters.Select(item => item.Name))),
            _localization.GetLocalizedString("BIM"),
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning,
            MessageBoxResult.No) == MessageBoxResult.Yes) {
            foreach(var item in filters) {
                Filters.Remove(item);
            }

            SelectedFilter = Filters.FirstOrDefault();
        }
    }

    private bool CanDelete(IList selectedItems) {
        return selectedItems != null
               && selectedItems.OfType<FilterViewModel>().Count() != 0;
    }

    private void Rename() {
        string name = GetFilterName(SelectedFilter.Name);
        if(name is null) {
            return;
        }

        SelectedFilter.Name = name;
        Filters = new ObservableCollection<FilterViewModel>(Filters.OrderBy(item => item.Name));
        SelectedFilter = Filters.FirstOrDefault(
            item => item.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase));
    }

    private bool CanRename() {
        return SelectedFilter is not null;
    }

    private void Copy() {
        string name = GetFilterName(SelectedFilter.Name);
        if(name is null) {
            return;
        }

        var newFilter = new FilterViewModel(name, CopyFilterProvider(SelectedFilter));
        Filters.Add(newFilter);

        Filters = new ObservableCollection<FilterViewModel>(Filters.OrderBy(item => item.Name));
        SelectedFilter = newFilter;
    }

    private bool CanCopy() {
        return SelectedFilter is not null && SelectedFilter.FilterProvider.CanGetFilter(out _);
    }

    private ILogicalFilterProvider CopyFilterProvider(FilterViewModel filter) {
        string filterContext = _filterContextParser.Serialize(filter.FilterProvider.GetFilter());
        return _filterContextParser.TryParse(filterContext, out var context)
            ? _filterProviderFactory.Create(_dataProvider, context!)
            : _filterProviderFactory.Create(_dataProvider);
    }

    private void Save() {
        var filtersConfig = GetFiltersConfig();
        filtersConfig.SaveProjectConfig();
        MessageText = _localization.GetLocalizedString("FilterCreation.SuccessSave");
        RefreshMessage();
    }

    private void SaveAs() {
        var filtersConfig = GetFiltersConfig();

        var cs = new ConfigSaverService(_revitRepository, SaveFileDialogService);
        cs.Save(filtersConfig);
        MessageText = _localization.GetLocalizedString("FilterCreation.SuccessSave");
        RefreshMessage();
    }

    /// <summary>
    /// Собирает конфиг для сохранения: поисковые наборы записываются в новые настройки,
    /// а в устаревшие настройки записывается заглушка, ломающая чтение конфига старыми версиями плагина.
    /// </summary>
    private FiltersConfig GetFiltersConfig() {
        string revitFilePath = Path.Combine(_revitRepository.GetObjectName(), _revitRepository.GetDocumentName());
        var filtersConfig = FiltersConfig.GetFiltersConfig(revitFilePath, _revitRepository.Doc);
        filtersConfig.FilterSettings = [.. GetFilters()];
        filtersConfig.Filters = LegacyConfigPoison.Create(_revitRepository);
        filtersConfig.RevitVersion = ModuleEnvironment.RevitVersion;
        return filtersConfig;
    }

    private bool CanSave() {
        foreach(var filter in Filters) {
            if(!filter.FilterProvider.CanGetFilter(out var errors)) {
                ErrorText = $"{filter.Name}: {errors.FirstOrDefault()?.Message}";
                return false;
            }
        }
        ErrorText = null;
        return true;
    }

    private void Load() {
        var cl = new ConfigLoaderService(_revitRepository, _localization, OpenFileDialogService, MessageBoxService);
        var config = cl.Load<FiltersConfig>();
        cl.CheckConfig(config);

        var newFilters = InitializeFilters(config).ToList();
        var nameResolver = new NameResolver<FilterViewModel>(Filters, newFilters);
        Filters = new ObservableCollection<FilterViewModel>(nameResolver.GetCollection());
        MessageText = _localization.GetLocalizedString("FilterCreation.SuccessLoad");
        RefreshMessage();
    }

    private void CheckSearchSet() {
        Save();
        var vm = new SearchSetsViewModel(
            _revitRepository,
            _localization,
            SelectedFilter.Name,
            SelectedFilter.FilterProvider.GetFilter(),
            MessageBoxService);
        var view = new RevitClashDetective.Views.Filters.SearchSetView() { DataContext = vm };
        view.Show();
    }

    private bool CanCheckSearchSet() {
        return SelectedFilter is not null && CanSave();
    }

    private void InitializeTimer() {
        _timer = new DispatcherTimer {
            Interval = new TimeSpan(0, 0, 0, 3)
        };
        _timer.Tick += (s, a) => { MessageText = null; _timer.Stop(); };
    }

    private void RefreshMessage() {
        _timer.Start();
    }

    public void OnWindowClosing(CancelEventArgs e) {
        if(SaveCommand.CanExecute(default)
           && MessageBoxService.Show(
               _localization.GetLocalizedString("FilterCreation.SavePrompt"),
               _localization.GetLocalizedString("BIM"),
               MessageBoxButton.YesNo,
               MessageBoxImage.Question)
           == MessageBoxResult.Yes) {
            SaveCommand.Execute(default);
        } else if(!SaveCommand.CanExecute(default)
                  && MessageBoxService.Show(
                      _localization.GetLocalizedString("FilterCreation.CannotSavePrompt"),
                      _localization.GetLocalizedString("BIM"),
                      MessageBoxButton.OKCancel,
                      MessageBoxImage.Warning)
                  == MessageBoxResult.Cancel) {
            e.Cancel = true;
        }
    }
}
