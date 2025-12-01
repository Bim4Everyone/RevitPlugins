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
    private readonly NameEditorService _nameEditorService;

    public FiltrationPageViewModel(
        ILocalizationService localization,
        FiltersRepository filtersRepo,
        NameEditorService nameEditorService) {
        _localization = localization ?? throw new ArgumentNullException(nameof(localization));
        _filtersRepo = filtersRepo ?? throw new ArgumentNullException(nameof(filtersRepo));
        _nameEditorService = nameEditorService ?? throw new ArgumentNullException(nameof(nameEditorService));
        // TODO
        Filters = [];
        AddFilterCommand = RelayCommand.Create(AddFilter);
        RenameFilterCommand = RelayCommand.Create<FilterViewModel>(RenameFilter, CanRenameFilter);
        RemoveFiltersCommand = RelayCommand.Create<IList>(RemoveFilters, CanRemoveFilters);
    }

    public ICommand LoadCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand SaveAsCommand { get; }
    public ICommand AddFilterCommand { get; }
    public ICommand RenameFilterCommand { get; }
    public ICommand CopyFilterCommand { get; }
    public ICommand RemoveFiltersCommand { get; }
    public ICommand ShowElementsCommand { get; }

    public ObservableCollection<FilterViewModel> Filters { get; }

    private void AddFilter() {
        try {
            var newName = _nameEditorService.CreateNewName(
                _localization.GetLocalizedString("FiltersPage.NewFilterPrompt"),
                Filters.Select(f => f.Name).ToArray());
            Filters.Add(new FilterViewModel() { Name = newName });
        } catch(OperationCanceledException) {
        }
        // TODO
    }

    private void RenameFilter(FilterViewModel filter) {
        try {
            filter.Name = _nameEditorService.CreateNewName(
                _localization.GetLocalizedString("FiltersPage.RenameFilterPrompt"),
                Filters.Select(f => f.Name).ToArray(),
                filter.Name);
        } catch(OperationCanceledException) {
        }
        // TODO
    }

    private bool CanRenameFilter(FilterViewModel filter) {
        return filter is not null;
    }

    private void RemoveFilters(IList items) {
        var filters = items.OfType<FilterViewModel>().ToArray();
        foreach(var filter in filters) {
            Filters.Remove(filter);
        }
        // TODO
    }

    private bool CanRemoveFilters(IList items) {
        return items != null
               && items.OfType<FilterViewModel>().Count() != 0;
    }
}
