using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Input;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitParamsChecker.Models.Filtration;
using RevitParamsChecker.Services;

namespace RevitParamsChecker.ViewModels.Filtration;

internal class FiltrationPageViewModel : BaseViewModel {
    private readonly ILocalizationService _localization;
    private readonly FiltersRepository _filtersRepo;
    private readonly FiltersConverter _filtersConverter;
    private readonly NamesService _namesService;
    private FilterViewModel _selectedFilter;
    private string _dirPath;
    private bool _filtersModified;

    public FiltrationPageViewModel(
        ILocalizationService localization,
        IOpenFileDialogService openFileDialogService,
        ISaveFileDialogService saveFileDialogService,
        IMessageBoxService messageBoxService,
        FiltersRepository filtersRepo,
        FiltersConverter filtersConverter,
        NamesService namesService) {
        OpenFileDialogService = openFileDialogService ?? throw new ArgumentNullException(nameof(openFileDialogService));
        SaveFileDialogService = saveFileDialogService ?? throw new ArgumentNullException(nameof(saveFileDialogService));
        MessageBoxService = messageBoxService ?? throw new ArgumentNullException(nameof(messageBoxService));
        _localization = localization ?? throw new ArgumentNullException(nameof(localization));
        _filtersRepo = filtersRepo ?? throw new ArgumentNullException(nameof(filtersRepo));
        _filtersConverter = filtersConverter ?? throw new ArgumentNullException(nameof(filtersConverter));
        _namesService = namesService ?? throw new ArgumentNullException(nameof(namesService));
        _dirPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

        Filters = [.._filtersRepo.GetFilters().Select(f => new FilterViewModel(f) { Modified = false })];
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
    public IOpenFileDialogService OpenFileDialogService { get; }
    public ISaveFileDialogService SaveFileDialogService { get; }
    public IMessageBoxService MessageBoxService { get; }

    public ObservableCollection<FilterViewModel> Filters { get; }

    public bool FiltersModified {
        get => _filtersModified;
        set => RaiseAndSetIfChanged(ref _filtersModified, value);
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
            var filter = new Filter() { Name = newName };
            var vm = new FilterViewModel(filter);
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
            var copyFilter = filter.GetFilter().Copy();
            copyFilter.Name = _namesService.CreateNewName(
                _localization.GetLocalizedString("FiltersPage.NewFilterPrompt"),
                Filters.Select(f => f.Name).ToArray(),
                filter.Name);
            var vm = new FilterViewModel(copyFilter);
            vm.PropertyChanged += OnFilterChanged;
            Filters.Add(vm);
            SelectedFilter = vm;
            FiltersModified = true;
        } catch(OperationCanceledException) {
        }
    }

    private bool CanCopyFilter(FilterViewModel filter) {
        return filter is not null; // TODO тут должна быть проверка ошибок в модели копируемого фильтра
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
        _filtersRepo.SetFilters(Filters.Select(f => f.GetFilter()).ToArray());
        foreach(var vm in Filters) {
            vm.Modified = false;
        }

        FiltersModified = false;
    }

    private void Export() {
        if(SaveFileDialogService.ShowDialog(
               _dirPath,
               _localization.GetLocalizedString("FiltrationPage.SaveFileDefaultName"))) {
            var filters = Filters.Select(f => f.GetFilter()).ToArray();
            string str = _filtersConverter.ConvertToString(filters);
            File.WriteAllText(SaveFileDialogService.File.FullName, str);
            _dirPath = SaveFileDialogService.File.DirectoryName;
        }
    }

    private bool CanSave() {
        return true; // TODO здесь должна быть проверка на ошибки в моделях всех фильтров
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
                            var vm = new FilterViewModel(f);
                            vm.PropertyChanged += OnFilterChanged;
                            return vm;
                        })
                        .ToArray())
                .OfType<FilterViewModel>();
            string selectedName = SelectedFilter.Name;
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
}
