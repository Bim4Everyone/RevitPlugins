using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

using dosymep.Bim4Everyone;
using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using Ninject;
using Ninject.Syntax;

using RevitClashDetective.Models;
using RevitClashDetective.Models.FilterModel;
using RevitClashDetective.ViewModels.SearchSet;
using RevitClashDetective.ViewModels.Services;
using RevitClashDetective.Views;

namespace RevitClashDetective.ViewModels.FilterCreatorViewModels;
internal class FiltersViewModel : BaseViewModel {
    private readonly RevitRepository _revitRepository;
    private readonly ILocalizationService _localization;
    private readonly IResolutionRoot _resolutionRoot;
    private readonly FiltersConfig _config;
    private ObservableCollection<FilterViewModel> _filters;
    private string _errorText;
    private string _messageText;
    private DispatcherTimer _timer;
    private FilterViewModel _selectedFilter;

    public FiltersViewModel(
        RevitRepository revitRepository,
        ILocalizationService localization,
        IOpenFileDialogService openFileDialogService,
        ISaveFileDialogService saveFileDialogService,
        IMessageBoxService messageBoxService,
        IResolutionRoot resolutionRoot,
        FiltersConfig config) {

        _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
        _localization = localization ?? throw new ArgumentNullException(nameof(localization));
        OpenFileDialogService = openFileDialogService ?? throw new ArgumentNullException(nameof(openFileDialogService));
        SaveFileDialogService = saveFileDialogService ?? throw new ArgumentNullException(nameof(saveFileDialogService));
        MessageBoxService = messageBoxService ?? throw new ArgumentNullException(nameof(messageBoxService));
        _resolutionRoot = resolutionRoot ?? throw new ArgumentNullException(nameof(resolutionRoot));
        _config = config ?? throw new ArgumentNullException(nameof(config));

        InitializeFilters();
        InitializeTimer();

        CreateCommand = RelayCommand.Create<Window>(Create);
        DeleteCommand = RelayCommand.Create(Delete, CanDelete);
        RenameCommand = RelayCommand.Create<Window>(Rename, CanRename);
        SaveCommand = RelayCommand.Create(Save, CanSave);
        SaveAsCommand = RelayCommand.Create(SaveAs, CanSave);
        LoadCommand = RelayCommand.Create(Load);
        CheckSearchSetCommand = RelayCommand.Create(CheckSearchSet, CanSave);
        AskForSaveCommand = RelayCommand.Create(AskForSave);

        SelectedFilter = Filters.FirstOrDefault();
        SelectedFilter?.InitializeFilter();

        PropertyChanged += OnSelectedFilterChanged;
    }

    public string ErrorText {
        get => _errorText;
        set => RaiseAndSetIfChanged(ref _errorText, value);
    }

    public string MessageText {
        get => _messageText;
        set => RaiseAndSetIfChanged(ref _messageText, value);
    }

    public ICommand AskForSaveCommand { get; }
    public ICommand CreateCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand RenameCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand SaveAsCommand { get; }
    public ICommand LoadCommand { get; }
    public ICommand CheckSearchSetCommand { get; }
    public IOpenFileDialogService OpenFileDialogService { get; }
    public ISaveFileDialogService SaveFileDialogService { get; }
    public IMessageBoxService MessageBoxService { get; }


    public FilterViewModel SelectedFilter {
        get => _selectedFilter;
        set => RaiseAndSetIfChanged(ref _selectedFilter, value);
    }

    public ObservableCollection<FilterViewModel> Filters {
        get => _filters;
        set => RaiseAndSetIfChanged(ref _filters, value);
    }

    public IEnumerable<Filter> GetFilters() {
        return Filters.Select(item => item.GetFilter());
    }

    private void OnSelectedFilterChanged(object sender, PropertyChangedEventArgs e) {
        if(e.PropertyName == nameof(SelectedFilter)) {
            SelectedFilter?.InitializeFilter();
        }
    }

    private void InitializeFilters() {
        Filters = new ObservableCollection<FilterViewModel>(InitializeFilters(_config));
    }

    private IEnumerable<FilterViewModel> InitializeFilters(FiltersConfig config) {
        foreach(var filter in config.Filters.OrderBy(item => item.Name)) {
            filter.RevitRepository = _revitRepository;
            filter.Set.SetRevitRepository(_revitRepository);
            yield return new FilterViewModel(_revitRepository, _localization, filter);
        }
    }

