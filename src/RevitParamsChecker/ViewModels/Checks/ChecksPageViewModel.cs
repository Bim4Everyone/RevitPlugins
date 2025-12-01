using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitParamsChecker.Models.Checks;
using RevitParamsChecker.Services;

namespace RevitParamsChecker.ViewModels.Checks;

internal class ChecksPageViewModel : BaseViewModel {
    private readonly ILocalizationService _localization;
    private readonly ChecksRepository _checksRepo;
    private readonly NameEditorService _nameEditorService;
    private bool _allSelected;

    public ChecksPageViewModel(
        ILocalizationService localization,
        ChecksRepository checksRepo,
        NameEditorService nameEditorService) {
        _localization = localization ?? throw new ArgumentNullException(nameof(localization));
        _checksRepo = checksRepo ?? throw new ArgumentNullException(nameof(checksRepo));
        _nameEditorService = nameEditorService ?? throw new ArgumentNullException(nameof(nameEditorService));
        // TODO
        Checks = [];
        AddCheckCommand = RelayCommand.Create(AddCheck);
        RenameCheckCommand = RelayCommand.Create<CheckViewModel>(RenameCheck, CanRenameCheck);
        RemoveChecksCommand = RelayCommand.Create<IList>(RemoveChecks, CanRemoveChecks);
    }

    public ICommand LoadCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand SaveAsCommand { get; }
    public ICommand AddCheckCommand { get; }
    public ICommand RenameCheckCommand { get; }
    public ICommand RemoveChecksCommand { get; }
    public ObservableCollection<CheckViewModel> Checks { get; }

    public bool AllSelected {
        get => _allSelected;
        set => RaiseAndSetIfChanged(ref _allSelected, value);
    }

    private void AddCheck() {
        try {
            var newName = _nameEditorService.CreateNewName(
                _localization.GetLocalizedString("ChecksPage.NewCheckPrompt"),
                Checks.Select(f => f.Name).ToArray());
            Checks.Add(new CheckViewModel() { Name = newName });
        } catch(OperationCanceledException) {
        }
        // TODO
    }

    private void RenameCheck(CheckViewModel filter) {
        try {
            filter.Name = _nameEditorService.CreateNewName(
                _localization.GetLocalizedString("ChecksPage.RenameCheckPrompt"),
                Checks.Select(f => f.Name).ToArray(),
                filter.Name);
        } catch(OperationCanceledException) {
        }
        // TODO
    }

    private bool CanRenameCheck(CheckViewModel filter) {
        return filter is not null;
    }

    private void RemoveChecks(IList items) {
        var filters = items.OfType<CheckViewModel>().ToArray();
        foreach(var filter in filters) {
            Checks.Remove(filter);
        }
        // TODO
    }

    private bool CanRemoveChecks(IList items) {
        return items != null
               && items.OfType<CheckViewModel>().Count() != 0;
    }
}
