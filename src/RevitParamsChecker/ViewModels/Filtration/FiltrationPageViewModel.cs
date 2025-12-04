using System;
using System.Collections;
using System.Collections.ObjectModel;
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

    public FiltrationPageViewModel(
        ILocalizationService localization,
        IOpenFileDialogService openFileDialogService,
        ISaveFileDialogService saveFileDialogService,
        FiltersRepository filtersRepo,
        FiltersConverter filtersConverter,
        NamesService namesService) {
        OpenFileDialogService = openFileDialogService ?? throw new ArgumentNullException(nameof(openFileDialogService));
        SaveFileDialogService = saveFileDialogService ?? throw new ArgumentNullException(nameof(saveFileDialogService));
        _localization = localization ?? throw new ArgumentNullException(nameof(localization));
        _filtersRepo = filtersRepo ?? throw new ArgumentNullException(nameof(filtersRepo));
        _filtersConverter = filtersConverter ?? throw new ArgumentNullException(nameof(filtersConverter));
        _namesService = namesService ?? throw new ArgumentNullException(nameof(namesService));

        Filters = [.._filtersRepo.GetFilters().Select(f => new FilterViewModel(f))];
        SelectedFilter = Filters.FirstOrDefault();
        AddFilterCommand = RelayCommand.Create(AddFilter);
        RenameFilterCommand = RelayCommand.Create<FilterViewModel>(RenameFilter, CanRenameFilter);
        RemoveFiltersCommand = RelayCommand.Create<IList>(RemoveFilters, CanRemoveFilters);
        CopyFilterCommand = RelayCommand.Create<FilterViewModel>(CopyFilter, CanCopyFilter);
        SaveCommand = RelayCommand.Create(Save, CanSave);
        SaveAsCommand = RelayCommand.Create(SaveAs, CanSave);
        LoadCommand = RelayCommand.Create(Load);
        ShowElementsCommand = RelayCommand.Create(ShowElements, CanShowElements);
    }

    public ICommand LoadCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand SaveAsCommand { get; }
    public ICommand AddFilterCommand { get; }
    public ICommand RenameFilterCommand { get; }
    public ICommand CopyFilterCommand { get; }
    public ICommand RemoveFiltersCommand { get; }
    public ICommand ShowElementsCommand { get; }
    public IOpenFileDialogService OpenFileDialogService { get; }
    public ISaveFileDialogService SaveFileDialogService { get; }

    public ObservableCollection<FilterViewModel> Filters { get; }

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
            Filters.Add(vm);
            SelectedFilter = vm;
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
            Filters.Add(vm);
            SelectedFilter = vm;
        } catch(OperationCanceledException) {
        }
    }

    private bool CanCopyFilter(FilterViewModel filter) {
        return filter is not null; // TODO тут должна быть проверка ошибок в модели копируемого фильтра
    }

    private void RemoveFilters(IList items) {
        var filters = items.OfType<FilterViewModel>().ToArray();
        foreach(var filter in filters) {
            Filters.Remove(filter);
        }

        SelectedFilter = Filters.FirstOrDefault();
    }

    private bool CanRemoveFilters(IList items) {
        return items != null
               && items.OfType<FilterViewModel>().Count() != 0;
    }

    private void Save() {
        _filtersRepo.SetFilters(Filters.Select(f => f.GetFilter()).ToArray());
    }

    private void SaveAs() {
        // TODO
    }

    private bool CanSave() {
        return true; // TODO здесь должна быть проверка на ошибки в моделях всех фильтров
    }

    private void Load() {
        // TODO
    }

    private void ShowElements() {
        // TODO
    }

    private bool CanShowElements() {
        return CanSave();
    }
}