    private void Create(Window p) {
        var newFilterName = new FilterNameViewModel(_localization, Filters.Select(f => f.Name));
        var view = _resolutionRoot.Get<FilterNameView>();
        view.DataContext = newFilterName;
        view.Owner = p;
        if(view.ShowDialog() == true) {
            var newFilter = new FilterViewModel(_revitRepository, _localization) { Name = newFilterName.Name, IsInitialized = true };
            Filters.Add(newFilter);

            Filters = new ObservableCollection<FilterViewModel>(Filters.OrderBy(item => item.Name));
            SelectedFilter = newFilter;
        }
    }

    private void Delete() {
        if(MessageBoxService.Show(
            _localization.GetLocalizedString("FilterCreation.DeleteFilterPrompt", SelectedFilter.Name),
            _localization.GetLocalizedString("BIM"),
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning,
            MessageBoxResult.No) == MessageBoxResult.Yes) {
            Filters.Remove(SelectedFilter);
            SelectedFilter = Filters.FirstOrDefault();
        }
    }

    private bool CanDelete() {
        return SelectedFilter != null;
    }

    private void Rename(Window p) {
        var newFilterName = new FilterNameViewModel(_localization, Filters.Select(f => f.Name), SelectedFilter.Name);
        var view = _resolutionRoot.Get<FilterNameView>();
        view.DataContext = newFilterName;
        view.Owner = p;
        if(view.ShowDialog() == true) {
            SelectedFilter.Name = newFilterName.Name;
            Filters = new ObservableCollection<FilterViewModel>(Filters.OrderBy(item => item.Name));
            SelectedFilter = Filters.FirstOrDefault(item => item.Name.Equals(newFilterName.Name, StringComparison.CurrentCultureIgnoreCase));
        }
    }

    private bool CanRename(Window p) {
        return p is not null && SelectedFilter is not null;
    }

    private void Save() {
        string revitFilePath = Path.Combine(_revitRepository.GetObjectName(), _revitRepository.GetDocumentName());
        var filtersConfig = FiltersConfig.GetFiltersConfig(revitFilePath, _revitRepository.Doc);
        filtersConfig.Filters = GetFilters().ToList();
        filtersConfig.RevitVersion = ModuleEnvironment.RevitVersion;
        filtersConfig.SaveProjectConfig();
        MessageText = _localization.GetLocalizedString("FilterCreation.SuccessSave");
        RefreshMessage();
    }

    private void SaveAs() {
        string revitFilePath = Path.Combine(_revitRepository.GetObjectName(), _revitRepository.GetDocumentName());
        var filtersConfig = FiltersConfig.GetFiltersConfig(revitFilePath, _revitRepository.Doc);
        filtersConfig.Filters = GetFilters().ToList();
        filtersConfig.RevitVersion = ModuleEnvironment.RevitVersion;

        var cs = new ConfigSaverService(_revitRepository, SaveFileDialogService);
        cs.Save(filtersConfig);
        MessageText = _localization.GetLocalizedString("FilterCreation.SuccessSave");
        RefreshMessage();
    }

    private bool CanSave() {
        var emptyCategoryFilter = Filters.FirstOrDefault(f => f.AllCategories.All(c => !c.IsSelected));
        if(emptyCategoryFilter is not null) {
            ErrorText = _localization.GetLocalizedString(
                "FilterCreation.Validation.SelectCategories", emptyCategoryFilter.Name);
            return false;
        }

        var emptyFilter = Filters.FirstOrDefault(f => f.Set.IsEmpty());
        if(emptyFilter is not null) {
            ErrorText = _localization.GetLocalizedString(
                "FilterCreation.Validation.EmptySet", emptyFilter.Name);
            return false;
        }

        string errorSetText = Filters.FirstOrDefault(s => !string.IsNullOrWhiteSpace(s.Set.GetErrorText()))
            ?.Set.GetErrorText();
        if(!string.IsNullOrWhiteSpace(errorSetText)) {
            ErrorText = errorSetText;
            return false;
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
        var filter = SelectedFilter.GetFilter();
        var vm = new SearchSetsViewModel(_revitRepository, _localization, filter, MessageBoxService);
        var view = new SearchSetView() { DataContext = vm };
        view.Show();
    }

    private void AskForSave() {
        if(SaveCommand.CanExecute(default)
           && MessageBoxService.Show(
               _localization.GetLocalizedString("Navigator.SavePrompt"),
               _localization.GetLocalizedString("BIM"),
               MessageBoxButton.YesNo,
               MessageBoxImage.Question)
           == MessageBoxResult.Yes) {
            SaveCommand.Execute(default);
        }
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
}
