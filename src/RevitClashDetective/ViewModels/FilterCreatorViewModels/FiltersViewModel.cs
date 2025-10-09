using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

using dosymep.Bim4Everyone;
using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitClashDetective.Models;
using RevitClashDetective.Models.FilterModel;
using RevitClashDetective.ViewModels.SearchSet;
using RevitClashDetective.ViewModels.Services;
using RevitClashDetective.Views;

namespace RevitClashDetective.ViewModels.FilterCreatorViewModels;
internal class FiltersViewModel : BaseViewModel {
    private readonly RevitRepository _revitRepository;
    private readonly FiltersConfig _config;
    private ObservableCollection<FilterViewModel> _filters;
    private string _errorText;
    private string _messageText;
    private DispatcherTimer _timer;
    private FilterViewModel _selectedFilter;

    public FiltersViewModel(
        RevitRepository revitRepository,
        IOpenFileDialogService openFileDialogService,
        ISaveFileDialogService saveFileDialogService,
        IMessageBoxService messageBoxService,
        FiltersConfig config) {

        _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
        OpenFileDialogService = openFileDialogService ?? throw new ArgumentNullException(nameof(openFileDialogService));
        SaveFileDialogService = saveFileDialogService ?? throw new ArgumentNullException(nameof(saveFileDialogService));
        MessageBoxService = messageBoxService ?? throw new ArgumentNullException(nameof(messageBoxService));
        _config = config ?? throw new ArgumentNullException(nameof(config));

        InitializeFilters();
        InitializeTimer();

        SelectedFilterChangedCommand = RelayCommand.Create(SelectedFilterChanged, CanSelectedFilterChanged);
        CreateCommand = RelayCommand.Create<Window>(Create);
        DeleteCommand = RelayCommand.Create(Delete, CanDelete);
        RenameCommand = RelayCommand.Create<Window>(Rename, CanRename);
        SaveCommand = RelayCommand.Create(Save, CanSave);
        SaveAsCommand = RelayCommand.Create(SaveAs, CanSave);
        LoadCommand = RelayCommand.Create(Load);
        CheckSearchSetCommand = RelayCommand.Create(CheckSearchSet, CanSave);

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
    public ICommand DeleteCommand { get; }
    public ICommand RenameCommand { get; }
    public ICommand SelectedFilterChangedCommand { get; }

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

    private void InitializeFilters() {
        Filters = new ObservableCollection<FilterViewModel>(InitializeFilters(_config));
    }

    private IEnumerable<FilterViewModel> InitializeFilters(FiltersConfig config) {
        foreach(var filter in config.Filters.OrderBy(item => item.Name)) {
            filter.RevitRepository = _revitRepository;
            filter.Set.SetRevitRepository(_revitRepository);
            yield return new FilterViewModel(_revitRepository, filter);
        }
    }

    private void SelectedFilterChanged() {
        SelectedFilter?.InitializeFilter();
    }

    private bool CanSelectedFilterChanged() {
        return SelectedFilter != null;
    }

    private void Create(Window p) {
        var newFilterName = new FilterNameViewModel(Filters.Select(f => f.Name));
        var view = new FilterNameView() { DataContext = newFilterName, Owner = p };
        if(view.ShowDialog() == true) {
            var newFilter = new FilterViewModel(_revitRepository) { Name = newFilterName.Name, IsInitialized = true };
            Filters.Add(newFilter);

            Filters = new ObservableCollection<FilterViewModel>(Filters.OrderBy(item => item.Name));
            SelectedFilter = newFilter;
        }
    }

    private void Delete() {
        if(MessageBoxService.Show($"Удалить фильтр \"{SelectedFilter.Name}\"?",
            "BIM",
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
        var newFilterName = new FilterNameViewModel(Filters.Select(f => f.Name), SelectedFilter.Name);
        var view = new FilterNameView() { DataContext = newFilterName, Owner = p };
        if(view.ShowDialog() == true) {
            SelectedFilter.Name = newFilterName.Name;
            Filters = new ObservableCollection<FilterViewModel>(Filters.OrderBy(item => item.Name));
            SelectedFilter = Filters.FirstOrDefault(item => item.Name.Equals(newFilterName.Name, StringComparison.CurrentCultureIgnoreCase));
        }
    }

    private bool CanRename(Window p) {
        return SelectedFilter != null;
    }

    private void Save() {
        string revitFilePath = Path.Combine(_revitRepository.GetObjectName(), _revitRepository.GetDocumentName());
        var filtersConfig = FiltersConfig.GetFiltersConfig(revitFilePath, _revitRepository.Doc);
        filtersConfig.Filters = GetFilters().ToList();
        filtersConfig.RevitVersion = ModuleEnvironment.RevitVersion;
        filtersConfig.SaveProjectConfig();
        MessageText = "Поисковые наборы успешно сохранены";
        RefreshMessage();
    }

    private void SaveAs() {
        string revitFilePath = Path.Combine(_revitRepository.GetObjectName(), _revitRepository.GetDocumentName());
        var filtersConfig = FiltersConfig.GetFiltersConfig(revitFilePath, _revitRepository.Doc);
        filtersConfig.Filters = GetFilters().ToList();
        filtersConfig.RevitVersion = ModuleEnvironment.RevitVersion;

        var cs = new ConfigSaverService(_revitRepository, SaveFileDialogService);
        cs.Save(filtersConfig);
        MessageText = "Поисковые наборы успешно сохранены";
        RefreshMessage();
    }

    private bool CanSave() {
        if(SelectedFilter == null || !SelectedFilter.IsInitialized) {
            return false;
        }

        if(Filters.Any(item => item.Set.IsEmpty())) {
            ErrorText = $"Все поля в поисковом наборе \"{Filters.FirstOrDefault(item => item.Set.IsEmpty())?.Name}\" должны быть заполнены.";
            return false;
        }

        ErrorText = Filters.FirstOrDefault(item => item.Set.GetErrorText() != null)?.Set?.GetErrorText();
        return string.IsNullOrEmpty(ErrorText);
    }

    private void Load() {
        var cl = new ConfigLoaderService(_revitRepository, OpenFileDialogService, MessageBoxService);
        var config = cl.Load<FiltersConfig>();
        cl.CheckConfig(config);

        var newFilters = InitializeFilters(config).ToList();
        var nameResolver = new NameResolver<FilterViewModel>(Filters, newFilters);
        Filters = new ObservableCollection<FilterViewModel>(nameResolver.GetCollection());
        MessageText = "Файл поисковых наборов успешно загружен";
        RefreshMessage();
    }

    private void CheckSearchSet() {
        Save();
        var filter = SelectedFilter.GetFilter();
        var vm = new SearchSetsViewModel(_revitRepository, filter, MessageBoxService);
        var view = new SearchSetView() { DataContext = vm };
        view.Show();
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
